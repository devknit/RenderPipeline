
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
			
			if( GetComponent<Camera>() is Camera camera)
			{
				cacheCamera = camera;
			}
			for( int i0 = 0; i0 < postProcesses.Length; ++i0)
			{
				postProcesses[ i0].Initialize( this);
				postProcesses[ i0].Create();
				postProcesses[ i0].CheckParameterChange();
			}
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
			if( cacheCamera != null)
			{
				cacheCamera.depthTextureMode = DepthTextureMode.None;
				cacheCamera.SetTargetBuffers( Display.main.colorBuffer, Display.main.depthBuffer);
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
			if( cacheCamera != null)
			{
				cacheCamera.forceIntoRenderTexture = false;
			}
		}
		void OnPreRender()
		{
			bool isRebuildCommandBuffers = false;
			
		#if UNITY_EDITOR
			if( cacheSelfIntoRenderTexture != selfIntoRenderTexture)
			{
				cacheSelfIntoRenderTexture = selfIntoRenderTexture;
				isRebuildCommandBuffers = true;
			}
			if( cacheForceUpdateDepthTexture != forceUpdateDepthTexture)
			{
				cacheForceUpdateDepthTexture = forceUpdateDepthTexture;
				isRebuildCommandBuffers = true;
			}
		#endif
			if( selfIntoRenderTexture != false)
			{
				if( cacheWidth != Screen.width
				||	cacheHeight != Screen.height)
				{
					isRebuildCommandBuffers = true;
				}
			}
			if( postProcesses != null)
			{
				for( int i0 = 0; i0 < postProcesses.Length; ++i0)
				{
					if( postProcesses[ i0].CheckParameterChange() != false)
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
			if( cacheCamera != null)
			{
				var enabledProcesses = new List<PostProcess>();
				var depthTextureMode = (forceUpdateDepthTexture != false)? 
					DepthTextureMode.Depth : DepthTextureMode.None;
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
				if( enabledProcesses.Count == 0)
				{
					cacheCamera.SetTargetBuffers( Display.main.colorBuffer, Display.main.depthBuffer);
				}
				else
				{
					if( selfIntoRenderTexture == false)
					{
						forceIntoRenderTexture = true;
						cacheCamera.SetTargetBuffers( Display.main.colorBuffer, Display.main.depthBuffer);
					}
					else
					{
						if( colorBuffer == null
						||	colorBuffer.width != Screen.width
						||	colorBuffer.height != Screen.height)
						{
							if( colorBuffer != null)
							{
								cacheCamera.SetTargetBuffers( Display.main.colorBuffer, Display.main.depthBuffer);
								colorBuffer.Release();
							}
							var colorBufferFormat = RenderTextureFormat.ARGB32;
							
							if( SystemInfo.SupportsRenderTextureFormat( RenderTextureFormat.RGB111110Float) != false)
							{
								colorBufferFormat = RenderTextureFormat.RGB111110Float;
							}
							colorBuffer = new RenderTexture( Screen.width, Screen.height, 0, colorBufferFormat);
							colorBuffer.name = "CameraPipeline::ColorBuffer";
						}
						if( depthBuffer == null
						||	depthBuffer.width != Screen.width
						||	depthBuffer.height != Screen.height)
						{
							if( depthBuffer != null)
							{
								cacheCamera.SetTargetBuffers( Display.main.colorBuffer, Display.main.depthBuffer);
								depthBuffer.Release();
							}
							depthBuffer = new RenderTexture( Screen.width, Screen.height, 24, RenderTextureFormat.Depth);
							depthBuffer.name = "CameraPipeline::DepthBuffer";
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
					
					if( selfIntoRenderTexture == false)
					{
						context.SetBuffers();
						context.SetSource0( BuiltinRenderTextureType.CameraTarget);
					}
					else
					{
						context.SetBuffers( colorBuffer, depthBuffer);
						context.SetSource0( colorBuffer);
					}
					context.SetTarget0( BuiltinRenderTextureType.CameraTarget);
					int temporaryCount = 0;
					
					for( int i0 = 0; i0 < enabledProcesses.Count; ++i0)
					{
						depthTextureMode |= enabledProcesses[ i0].GetDepthTextureMode();
					}
					if( selfIntoRenderTexture != false && (depthTextureMode & DepthTextureMode.Depth) != 0)
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
					foreach( var userdTemporaryId in usedTemporaries.Keys)
					{
						commandBufferPostProcesses.ReleaseTemporaryRT( userdTemporaryId);
					}
					if( selfIntoRenderTexture != false)
					{
						if( (depthTextureMode & DepthTextureMode.Depth) != 0)
						{
							commandBufferPostProcesses.ReleaseTemporaryRT( kShaderPropertyDepthTextureId);
							depthTextureMode &= ~DepthTextureMode.Depth;
						}
					}
					cacheCamera.AddCommandBuffer( CameraEvent.BeforeImageEffects, commandBufferPostProcesses);
				}
				cacheCamera.depthTextureMode = depthTextureMode;
				cacheCamera.forceIntoRenderTexture = forceIntoRenderTexture;
			}
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
		bool selfIntoRenderTexture = false;
		[SerializeField]
		bool forceUpdateDepthTexture = false;
		[SerializeField]
		GameObject postProcessesTarget = default;
		
		Mesh fillMesh;
		Material materialCopy;
		RenderTexture colorBuffer;
		RenderTexture depthBuffer;
		
		PostProcess[] postProcesses;
		CommandBuffer commandBufferDepthTexture;
		CommandBuffer commandBufferPostProcesses;
		
		Camera cacheCamera;
		int? cacheWidth;
		int? cacheHeight;
	#if UNITY_EDITOR
		bool? cacheSelfIntoRenderTexture;
		bool? cacheForceUpdateDepthTexture;
	#endif
	}
}
