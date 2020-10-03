
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

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
			
			if( shaderCopy != null && materialCopy == null)
			{
				materialCopy = new Material( shaderCopy);
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
			for( int i0 = 0; i0 < opaqueProcesses.Length; ++i0)
			{
				opaqueProcesses[ i0]?.ClearPropertiesCache();
			}
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
			
			for( int i0 = 0; i0 < opaqueProcesses.Length; ++i0)
			{
				opaqueProcesses[ i0]?.Dispose();
			}
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
			if( materialColor != null)
			{
				ObjectUtility.Release( materialColor);
				materialColor = null;
			}
			if( materialCopy != null)
			{
				ObjectUtility.Release( materialCopy);
				materialCopy = null;
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
			if( shaderCopy != null && materialCopy == null)
			{
				materialCopy = new Material( shaderCopy);
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
			if( RestoreAndCheckParameter( opaqueProcesses, fourceCacheClear) != false)
			{
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
		List<IPostProcess> CollectionEnabledProcesses( IPostProcess[] processes, ref DepthTextureMode depthTextureMode, ref bool highDynamicRangeTarget)
		{
			var enabledProcesses = new List<IPostProcess>();
			IPostProcess process, prevProcess;
			int i0;
			
			for( i0 = 0, prevProcess = null; i0 < processes.Length; ++i0)
			{
				process = processes[ i0];
				
				if( (process?.Valid() ?? false) != false)
				{
					enabledProcesses.Add( process);
					depthTextureMode |= process.GetDepthTextureMode();
					
					if( process.IsHighDynamicRange() != false)
					{
						highDynamicRangeTarget = true;
					}
					prevProcess = process;
				}
			}
			return enabledProcesses;
		}
		int EnabledProcessCount( IPostProcess[] processes, ref DepthTextureMode depthTextureMode, ref bool highDynamicRangeTarget)
		{
			IPostProcess process;
			int i0, count = 0;
			
			for( i0 = 0; i0 < processes.Length; ++i0)
			{
				process = processes[ i0];
				
				if( (process?.Valid() ?? false) != false)
				{
					depthTextureMode |= process.GetDepthTextureMode();
					
					if( process.IsHighDynamicRange() != false)
					{
						highDynamicRangeTarget = true;
					}
					++count;
				}
			}
			return count;
		}
		void RebuildCommandBuffers()
		{
			var depthTextureMode = defaultDepthTextureMode;
			bool highDynamicRangeTarget = false;
			bool forceIntoRenderTexture = false;
			IPostProcess nextProcess;
			int i0;
			
			/* 既存のコマンドバッファを解放する */
			RemoveCommandBuffers();
			
			/* 有効なプロセスを収集する */
			List<IPostProcess> enabledOpaqueProcesses = CollectionEnabledProcesses( 
				opaqueProcesses, ref depthTextureMode, ref highDynamicRangeTarget);
			List<IPostProcess> enabledPostProcesses = CollectionEnabledProcesses( 
				postProcesses, ref depthTextureMode, ref highDynamicRangeTarget);
				
		#if true
			int enabledOpaqueProcessCount = enabledOpaqueProcesses.Count;
			int enabledProcessCount = enabledPostProcesses.Count;
		#else
			int enabledOpaqueProcessCount = EnabledProcessCount( 
				opaqueProcesses, ref depthTextureMode, ref highDynamicRangeTarget);
			int enabledProcessCount = EnabledProcessCount( 
				postProcesses, ref depthTextureMode, ref highDynamicRangeTarget);
		#endif
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
					
					var depthTexture = new RenderTargetIdentifier( kShaderPropertyDepthTextureId);
					commandBufferDepthTexture.GetTemporaryRT( kShaderPropertyDepthTextureId, 
						-1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.RFloat); //EDGE(WebGL)だとRFloatが使えない；
					Blit( commandBufferDepthTexture, depthBuffer, depthTexture);
					commandBufferDepthTexture.SetGlobalTexture( kShaderPropertyCameraDepthTexture, depthTexture);
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
				
				var usedTemporaries = new Dictionary<int, TemporaryTarget>();
				var recycleTemporaries = new Dictionary<int, TemporaryTarget>();
				var context = new TargetContext();
				int temporaryCount = 0;
				
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
				for( i0 = 0; i0 < enabledOpaqueProcesses.Count; ++i0)
				{
					foreach( var userdTemporary in usedTemporaries.Values)
					{
						if( context.ConfirmUsePropertyId( userdTemporary.propertyId) == false)
						{
							if( recycleTemporaries.ContainsKey( userdTemporary.propertyId) == false)
							{
								recycleTemporaries.Add( userdTemporary.propertyId, userdTemporary);
							}
						}
					}
					if( i0 == enabledOpaqueProcesses.Count - 1)
					{
						if( OverrideTargetBuffers == false)
						{
							context.SetTarget0( BuiltinRenderTextureType.CameraTarget);
						}
						else
						{
							context.SetTarget0( colorBuffer);
						}
						nextProcess = null;
					}
					else
					{
						nextProcess = enabledOpaqueProcesses[ i0 + 1];
					}
					enabledOpaqueProcesses[ i0].BuildCommandBuffer( this,
						commandBufferOpaqueProcesses, context, nextProcess,
						(width, height, depth, filterMode, format) =>
						{
							foreach( var recycleTemporary in recycleTemporaries.Values)
							{
								if( recycleTemporary.width == width
								||	recycleTemporary.height == height
								||	recycleTemporary.depth == depth
								||	recycleTemporary.filterMode == filterMode
								||	recycleTemporary.format == format)
								{
									recycleTemporaries.Remove( recycleTemporary.propertyId);
									return recycleTemporary.propertyId;
								}
							}
							int temporary = Shader.PropertyToID( "CameraPipeline::OpaqueTemporary" + temporaryCount);
							commandBufferOpaqueProcesses.GetTemporaryRT( temporary, width, height, depth, filterMode, format);
							usedTemporaries.Add( temporary, new TemporaryTarget( temporary, width, height, depth, filterMode, format));
							++temporaryCount;
							return temporary;
						});
					context.Next();
				}
				foreach( var userdTemporaryId in usedTemporaries.Keys)
				{
					commandBufferOpaqueProcesses.ReleaseTemporaryRT( userdTemporaryId);
				}
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
				
				if( enabledProcessCount > 0)
				{
					var usedTemporaries = new Dictionary<int, TemporaryTarget>();
					var recycleTemporaries = new Dictionary<int, TemporaryTarget>();
					var context = new TargetContext();
					int temporaryCount = 0;
					
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
					for( i0 = 0; i0 < enabledPostProcesses.Count; ++i0)
					{
						foreach( var userdTemporary in usedTemporaries.Values)
						{
							if( context.ConfirmUsePropertyId( userdTemporary.propertyId) == false)
							{
								if( recycleTemporaries.ContainsKey( userdTemporary.propertyId) == false)
								{
									recycleTemporaries.Add( userdTemporary.propertyId, userdTemporary);
								}
							}
						}
						if( i0 == enabledPostProcesses.Count - 1)
						{
							context.SetTarget0( BuiltinRenderTextureType.CameraTarget);
							nextProcess = null;
						}
						else
						{
							nextProcess = enabledPostProcesses[ i0 + 1];
						}
						enabledPostProcesses[ i0].BuildCommandBuffer( this,
							commandBufferPostProcesses, context, nextProcess,
							(width, height, depth, filterMode, format) =>
							{
								foreach( var recycleTemporary in recycleTemporaries.Values)
								{
									if( recycleTemporary.width == width
									||	recycleTemporary.height == height
									||	recycleTemporary.depth == depth
									||	recycleTemporary.filterMode == filterMode
									||	recycleTemporary.format == format)
									{
										recycleTemporaries.Remove( recycleTemporary.propertyId);
										return recycleTemporary.propertyId;
									}
								}
								int temporary = Shader.PropertyToID( "CameraPipeline::PostTemporary" + temporaryCount);
								commandBufferPostProcesses.GetTemporaryRT( temporary, width, height, depth, filterMode, format);
								usedTemporaries.Add( temporary, new TemporaryTarget( temporary, width, height, depth, filterMode, format));
								++temporaryCount;
								return temporary;
							});
						context.Next();
					}
					foreach( var userdTemporaryId in usedTemporaries.Keys)
					{
						commandBufferPostProcesses.ReleaseTemporaryRT( userdTemporaryId);
					}
				}
				else
				{
					Blit( commandBufferPostProcesses, colorBuffer, BuiltinRenderTextureType.CameraTarget);
				}
				if( (depthTextureMode & DepthTextureMode.Depth) != 0 && OverrideCameraDepthTexture != false)
				{
					commandBufferPostProcesses.ReleaseTemporaryRT( kShaderPropertyDepthTextureId);
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
			if( materialCopy != null)
			{
				commandBuffer.SetRenderTarget( 
					destination, 
					RenderBufferLoadAction.DontCare,
					RenderBufferStoreAction.Store,
					RenderBufferLoadAction.DontCare,
					RenderBufferStoreAction.DontCare);
				commandBuffer.SetGlobalTexture( kShaderPropertyMainTex, source);
				DrawFill( commandBuffer, materialCopy, 0);
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
		sealed class TemporaryTarget
		{
			public TemporaryTarget( int propertyId, int width, int height, int depth, FilterMode filterMode, RenderTextureFormat format)
			{
				this.propertyId = propertyId;
				this.width = width;
				this.height = height;
				this.depth = depth;
				this.filterMode = filterMode;
				this.format = format;
			}
			public int propertyId;
			public int width;
			public int height;
			public int depth;
			public FilterMode filterMode;
			public RenderTextureFormat format;
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
		static readonly int kShaderPropertyMainTex = Shader.PropertyToID( "_MainTex");
		static readonly int kShaderPropertyColor = Shader.PropertyToID( "_Color");
		static readonly int kShaderPropertyDepthTextureId = Shader.PropertyToID( "CameraPipeline::DepthTexture");
		static readonly int kShaderPropertyCameraDepthTexture = Shader.PropertyToID( "_CameraDepthTexture");
		
		const string kTipsOverrideTargetBuffers = 
			"レンダリングパスでUpdateDepthTextureが処理されていない状態でも_CameraDepthTextureに深度情報が書き込まれるようになります。\n\n" +
			"この機能を有効にした場合、UpdateDepthTextureで行われるDrawCallが無駄になるため、UpdateDepthTextureが発生しない様にする必要があります。\n\n" +
			"UpdateDepthTextureが発生しない様にするにはModeがRealtimeに設定されているLightのShadowTypeにNoShadowsが設定されている必要があります。\n\n" +
			"※この機能はUpdateDepthTextureで_CameraDepthTextureが利用可能になる場合と異なり、ForwardOpaque中に使用することが出来ません。\n\n" +
			"※ポストプロセスの使用状況によって_CameraDepthTextureに書き込まれない場合があるため、強制する場合は DefaultDepthTextureMode の Depth を有効にしてください。";
		
		[SerializeField]
		Shader shaderCopy = default;
		[SerializeField]
		DepthTextureMode defaultDepthTextureMode = default;
		[SerializeField, TooltipAttribute( kTipsOverrideTargetBuffers)]
		bool overrideTargetBuffers = false;
		[SerializeField]
		bool overrideCameraDepthTexture = true;
		
		[SerializeField]
		GameObject postProcessesTarget = default;
		
		Mesh fillMesh;
		Material materialCopy;
		Material materialColor;
		RenderTexture colorBuffer;
		RenderTexture depthBuffer;
		bool isRebuildCommandBuffers;
		
		IPostProcess[] opaqueProcesses = new IPostProcess[ kOpaqueProcesses.Length];
		IPostProcess[] postProcesses = new IPostProcess[ kPostProcesses.Length];
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
	