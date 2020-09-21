
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	[System.Serializable]
	public sealed partial class CameraMotionBlur : PostProcess
	{
		public enum Quality
		{
			kLow,
			kMiddle,
			kHigh
		}
		public CameraMotionBlurProperties Properties
		{
			get{ return (sharedSettings != null)? sharedSettings.properties : properties; }
		}
		internal override void Create()
		{
			if( shader != null && material == null)
			{
				material = new Material( shader);
			}
		}
		internal override void Dispose()
		{
			if( material != null)
			{
				ObjectUtility.Release( material);
				material = null;
			}
		}
		internal override bool RestoreResources()
		{
			bool rebuild = false;
			
			if( shader != null && material == null)
			{
				material = new Material( shader);
				rebuild = true;
			}
			return rebuild;
		}
		internal override bool Valid()
		{
			return Properties.Enabled != false && material != null;
		}
		internal override void ClearCache()
		{
			Properties.ClearCache();
		}
		internal override bool CheckParameterChange( bool clearCache)
		{
			if( clearCache != false)
			{
				Properties.ClearCache();
			}
			return Properties.CheckParameterChange( material, pipeline.CacheCamera, (width, height) => 
			{
				descriptor = new RenderTextureDescriptor( width, height, TextureUtil.DefaultHDR);
				descriptor.useMipMap = false;
				descriptor.autoGenerateMips = false;
			});
		}
		internal override DepthTextureMode GetDepthTextureMode()
		{
			return DepthTextureMode.Depth;
		}
		internal override bool IsHighDynamicRange()
		{
			return false;
		}
		protected override bool OnDuplicate()
		{
			return false;
		}
		internal override void BuildCommandBuffer( 
			CommandBuffer commandBuffer, TargetContext context, 
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
			/**/
			var blurTarget = new RenderTargetIdentifier( kShaderTargetBlur);
			commandBuffer.GetTemporaryRT( kShaderTargetBlur, descriptor, FilterMode.Point);
			
			commandBuffer.SetRenderTarget( 
				blurTarget, 
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.Store,
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.DontCare);
			commandBuffer.SetGlobalTexture( kShaderPropertyMainTex, context.source0);
			pipeline.DrawFill( commandBuffer, material, 0);
			
			/**/
			commandBuffer.SetRenderTarget( 
				context.target0,
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.Store,
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.DontCare);
			commandBuffer.SetGlobalTexture( kShaderPropertyMainTex, context.source0);
			commandBuffer.SetGlobalTexture( kShaderPropertyBlurTex, blurTarget);
			pipeline.DrawFill( commandBuffer, material, 1);
			
			commandBuffer.ReleaseTemporaryRT( kShaderTargetBlur);
			context.duplicated = false;
		}
		
		static readonly int kShaderTargetBlur = Shader.PropertyToID( "_CameraMotionBlurTarget");
		static readonly int kShaderPropertyBlurTex = Shader.PropertyToID( "_BlurTex");
		
		[SerializeField]
		CameraMotionBlurSettings sharedSettings = default;
		[SerializeField]
		CameraMotionBlurProperties properties = default;
		[SerializeField]
		Shader shader = default;
		[System.NonSerialized]
		Material material;
		[System.NonSerialized]
		RenderTextureDescriptor descriptor;
	}
}
