
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
		public bool FlipHorizontal
		{
			get{ return flipHorizontal; }
			set
			{
				if( flipHorizontal != value)
				{
					flipHorizontal = value;
					isRebuildCommandBuffers = true;
				}
			}
		}
		public Color ScreenBlendColor
		{
			get{ return screenBlendColor; }
			set{ screenBlendColor = value; }
		}
		public EdgeDetection EdgeDetection
		{
			get{ return postProcesses[ 0] as EdgeDetection; }
			private set{ ApplyProcess( ref postProcesses[ 0], value); }
		}
		public Bloom Bloom
		{
			get{ return postProcesses[ 1] as Bloom; }
			private set{ ApplyProcess( ref postProcesses[ 1], value); }
		}
		public DepthOfField DepthOfField
		{
			get{ return postProcesses[ 2] as DepthOfField; }
			private set{ ApplyProcess( ref postProcesses[ 2], value); }
		}
		public Mosaic Mosaic
		{
			get{ return postProcesses[ 3] as Mosaic; }
			private set{ ApplyProcess( ref postProcesses[ 3], value); }
		}
		public Fxaa3 Fxaa3
		{
			get{ return postProcesses[ 4] as Fxaa3; }
			private set{ ApplyProcess( ref postProcesses[ 4], value); }
		}
		public AlphaBlend AlphaBlend
		{
			get{ return postProcesses[ 5] as AlphaBlend; }
			private set{ ApplyProcess( ref postProcesses[ 5], value); }
		}
		void Awake()
		{
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
			
			flipMesh = new Mesh();
			flipMesh.SetVertices(
				new Vector3[]{
					new Vector3( 0, 0, 0),
					new Vector3( 0, 1, 0),
					new Vector3( 1, 1, 0),
					new Vector3( 1, 0, 0)
				});
			flipMesh.SetUVs( 
				0,
				new Vector2[]{
					new Vector2( 1, 0),
					new Vector2( 1, 1),
					new Vector2( 0, 1),
					new Vector2( 0, 0)
				});
			flipMesh.SetIndices(
				new int[]{ 0, 1, 2, 3 }, MeshTopology.Quads, 0, false);
			flipMesh.Optimize();
			flipMesh.UploadMeshData( true);
			
			if( shaderCopy != null && materialCopy == null)
			{
				materialCopy = new Material( shaderCopy);
			}
			if( shaderColor != null && materialColor == null)
			{
				materialColor = new Material( shaderColor);
			}
			CollectionProcesses();

			cacheCamera = GetComponent<Camera>();
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
			
			var edgeDetection = targetObject.GetComponent<EdgeDetection>() as EdgeDetection;
			if( IsMissing( edgeDetection) != false)
			{
				EdgeDetection = null;
				rebuild = true;
			}
			else if( EdgeDetection != edgeDetection)
			{
				EdgeDetection = edgeDetection;
				rebuild = true;
			}
			var bloom = targetObject.GetComponent<Bloom>() as Bloom;
			if( IsMissing( bloom) != false)
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
			if( IsMissing( depthOfField) != false)
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
			if( IsMissing( mosaic) != false)
			{
				Mosaic = null;
				rebuild = true;
			}
			else if( Mosaic != mosaic)
			{
				Mosaic = mosaic;
				rebuild = true;
			}
			var fxaa3 = targetObject.GetComponent<Fxaa3>() as Fxaa3;
			if( IsMissing( fxaa3) != false)
			{
				Fxaa3 = null;
				rebuild = true;
			}
			else if( Fxaa3 != fxaa3)
			{
				Fxaa3 = fxaa3;
				rebuild = true;
			}
			var alphaBlend = targetObject.GetComponent<AlphaBlend>() as AlphaBlend;
			if( IsMissing( alphaBlend) != false)
			{
				AlphaBlend = null;
				rebuild = true;
			}
			else if( AlphaBlend != alphaBlend)
			{
				AlphaBlend = alphaBlend;
				rebuild = true;
			}
			return rebuild;
		}
		static bool IsMissing<T>( T obj) where T : class
		{
			if( obj == null)
			{
				return object.ReferenceEquals( obj, null);
			}
			return false;
		}
		void Release( Object obj)
		{
		#if UNITY_EDITOR
			if( UnityEditor.EditorApplication.isPlaying == false)
			{
				DestroyImmediate( obj);
			}
			else
		#endif
			{
				Destroy( obj);
			}
		}
		void OnDestroy()
		{
			if( commandBufferDepthTexture != null)
			{
				cacheCamera.RemoveCommandBuffer( CameraEvent.AfterForwardOpaque, commandBufferDepthTexture);
				commandBufferDepthTexture = null;
			}
			if( commandBufferPostProcesses != null)
			{
				cacheCamera.RemoveCommandBuffer( CameraEvent.BeforeImageEffects, commandBufferPostProcesses);
				commandBufferPostProcesses = null;
			}
			if( postProcesses != null)
			{
				for( int i0 = 0; i0 < postProcesses.Length; ++i0)
				{
					postProcesses[ i0]?.Dispose();
				}
			}
			cacheCamera.allowHDR = false;
			cacheCamera.allowMSAA = false;
			cacheCamera.forceIntoRenderTexture = false;
			cacheCamera.depthTextureMode = DepthTextureMode.None;
			cacheCamera.SetTargetBuffers( Display.main.colorBuffer, Display.main.depthBuffer);
			
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
				Release( materialColor);
				materialColor = null;
			}
			if( materialCopy != null)
			{
				Release( materialCopy);
				materialCopy = null;
			}
			if( fillMesh != null)
			{
				Release( fillMesh);
				fillMesh = null;
			}
			if( flipMesh != null)
			{
				Release( flipMesh);
				flipMesh = null;
			}
		}
		void OnPreRender()
		{
		#if UNITY_EDITOR
			if( CollectionProcesses() != false)
			{
				isRebuildCommandBuffers = true;
			}
			if( cacheOverrideTargetBuffers != OverrideTargetBuffers)
			{
				cacheOverrideTargetBuffers = OverrideTargetBuffers;
				isRebuildCommandBuffers = true;
			}
			if( cacheDefaultDepthTextureMode != defaultDepthTextureMode)
			{
				cacheDefaultDepthTextureMode = defaultDepthTextureMode;
				isRebuildCommandBuffers = true;
			}
			if( cacheFlipHorizontal != flipHorizontal)
			{
				cacheFlipHorizontal = flipHorizontal;
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
			if( postProcesses != null)
			{
				for( int i0 = 0; i0 < postProcesses.Length; ++i0)
				{
					if( (postProcesses[ i0]?.CheckParameterChange( false) ?? false) != false)
					{
						isRebuildCommandBuffers = true;
					}
				}
			}
			if( cacheScreenBlendColor != screenBlendColor)
			{
				materialColor.SetColor( kShaderPropertyColor, screenBlendColor);
				cacheScreenBlendColor = screenBlendColor;
			}
			if( isRebuildCommandBuffers != false)
			{
				RebuildCommandBuffers();
			}
		}
		void RebuildCommandBuffers()
		{
			var enabledProcesses = new List<PostProcess>();
			var depthTextureMode = defaultDepthTextureMode;
			bool highDynamicRange = false;
			bool forceIntoRenderTexture = false;
			PostProcess process, prevProcess = null;
			
			for( int i0 = 0; i0 < postProcesses.Length; ++i0)
			{
				process = postProcesses[ i0];
				
				if( (process?.Valid() ?? false) != false)
				{
					enabledProcesses.Add( process);
					
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
			if( commandBufferDepthTexture != null)
			{
				cacheCamera.RemoveCommandBuffer( CameraEvent.AfterForwardOpaque, commandBufferDepthTexture);
				commandBufferDepthTexture = null;
			}
			if( commandBufferPostProcesses != null)
			{
				cacheCamera.RemoveCommandBuffer( CameraEvent.BeforeImageEffects, commandBufferPostProcesses);
				commandBufferPostProcesses = null;
			}
			if( enabledProcesses.Count == 0 && OverrideTargetBuffers == false)
			{
			#if UNITY_EDITOR
				cacheCamera.SetTargetBuffers( Display.main.colorBuffer, Display.main.depthBuffer);
			#endif
			}
			else
			{
				if( OverrideTargetBuffers == false)
				{
					forceIntoRenderTexture = true;
				#if UNITY_EDITOR
					/* [2019.4.1f1]
					   SetTargetBuffers の引数に Display.main.*****Buffer を渡しても実機では正しく動作しない。
					   エディタ上でのみ、SetTargetBuffers を呼び出す前と同じ状態に戻る。
					   実機では現状元に戻す方法が存在しない。
					 */
					cacheCamera.SetTargetBuffers( Display.main.colorBuffer, Display.main.depthBuffer);
				#endif
				}
				else
				{
					bool refreshColorBuffer = colorBuffer == null || colorBuffer.width != Screen.width || colorBuffer.height != Screen.height;
					bool refreshDepthBuffer = depthBuffer == null || depthBuffer.width != Screen.width || depthBuffer.height != Screen.height;
					
					if( refreshColorBuffer != false
					||	refreshDepthBuffer != false)
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
				}
				commandBufferPostProcesses = new CommandBuffer();
				commandBufferPostProcesses.name = "CameraPipeline::PostProcesses";
				commandBufferPostProcesses.Clear();
				commandBufferPostProcesses.SetProjectionMatrix( Matrix4x4.Ortho( 0, 1, 0, 1, 0, 1));
				commandBufferPostProcesses.SetViewMatrix( Matrix4x4.identity);
				
				var usedTemporaries = new Dictionary<int, TemporaryTarget>();
				var recycleTemporaries = new Dictionary<int, TemporaryTarget>();
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
				int temporaryCount = 0;
				
				for( int i0 = 0; i0 < enabledProcesses.Count; ++i0)
				{
					if( OverrideTargetBuffers == false)
					{
						if( enabledProcesses[ i0].IsHighDynamicRange() != false)
						{
							highDynamicRange = true;
						}
					}
					depthTextureMode |= enabledProcesses[ i0].GetDepthTextureMode();
				}
				if( OverrideTargetBuffers != false && (depthTextureMode & DepthTextureMode.Depth) != 0)
				{
					commandBufferDepthTexture = new CommandBuffer();
					commandBufferDepthTexture.name = "CameraPipeline::DepthTexture";
					commandBufferDepthTexture.Clear();
					commandBufferDepthTexture.SetProjectionMatrix( Matrix4x4.Ortho( 0, 1, 0, 1, 0, 1));
					commandBufferDepthTexture.SetViewMatrix( Matrix4x4.identity);
					
					var depthTexture = new RenderTargetIdentifier( kShaderPropertyDepthTextureId);
					commandBufferDepthTexture.GetTemporaryRT( kShaderPropertyDepthTextureId, 
						-1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.RFloat);
					Blit( commandBufferDepthTexture, context.depthBuffer, depthTexture);
					commandBufferDepthTexture.SetGlobalTexture( kShaderPropertyCameraDepthTexture, depthTexture);
					cacheCamera.AddCommandBuffer( CameraEvent.AfterForwardOpaque, commandBufferDepthTexture);
				}
				for( int i0 = 0; i0 < enabledProcesses.Count; ++i0)
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
					if( i0 == enabledProcesses.Count - 1)
					{
						context.SetTarget0( BuiltinRenderTextureType.CameraTarget);
					}
					enabledProcesses[ i0].BuildCommandBuffer( 
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
				if( OverrideTargetBuffers != false)
				{
				#if false
					commandBufferPostProcesses.SetRenderTarget( 
						BuiltinRenderTextureType.CameraTarget, 
						RenderBufferLoadAction.DontCare,
						RenderBufferStoreAction.Store,
						RenderBufferLoadAction.DontCare,
						RenderBufferStoreAction.DontCare);
					commandBufferPostProcesses.SetGlobalTexture( kShaderPropertyMainTex, colorBuffer);
					commandBufferPostProcesses.DrawMesh( (flipHorizontal == false)? fillMesh : flipMesh, Matrix4x4.identity, materialColor, 0, 0);
				#endif	
					if( (depthTextureMode & DepthTextureMode.Depth) != 0)
					{
						commandBufferPostProcesses.ReleaseTemporaryRT( kShaderPropertyDepthTextureId);
						depthTextureMode &= ~DepthTextureMode.Depth;
					}
				}
				foreach( var userdTemporaryId in usedTemporaries.Keys)
				{
					commandBufferPostProcesses.ReleaseTemporaryRT( userdTemporaryId);
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
		const string kTipsFlipHorizontal = "この機能を有効にした場合、水平方向に画面を反転させます。";
		
		[SerializeField]
		Shader shaderCopy = default;
		[SerializeField]
		Shader shaderColor = default;
		[SerializeField]
		DepthTextureMode defaultDepthTextureMode = default;
		[SerializeField, TooltipAttribute( kTipsOverrideTargetBuffers)]
		bool overrideTargetBuffers = false;
		[SerializeField, TooltipAttribute( kTipsFlipHorizontal)]
		bool flipHorizontal = false;
		[SerializeField]
		Color screenBlendColor = Color.clear;
		[SerializeField]
		GameObject postProcessesTarget = default;
		
		Mesh fillMesh;
		Mesh flipMesh;
		Material materialCopy;
		Material materialColor;
		RenderTexture colorBuffer;
		RenderTexture depthBuffer;
		bool isRebuildCommandBuffers;
		
		PostProcess[] postProcesses = new PostProcess[ 6];
		CommandBuffer commandBufferDepthTexture;
		CommandBuffer commandBufferPostProcesses;
		
		Camera cacheCamera;
		int? cacheWidth;
		int? cacheHeight;
		Color? cacheScreenBlendColor;
	#if UNITY_EDITOR
		DepthTextureMode? cacheDefaultDepthTextureMode;
		bool? cacheOverrideTargetBuffers;
		bool? cacheFlipHorizontal;
	#endif
	}
}
