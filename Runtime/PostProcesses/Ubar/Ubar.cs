
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	[System.Serializable]
	public sealed class Ubar : IPostProcess
	{
		public void Create()
		{
			if( shader != null && material == null)
			{
				material = new Material( shader);
			}
		}
		public void Dispose()
		{
			if( material != null)
			{
				ObjectUtility.Release( material);
				material = null;
			}
		}
		public bool RestoreMaterials()
		{
			bool rebuild = false;
			
			if( shader != null && material == null)
			{
				material = new Material( shader);
				rebuild = true;
			}
			return rebuild;
		}
		public bool Valid()
		{
			if( material != null)
			{
				return vignette?.Enabled ?? false;
			}
			return false;
		}
		public void ClearPropertiesCache()
		{
			vignette?.ClearCache();
		}
		public bool UpdateProperties( RenderPipeline pipeline, bool clearCache)
		{
			bool rebuild = false;
			
			if( clearCache != false)
			{
				vignette?.ClearCache();
			}
			if( (vignette?.CheckParameterChange( material) ?? false) != false)
			{
				rebuild = true;
			}
			return rebuild;
		}
		public CameraEvent GetCameraEvent()
		{
			return CameraEvent.BeforeImageEffects;
		}
		public DepthTextureMode GetDepthTextureMode()
		{
			return DepthTextureMode.None;
		}
		public bool IsHighDynamicRange()
		{
			return false;
		}
		public void BuildCommandBuffer( RenderPipeline pipeline,
			CommandBuffer commandBuffer, TargetContext context, IPostProcess nextProcess,
			System.Func<int, int, int, FilterMode, RenderTextureFormat, int> GetTemporaryRT)
		{
			if( context.CompareSource0ToTarget0() != false)
			{
				int temporary = GetTemporaryRT( -1, -1, 0, FilterMode.Bilinear, TextureUtil.DefaultHDR);
				if( nextProcess == null)
				{
					commandBuffer.Blit( context.source0, temporary);
					context.SetSource0( temporary);
				}
				else
				{
					context.SetTarget0( temporary);
				}
			}
			commandBuffer.SetRenderTarget( 
				context.target0, 
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.Store,
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.DontCare);
			commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, context.source0);
			pipeline.DrawFill( commandBuffer, material, 0);
			context.duplicated = false;
		}
		public long GetDepthStencilHashCode()
		{
			return DepthStencil.kDefaultHash;
		}
		
		[SerializeField]
        Shader shader = default;
		[System.NonSerialized]
		Material material;
		[System.NonSerialized]
		VignetteProperties vignette = default;
	}
}
