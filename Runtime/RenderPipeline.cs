
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

//CameraPipeline
namespace RenderPipeline
{
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[RequireComponent( typeof( Camera))]
	public sealed class RenderPipeline : MonoBehaviour
	{
		const int kOpaquePriorityEdgeDetection = 0;
		const int kOpaquePrioritySSAO = 1;
		const int kOpaquePriorityMax = 2;
		
		public EdgeDetection EdgeDetection
		{
			get{ return opaqueProcesses[ kOpaquePriorityEdgeDetection] as EdgeDetection; }
			private set{ ApplyProcess( ref opaqueProcesses[ kOpaquePriorityEdgeDetection], value); }
		}
		public SSAO SSAO
		{
			get{ return opaqueProcesses[ kOpaquePrioritySSAO] as SSAO; }
			private set{ ApplyProcess( ref opaqueProcesses[ kOpaquePrioritySSAO], value); }
		}
		
		const int kPostPriorityBloom = 0;
		const int kPostPriorityDepthOfField = 1;
		const int kPostPriorityMosaic = 2;
		const int kPostPriorityFXAA = 3;
		const int kPostPriorityScreenBlend = 4;
		const int kPostPriorityMax = 5;
		
		public Bloom Bloom
		{
			get{ return postProcesses[ kPostPriorityBloom] as Bloom; }
			private set{ ApplyProcess( ref postProcesses[ kPostPriorityBloom], value); }
		}
		public DepthOfField DepthOfField
		{
			get{ return postProcesses[ kPostPriorityDepthOfField] as DepthOfField; }
			private set{ ApplyProcess( ref postProcesses[ kPostPriorityDepthOfField], value); }
		}
		public Mosaic Mosaic
		{
			get{ return postProcesses[ kPostPriorityMosaic] as Mosaic; }
			private set{ ApplyProcess( ref postProcesses[ kPostPriorityMosaic], value); }
		}
		public FXAA FXAA
		{
			get{ return postProcesses[ kPostPriorityFXAA] as FXAA; }
			private set{ ApplyProcess( ref postProcesses[ kPostPriorityFXAA], value); }
		}
		public ScreenBlend ScreenBlend
		{
			get{ return postProcesses[ kPostPriorityScreenBlend] as ScreenBlend; }
			private set{ ApplyProcess( ref postProcesses[ kPostPriorityScreenBlend], value); }
		}
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
		}
		void ApplyProcess( ref PostProcess prevProcess, PostProcess newProcess)
		{
			if( object.ReferenceEquals( prevProcess, null) == false)
			{
				prevProcess.Dispose();
			}
			if( newProcess != null)
			{
				newProcess.Initialize( this);
				newProcess.Create();
				newProcess.CheckParameterChange( true);
			}
			prevProcess = newProcess;
		}
		bool CollectionProcesses()
		{
			GameObject targetObject = (postProcessesTarget != null)? postProcessesTarget : gameObject;
			bool rebuild = false;
			
		#if UNITY_EDITOR
			if( (opaqueProcesses?.Length ?? 0) != kOpaquePriorityMax)
			{
				opaqueProcesses = new PostProcess[ kOpaquePriorityMax];
				rebuild = true;
			}
			if( (postProcesses?.Length ?? 0) != kPostPriorityMax)
			{
				postProcesses = new PostProcess[ kPostPriorityMax];
				rebuild = true;
			}
		#endif
			var edgeDetection = targetObject.GetComponent<EdgeDetection>() as EdgeDetection;
			if( ObjectUtility.IsMissing( edgeDetection) != false)
			{
				EdgeDetection = null;
				rebuild = true;
			}
			else if( EdgeDetection != edgeDetection)
			{
				EdgeDetection = edgeDetection;
				rebuild = true;
			}
			var ssao = targetObject.GetComponent<SSAO>() as SSAO;
			if( ObjectUtility.IsMissing( ssao) != false)
			{
				SSAO = null;
				rebuild = true;
			}
			else if( SSAO != ssao)
			{
				SSAO = ssao;
				rebuild = true;
			}
			var bloom = targetObject.GetComponent<Bloom>() as Bloom;
			if( ObjectUtility.IsMissing( bloom) != false)
			{
				Bloom = null;
				rebuild = true;
			}
			else if( Bloom != bloom)
			{
				Bloom = bloom;
				rebuild = true;
			}
			var depthOfField = targetObject.GetComponent<DepthOfField>() as DepthOfField;
			if( ObjectUtility.IsMissing( depthOfField) != false)
			{
				DepthOfField = null;
				rebuild = true;
			}
			else if( DepthOfField != depthOfField)
			{
				DepthOfField = depthOfField;
				rebuild = true;
			}
			var mosaic = targetObject.GetComponent<Mosaic>() as Mosaic;
			if( ObjectUtility.IsMissing( mosaic) != false)
			{
				Mosaic = null;
				rebuild = true;
			}
			else if( Mosaic != mosaic)
			{
				Mosaic = mosaic;
				rebuild = true;
			}
			var fxaa = targetObject.GetComponent<FXAA>() as FXAA;
			if( ObjectUtility.IsMissing( fxaa) != false)
			{
				FXAA = null;
				rebuild = true;
			}
			else if( FXAA != fxaa)
			{
				FXAA = fxaa;
				rebuild = true;
			}
			var screenBlend = targetObject.GetComponent<ScreenBlend>() as ScreenBlend;
			if( ObjectUtility.IsMissing( screenBlend) != false)
			{
				ScreenBlend = null;
				rebuild = true;
			}
			else if( ScreenBlend != screenBlend)
			{
				ScreenBlend = screenBlend;
				rebuild = true;
			}
			return rebuild;
		}
		void OnDisable()
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
			for( int i0 = 0; i0 < postProcesses.Length; ++i0)
			{
				postProcesses[ i0]?.ClearCache();
			}
			cacheCamera.allowHDR = false;
			cacheCamera.allowMSAA = false;
			cacheCamera.forceIntoRenderTexture = false;
			cacheCamera.depthTextureMode = DepthTextureMode.None;
		#if UNITY_EDITOR
			cacheCamera.SetTargetBuffers( Display.main.colorBuffer, Display.main.depthBuffer);
		#endif
		}
		void OnDestroy()
		{
		#if UNITY_EDITOR
			cacheCamera.SetTargetBuffers( Display.main.colorBuffer, Display.main.depthBuffer);
		#endif
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
		#endif
		#if UNITY_EDITOR
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
		#endif
			if( OverrideTargetBuffers != false)
			{
				if( cacheWidth != Screen.width
				||	cacheHeight != Screen.height)
				{
					isRebuildCommandBuffers = true;
				}
			}
			if( cacheCamera.allowMSAA != false)
			{
				cacheCamera.allowMSAA = false;
				isRebuildCommandBuffers = true;
			}
			for( int i0 = 0; i0 < postProcesses.Length; ++i0)
			{
				bool cacheClear = fourceCacheClear;
			#if UNITY_EDITOR
				if( postProcesses[ i0]?.RestoreResources() != false)
				{
					cacheClear = true;
				}
			#endif
				if( (postProcesses[ i0]?.CheckParameterChange( cacheClear) ?? false) != false)
				{
					isRebuildCommandBuffers = true;
				}
			}
			if( isRebuildCommandBuffers != false)
			{
				RebuildCommandBuffers();
			}
		}
		void RebuildCommandBuffers()
		{
			var enabledOpaqueProcesses = new List<PostProcess>();
			var enabledPostProcesses = new List<PostProcess>();
			var depthTextureMode = defaultDepthTextureMode;
			bool highDynamicRange = false;
			bool forceIntoRenderTexture = false;
			PostProcess process, prevProcess;
			int i0;
			
			/* 既存のコマンドバッファを解放する */
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
			
			/* 有効なプロセスを収集すると共にバッファの属性を求める */
			for( i0 = 0, prevProcess = null; i0 < opaqueProcesses.Length; ++i0)
			{
				process = opaqueProcesses[ i0];
				
				if( (process?.Valid() ?? false) != false)
				{
					enabledOpaqueProcesses.Add( process);
					depthTextureMode |= process.GetDepthTextureMode();
					
					if( process.IsHighDynamicRange() != false)
					{
						highDynamicRange = true;
					}
					if( prevProcess != null)
					{
						prevProcess.nextProcess = process;
					}
					prevProcess = process;
				}
			}
			if( prevProcess != null)
			{
				prevProcess.nextProcess = null;
			}
			for( i0 = 0, prevProcess = null; i0 < postProcesses.Length; ++i0)
			{
				process = postProcesses[ i0];
				
				if( (process?.Valid() ?? false) != false)
				{
					enabledPostProcesses.Add( process);
					depthTextureMode |= process.GetDepthTextureMode();
					
					if( process.IsHighDynamicRange() != false)
					{
						highDynamicRange = true;
					}
					if( prevProcess != null)
					{
						prevProcess.nextProcess = process;
					}
					prevProcess = process;
				}
			}
			if( prevProcess != null)
			{
				prevProcess.nextProcess = null;
			}
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
				cacheWidth = Screen.width;
				cacheHeight = Screen.height;
				
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
			if( enabledOpaqueProcesses.Count > 0)
			{
				commandBufferOpaqueProcesses = new CommandBuffer();
				commandBufferOpaqueProcesses.name = "CameraPipeline::PostProcesses";
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
					}
					enabledOpaqueProcesses[ i0].BuildCommandBuffer( 
						commandBufferOpaqueProcesses, context, 
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
							int temporary = Shader.PropertyToID( "CameraPipeline::Temporary" + temporaryCount);
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
			if( enabledPostProcesses.Count > 0 || OverrideTargetBuffers != false)
			{
				commandBufferPostProcesses = new CommandBuffer();
				commandBufferPostProcesses.name = "CameraPipeline::PostProcesses";
				commandBufferPostProcesses.Clear();
				commandBufferPostProcesses.SetProjectionMatrix( Matrix4x4.Ortho( 0, 1, 0, 1, 0, 1));
				commandBufferPostProcesses.SetViewMatrix( Matrix4x4.identity);
				
				if( enabledPostProcesses.Count > 0)
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
						}
						enabledPostProcesses[ i0].BuildCommandBuffer( 
							commandBufferPostProcesses, context, 
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
								int temporary = Shader.PropertyToID( "CameraPipeline::Temporary" + temporaryCount);
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
			cacheCamera.allowHDR = highDynamicRange;
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
		
		PostProcess[] opaqueProcesses = new PostProcess[ kOpaquePriorityMax];
		PostProcess[] postProcesses = new PostProcess[ kPostPriorityMax];
		CommandBuffer commandBufferDepthTexture;
		CommandBuffer commandBufferOpaqueProcesses;
		CommandBuffer commandBufferPostProcesses;
		
		Camera cacheCamera;
		int? cacheWidth;
		int? cacheHeight;
	#if UNITY_EDITOR
		DepthTextureMode? cacheDefaultDepthTextureMode;
		bool? cacheOverrideTargetBuffers;
		bool? cacheOverrideCameraDepthTexture;
	#endif
	}
}
	