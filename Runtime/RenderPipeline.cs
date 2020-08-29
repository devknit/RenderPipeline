
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

//CameraPipeline
namespace RenderPipeline
{
//	[ExecuteInEditMode]
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
			var processes = new List<PostProcess>();
			
			if( postProcessesTarget == null)
			{
				postProcessesTarget = gameObject;
			}
			if( postProcessesTarget.GetComponent<EdgeDetection>() is EdgeDetection edgeDetection)
			{
				processes.Add( edgeDetection);
			}
			if( postProcessesTarget.GetComponent<Bloom>() is Bloom bloom)
			{
				processes.Add( bloom);
			}
			if( postProcessesTarget.GetComponent<DepthOfField>() is DepthOfField depthOfField)
			{
				processes.Add( depthOfField);
			}
			if( postProcessesTarget.GetComponent<Mosaic>() is Mosaic mosaic)
			{
				processes.Add( mosaic);
			}
			if( postProcessesTarget.GetComponent<Fxaa3>() is Fxaa3 fxaa3)
			{
				processes.Add( fxaa3);
			}
			postProcesses = processes.ToArray();
			
			for( int i0 = 0; i0 < postProcesses.Length; ++i0)
			{
				postProcesses[ i0].Initialize( this);
				postProcesses[ i0].Create();
				postProcesses[ i0].CheckParameterChange( true);
			}
			cacheCamera = GetComponent<Camera>();
			RebuildCommandBuffers();
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
					postProcesses[ i0].Dispose();
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
			if( materialCopy != null)
			{
				Destroy( materialCopy);
				materialCopy = null;
			}
			if( fillMesh != null)
			{
				Destroy( fillMesh);
				fillMesh = null;
			}
			if( flipMesh != null)
			{
				Destroy( flipMesh);
				flipMesh = null;
			}
		}
		void OnPreRender()
		{
		#if UNITY_EDITOR
			if( cacheOverrideTargetBuffers != overrideTargetBuffers)
			{
				cacheOverrideTargetBuffers = overrideTargetBuffers;
				isRebuildCommandBuffers = true;
			}
			if( cacheForceUpdateDepthTexture != forceUpdateDepthTexture)
			{
				cacheForceUpdateDepthTexture = forceUpdateDepthTexture;
				isRebuildCommandBuffers = true;
			}
			if( cacheFlipHorizontal != flipHorizontal)
			{
				cacheFlipHorizontal = flipHorizontal;
				isRebuildCommandBuffers = true;
			}
		#endif
			if( overrideTargetBuffers != false)
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
					if( postProcesses[ i0].CheckParameterChange( false) != false)
					{
						isRebuildCommandBuffers = true;
					}
				}
			}
			if( isRebuildCommandBuffers != false)
			{
				RebuildCommandBuffers();
			}
		}
		void RebuildCommandBuffers()
		{
			var enabledProcesses = new List<PostProcess>();
			var depthTextureMode = (forceUpdateDepthTexture != false)? 
				DepthTextureMode.Depth : DepthTextureMode.None;
			bool highDynamicRange = false;
			bool forceIntoRenderTexture = false;
			PostProcess process, prevProcess = null;
			
			for( int i0 = 0; i0 < postProcesses.Length; ++i0)
			{
				process = postProcesses[ i0];
				if( process.Valid() != false)
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
#if true
			if( enabledProcesses.Count == 0 && overrideTargetBuffers == false)
			{
			#if UNITY_EDITOR
				cacheCamera.SetTargetBuffers( Display.main.colorBuffer, Display.main.depthBuffer);
			#endif
			}
			else
#endif
			{
				if( overrideTargetBuffers == false)
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
				
				if( overrideTargetBuffers == false)
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
					if( overrideTargetBuffers == false)
					{
						if( enabledProcesses[ i0].IsHighDynamicRange() != false)
						{
							highDynamicRange = true;
						}
					}
					depthTextureMode |= enabledProcesses[ i0].GetDepthTextureMode();
				}
				if( overrideTargetBuffers != false && (depthTextureMode & DepthTextureMode.Depth) != 0)
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
						if( overrideTargetBuffers == false)
						{
							context.SetTarget0( BuiltinRenderTextureType.CameraTarget);
						}
						else
						{
							context.SetTarget0( colorBuffer);
						}
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
				if( overrideTargetBuffers != false)
				{
					commandBufferPostProcesses.SetRenderTarget( 
						BuiltinRenderTextureType.CameraTarget, 
						RenderBufferLoadAction.DontCare,
						RenderBufferStoreAction.Store,
						RenderBufferLoadAction.DontCare,
						RenderBufferStoreAction.DontCare);
					commandBufferPostProcesses.SetGlobalTexture( kShaderPropertyMainTex, colorBuffer);
					commandBufferPostProcesses.DrawMesh( (flipHorizontal == false)? fillMesh : flipMesh, Matrix4x4.identity, materialCopy, 0, 0);
					
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
		
		static readonly int kShaderPropertyMainTex = Shader.PropertyToID( "_MainTex");
		static readonly int kShaderPropertyDepthTextureId = Shader.PropertyToID( "CameraPipeline::DepthTexture");
		static readonly int kShaderPropertyCameraDepthTexture = Shader.PropertyToID( "_CameraDepthTexture");
		
		[SerializeField]
		Shader shaderCopy = default;
		[SerializeField]
		bool overrideTargetBuffers = false;
		[SerializeField]
		bool forceUpdateDepthTexture = false;
		[SerializeField]
		bool flipHorizontal = false;
		[SerializeField]
		GameObject postProcessesTarget = default;
		
		Mesh fillMesh;
		Mesh flipMesh;
		Material materialCopy;
		RenderTexture colorBuffer;
		RenderTexture depthBuffer;
		bool isRebuildCommandBuffers;
		
		PostProcess[] postProcesses;
		CommandBuffer commandBufferDepthTexture;
		CommandBuffer commandBufferPostProcesses;
		
		Camera cacheCamera;
		int? cacheWidth;
		int? cacheHeight;
	#if UNITY_EDITOR
		bool? cacheOverrideTargetBuffers;
		bool? cacheForceUpdateDepthTexture;
		bool? cacheFlipHorizontal;
	#endif
	}
}
