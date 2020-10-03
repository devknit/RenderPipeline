
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[RequireComponent( typeof( Camera))]
	public sealed partial class RenderPipeline : MonoBehaviour
	{
		void Awake()
		{
			cacheCamera = GetComponent<Camera>();
			fillMesh = new Mesh();
			fillMesh.SetVertices(
				new Vector3[]{
					new Vector3( 0, 0, 0),
					new Vector3( 0, 1, 0),
					new Vector3( 1, 1, 0),
					new Vector3( 1, 0, 0)
				});
			fillMesh.SetUVs( 
				0,
				new Vector2[]{
					new Vector2( 0, 0),
					new Vector2( 0, 1),
					new Vector2( 1, 1),
					new Vector2( 1, 0)
				});
			fillMesh.SetIndices(
				new int[]{ 0, 1, 2, 3 }, MeshTopology.Quads, 0, false);
			fillMesh.Optimize();
			fillMesh.UploadMeshData( true);
			
			if( copyShader != null && copyMaterial == null)
			{
				copyMaterial = new Material( copyShader);
			}
			CollectionProcesses();
			RebuildCommandBuffers();
		#if UNITY_EDITOR
			UnityEditor.EditorApplication.projectChanged += () =>
			{
				ClearPropertiesCache();
			};
		#endif
		}
	#if UNITY_EDITOR
		void OnDisable()
		{
			cacheCamera.SetTargetBuffers( Display.main.colorBuffer, Display.main.depthBuffer);
			cacheCamera.depthTextureMode = DepthTextureMode.None;
			cacheCamera.forceIntoRenderTexture = false;
			cacheCamera.allowHDR = false;
			cacheCamera.allowMSAA = false;
			
			RemoveCommandBuffers();
			ClearPropertiesCache();
		}
		void ClearPropertiesCache()
		{
			for( int i0 = 0; i0 < postProcesses.Length; ++i0)
			{
				postProcesses[ i0]?.ClearPropertiesCache();
			}
		}
	#endif
		void OnDestroy()
		{
		#if UNITY_EDITOR
			cacheCamera.SetTargetBuffers( Display.main.colorBuffer, Display.main.depthBuffer);
		#endif
			RemoveCommandBuffers();
			
			for( int i0 = 0; i0 < postProcesses.Length; ++i0)
			{
				postProcesses[ i0]?.Dispose();
			}
			if( colorBuffer != null)
			{
				colorBuffer.Release();
				colorBuffer = null;
			}
			if( depthBuffer != null)
			{
				depthBuffer.Release();
				depthBuffer = null;
			}
			if( copyMaterial != null)
			{
				ObjectUtility.Release( copyMaterial);
				copyMaterial = null;
			}
			if( fillMesh != null)
			{
				ObjectUtility.Release( fillMesh);
				fillMesh = null;
			}
		}
		void OnPreRender()
		{
			bool fourceCacheClear = false;
		#if UNITY_EDITOR
			if( Application.isPlaying == false && enabled == false)
			{
				return;
			}
			if( CollectionProcesses() != false)
			{
				isRebuildCommandBuffers = true;
				fourceCacheClear = true;
			}
			if( cacheOverrideTargetBuffers != OverrideTargetBuffers)
			{
				cacheOverrideTargetBuffers = OverrideTargetBuffers;
				isRebuildCommandBuffers = true;
			}
			if( cacheOverrideCameraDepthTexture != OverrideCameraDepthTexture)
			{
				cacheOverrideCameraDepthTexture = OverrideCameraDepthTexture;
				isRebuildCommandBuffers = true;
			}
			if( cacheDefaultDepthTextureMode != defaultDepthTextureMode)
			{
				cacheDefaultDepthTextureMode = defaultDepthTextureMode;
				isRebuildCommandBuffers = true;
			}
			if( copyShader != null && copyMaterial == null)
			{
				copyMaterial = new Material( copyShader);
			}
		#endif
			if( OverrideTargetBuffers != false)
			{
				if( cacheScreenWidth != Screen.width || cacheScreenHeight != Screen.height)
				{
					isRebuildCommandBuffers = true;
				}
			}
			if( cacheCamera.allowMSAA != false)
			{
				cacheCamera.allowMSAA = false;
				isRebuildCommandBuffers = true;
			}
			if( RestoreAndCheckParameter( postProcesses, fourceCacheClear) != false)
			{
				isRebuildCommandBuffers = true;
			}
			if( isRebuildCommandBuffers != false)
			{
				RebuildCommandBuffers();
			}
		}
		bool RestoreAndCheckParameter( IPostProcess[] processes, bool fourceCacheClear)
		{
			bool rebuild = false;
			
			for( int i0 = 0; i0 < processes.Length; ++i0)
			{
				bool cacheClear = fourceCacheClear;
				
				if( processes[ i0] is UbarProperty ubarProperty)
				{
					if( ubarProperty.Independent() == false)
					{
						continue;
					}
				}
			#if UNITY_EDITOR
				if( (processes[ i0]?.RestoreMaterials() ?? false) != false)
				{
					cacheClear = true;
				}
			#endif
				if( (processes[ i0]?.UpdateProperties( this, cacheClear) ?? false) != false)
				{
					rebuild = true;
				}
			}
			return rebuild;
		}
		void RemoveCommandBuffers()
		{
			if( commandBufferDepthTexture != null)
			{
				cacheCamera.RemoveCommandBuffer( CameraEvent.AfterForwardOpaque, commandBufferDepthTexture);
				commandBufferDepthTexture = null;
			}
			if( commandBufferOpaqueProcesses != null)
			{
				cacheCamera.RemoveCommandBuffer( CameraEvent.BeforeImageEffectsOpaque, commandBufferOpaqueProcesses);
				commandBufferOpaqueProcesses = null;
			}
			if( commandBufferPostProcesses != null)
			{
				cacheCamera.RemoveCommandBuffer( CameraEvent.BeforeImageEffects, commandBufferPostProcesses);
				commandBufferPostProcesses = null;
			}
		}
		int EnabledProcessCount( IPostProcess[] processes, CameraEvent cameraEvent, ref DepthTextureMode depthTextureMode, ref bool highDynamicRangeTarget)
		{
			IPostProcess process;
			int i0, count = 0;
			
			for( i0 = 0; i0 < processes.Length; ++i0)
			{
				process = processes[ i0];
				
				if( process != null)
				{
					if( process.GetCameraEvent() == cameraEvent && process.Valid() != false)
					{
						depthTextureMode |= process.GetDepthTextureMode();
						
						if( process.IsRequiredHighDynamicRange() != false)
						{
							highDynamicRangeTarget = true;
						}
						++count;
					}
				}
			}
			return count;
		}
		void RebuildCommandBuffers()
		{
			var depthTextureMode = defaultDepthTextureMode;
			bool highDynamicRangeTarget = false;
			bool forceIntoRenderTexture = false;
			IPostProcess nextProcess, process;
			int i0;
			
			/* 既存のコマンドバッファを解放する */
			RemoveCommandBuffers();
			
			/* 有効なプロセス数を求める */
			int enabledOpaqueProcessCount = EnabledProcessCount( 
				postProcesses, CameraEvent.BeforeImageEffectsOpaque, 
				ref depthTextureMode, ref highDynamicRangeTarget);
			int enabledProcessCount = EnabledProcessCount( 
				postProcesses, CameraEvent.BeforeImageEffects,
				ref depthTextureMode, ref highDynamicRangeTarget);
			
			/* [2019.4.1f1]
			   SetTargetBuffers の引数に Display.main.*****Buffer を渡しても実機では正しく動作しない。
			   エディタ上では動作し、SetTargetBuffers を呼び出す前と同じ状態に戻る。
			   実機では現状元に戻す方法が存在しないと思われる。
			   そのため以下の条件文はランタイム中に切り替わることは想定しない。
			 */
			if( OverrideTargetBuffers == false)
			{
				forceIntoRenderTexture = true;
			#if UNITY_EDITOR
				cacheCamera.SetTargetBuffers( Display.main.colorBuffer, Display.main.depthBuffer);
			#endif
			}
			else
			{
				bool refreshColorBuffer = colorBuffer == null || colorBuffer.width != Screen.width || colorBuffer.height != Screen.height;
				bool refreshDepthBuffer = depthBuffer == null || depthBuffer.width != Screen.width || depthBuffer.height != Screen.height;
				
				if( refreshColorBuffer != false || refreshDepthBuffer != false)
				{
					cacheCamera.SetTargetBuffers( Display.main.colorBuffer, Display.main.depthBuffer);
					
					if( refreshColorBuffer != false)
					{
						if( colorBuffer != null)
						{
							colorBuffer.Release();
						}
						var colorBufferFormat = RenderTextureFormat.ARGB32;
						
						if( SystemInfo.SupportsRenderTextureFormat( RenderTextureFormat.DefaultHDR) != false)
						{
							colorBufferFormat = RenderTextureFormat.DefaultHDR;
						}
						colorBuffer = new RenderTexture( Screen.width, Screen.height, 0, colorBufferFormat);
						colorBuffer.name = "CameraPipeline::ColorBuffer";
					}
					if( refreshDepthBuffer != false)
					{
						if( depthBuffer != null)
						{
							depthBuffer.Release();
						}
						depthBuffer = new RenderTexture( Screen.width, Screen.height, 24, RenderTextureFormat.Depth);
						depthBuffer.name = "CameraPipeline::DepthBuffer";
					}
				}
				cacheCamera.SetTargetBuffers( colorBuffer.colorBuffer, depthBuffer.depthBuffer);
				cacheScreenWidth = Screen.width;
				cacheScreenHeight = Screen.height;
				
				if( (depthTextureMode & DepthTextureMode.Depth) != 0 && OverrideCameraDepthTexture != false)
				{
					commandBufferDepthTexture = new CommandBuffer();
					commandBufferDepthTexture.name = "CameraPipeline::DepthTexture";
					commandBufferDepthTexture.Clear();
					commandBufferDepthTexture.SetProjectionMatrix( Matrix4x4.Ortho( 0, 1, 0, 1, 0, 1));
					commandBufferDepthTexture.SetViewMatrix( Matrix4x4.identity);
					
					RenderTextureFormat format = RenderTextureFormat.R8;
					
					//EDGE(WebGL)だとRFloatが使えない；
					if( SystemInfo.SupportsRenderTextureFormat( RenderTextureFormat.RHalf) != false)
					{
						format = RenderTextureFormat.RHalf;
					}
					if( SystemInfo.SupportsRenderTextureFormat( RenderTextureFormat.RFloat) != false)
					{
						format = RenderTextureFormat.RFloat;
					}
					var overrideDepthTexture = new RenderTargetIdentifier( ShaderProperty.OverrideDepthTexture);
					commandBufferDepthTexture.GetTemporaryRT( ShaderProperty.OverrideDepthTexture, 
						-1, -1, 0, FilterMode.Bilinear, format);
					Blit( commandBufferDepthTexture, depthBuffer, overrideDepthTexture);
					commandBufferDepthTexture.SetGlobalTexture( ShaderProperty.CameraDepthTexture, overrideDepthTexture);
					cacheCamera.AddCommandBuffer( CameraEvent.AfterForwardOpaque, commandBufferDepthTexture);
				}
			}
			/* 不透明系プロセス */
			if( enabledOpaqueProcessCount > 0)
			{
				commandBufferOpaqueProcesses = new CommandBuffer();
				commandBufferOpaqueProcesses.name = "CameraPipeline::OpaqueProcesses";
				commandBufferOpaqueProcesses.Clear();
				commandBufferOpaqueProcesses.SetProjectionMatrix( Matrix4x4.Ortho( 0, 1, 0, 1, 0, 1));
				commandBufferOpaqueProcesses.SetViewMatrix( Matrix4x4.identity);
				ResetTemporaryRT( commandBufferOpaqueProcesses);
				
				var context = new TargetContext();
				
				if( OverrideTargetBuffers == false)
				{
					context.SetBuffers();
					context.SetSource0( BuiltinRenderTextureType.CameraTarget);
					context.SetTarget0( BuiltinRenderTextureType.CameraTarget);
				}
				else
				{
					context.SetBuffers( colorBuffer, depthBuffer);
					context.SetSource0( colorBuffer);
					context.SetTarget0( colorBuffer);
				}
				for( i0 = 0, process = null; i0 < postProcesses.Length; ++i0)
				{
					nextProcess = postProcesses[ i0];
					
					if( nextProcess?.GetCameraEvent() != CameraEvent.BeforeImageEffectsOpaque)
					{
						continue;
					}
					if( nextProcess.Valid() != false)
					{
						if( nextProcess is UbarProperty ubarProperty)
						{
							if( ubarProperty.Independent() == false)
							{
								continue;
							}
						}
						if( process != null)
						{
							RecycleTemporaryRT( context);
							process.BuildCommandBuffer( this, commandBufferOpaqueProcesses, context, nextProcess);
							context.Next();
						}
						process = nextProcess;
					}
				}
				if( process != null)
				{
					RecycleTemporaryRT( context);
					
					if( OverrideTargetBuffers == false)
					{
						context.SetTarget0( BuiltinRenderTextureType.CameraTarget);
					}
					else
					{
						context.SetTarget0( colorBuffer);
					}
					process.BuildCommandBuffer( this, commandBufferOpaqueProcesses, context, null);
				}
				ReleaseTemporaryRT();
				cacheCamera.AddCommandBuffer( CameraEvent.BeforeImageEffectsOpaque, commandBufferOpaqueProcesses);
			}
			/* 半透明系プロセス+FinalPass */
			if( enabledProcessCount > 0 || OverrideTargetBuffers != false)
			{
				commandBufferPostProcesses = new CommandBuffer();
				commandBufferPostProcesses.name = "CameraPipeline::PostProcesses";
				commandBufferPostProcesses.Clear();
				commandBufferPostProcesses.SetProjectionMatrix( Matrix4x4.Ortho( 0, 1, 0, 1, 0, 1));
				commandBufferPostProcesses.SetViewMatrix( Matrix4x4.identity);
				ResetTemporaryRT( commandBufferPostProcesses);
				
				if( enabledProcessCount > 0)
				{
					var context = new TargetContext();
					
					if( OverrideTargetBuffers == false)
					{
						context.SetBuffers();
						context.SetSource0( BuiltinRenderTextureType.CameraTarget);
						context.SetTarget0( BuiltinRenderTextureType.CameraTarget);
					}
					else
					{
						context.SetBuffers( colorBuffer, depthBuffer);
						context.SetSource0( colorBuffer);
						context.SetTarget0( colorBuffer);
					}
					for( i0 = 0, process = null; i0 < postProcesses.Length; ++i0)
					{
						nextProcess = postProcesses[ i0];
						
						if( nextProcess?.GetCameraEvent() != CameraEvent.BeforeImageEffects)
						{
							continue;
						}
						if( nextProcess.Valid() != false)
						{
							if( nextProcess is UbarProperty ubarProperty)
							{
								if( ubarProperty.Independent() == false)
								{
									continue;
								}
							}
							if( process != null)
							{
								RecycleTemporaryRT( context);
								process.BuildCommandBuffer( this, commandBufferPostProcesses, context, nextProcess);
								context.Next();
							}
							process = nextProcess;
						}
					}
					if( process != null)
					{
						RecycleTemporaryRT( context);
						context.SetTarget0( BuiltinRenderTextureType.CameraTarget);
						process.BuildCommandBuffer( this, commandBufferPostProcesses, context, null);
					}
					ReleaseTemporaryRT();
				}
				else
				{
					Blit( commandBufferPostProcesses, colorBuffer, BuiltinRenderTextureType.CameraTarget);
				}
				if( (depthTextureMode & DepthTextureMode.Depth) != 0 && OverrideCameraDepthTexture != false)
				{
					commandBufferPostProcesses.ReleaseTemporaryRT( ShaderProperty.OverrideDepthTexture);
					depthTextureMode &= ~DepthTextureMode.Depth;
				}
				cacheCamera.AddCommandBuffer( CameraEvent.BeforeImageEffects, commandBufferPostProcesses);
			}
			cacheCamera.allowHDR = highDynamicRangeTarget;
			cacheCamera.depthTextureMode = depthTextureMode;
			cacheCamera.forceIntoRenderTexture = forceIntoRenderTexture;
			isRebuildCommandBuffers = false;
		}
		internal void Blit( CommandBuffer commandBuffer, RenderTargetIdentifier source, RenderTargetIdentifier destination)
		{
			if( copyMaterial != null)
			{
				commandBuffer.SetRenderTarget( 
					destination, 
					RenderBufferLoadAction.DontCare,
					RenderBufferStoreAction.Store,
					RenderBufferLoadAction.DontCare,
					RenderBufferStoreAction.DontCare);
				commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, source);
				DrawFill( commandBuffer, copyMaterial, 0);
			}
			else
			{
				commandBuffer.Blit( source, destination);
			}
		}
		internal void DrawFill( CommandBuffer commandBuffer, Material material, int shaderPass)
		{
			commandBuffer.DrawMesh( fillMesh, Matrix4x4.identity, material, 0, shaderPass);
		}
		internal Camera CacheCamera
		{
			get => cacheCamera;
		}
		internal bool OverrideTargetBuffers
		{
			get
			{
			#if UNITY_EDITOR
				if( Application.isPlaying == false)
				{
					return false;
				}
			#endif
				return overrideTargetBuffers;
			}
		}
		internal bool OverrideCameraDepthTexture
		{
			get
			{
				if( OverrideTargetBuffers != false)
				{
					return overrideCameraDepthTexture;
				}
				return false;
			}
		}
		
		const string kTipsOverrideTargetBuffers = 
			"レンダリングパスでUpdateDepthTextureが処理されていない状態でも_CameraDepthTextureに深度情報が書き込まれるようになります。\n\n" +
			"この機能を有効にした場合、UpdateDepthTextureで行われるDrawCallが無駄になるため、UpdateDepthTextureが発生しない様にする必要があります。\n\n" +
			"UpdateDepthTextureが発生しない様にするにはModeがRealtimeに設定されているLightのShadowTypeにNoShadowsが設定されている必要があります。\n\n" +
			"※この機能はUpdateDepthTextureで_CameraDepthTextureが利用可能になる場合と異なり、ForwardOpaque中に使用することが出来ません。\n\n" +
			"※ポストプロセスの使用状況によって_CameraDepthTextureに書き込まれない場合があるため、強制する場合は DefaultDepthTextureMode の Depth を有効にしてください。";
		
		[SerializeField]
		Shader copyShader = default;
		[SerializeField]
		Shader ubarShader = default;
		[SerializeField]
		DepthTextureMode defaultDepthTextureMode = default;
		[SerializeField, TooltipAttribute( kTipsOverrideTargetBuffers)]
		bool overrideTargetBuffers = false;
		[SerializeField]
		bool overrideCameraDepthTexture = true;
		
		[SerializeField]
		GameObject postProcessesTarget = default;
		
		Mesh fillMesh;
		Material copyMaterial;
		RenderTexture colorBuffer;
		RenderTexture depthBuffer;
		bool isRebuildCommandBuffers;
		
		IPostProcess[] postProcesses = new IPostProcess[ 2];
		CommandBuffer commandBufferDepthTexture;
		CommandBuffer commandBufferOpaqueProcesses;
		CommandBuffer commandBufferPostProcesses;
		
		Camera cacheCamera;
		int? cacheScreenWidth;
		int? cacheScreenHeight;
	#if UNITY_EDITOR
		DepthTextureMode? cacheDefaultDepthTextureMode;
		bool? cacheOverrideTargetBuffers;
		bool? cacheOverrideCameraDepthTexture;
	#endif
	}
}
	