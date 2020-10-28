
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
		public override void Create()
		{
			if( shader != null && material == null)
			{
				material = new Material( shader);
			}
		}
		public override void Dispose()
		{
			if( material != null)
			{
				ObjectUtility.Release( material);
				material = null;
			}
		}
		public override bool RestoreMaterials()
		{
			bool rebuild = false;
			
			if( shader != null && material == null)
			{
				material = new Material( shader);
				rebuild = true;
			}
			return rebuild;
		}
		public override bool Valid()
		{
			return Properties.Enabled != false && material != null;
		}
		public override void ClearPropertiesCache()
		{
			Properties.ClearCache();
		}
		public override PostProcessEvent GetPostProcessEvent()
		{
			return PostProcessEvent.BeforeImageEffects;
		}
		public override bool UpdateProperties( RenderPipeline pipeline, bool clearCache)
		{
			if( clearCache != false)
			{
				Properties.ClearCache();
			}
			return Properties.CheckParameterChange( pipeline, material, (width, height) => 
			{
				descriptor = new RenderTextureDescriptor( width, height, TextureUtil.DefaultHDR);
				descriptor.useMipMap = false;
				descriptor.autoGenerateMips = false;
			});
		}
		public override DepthTextureMode GetDepthTextureMode()
		{
			return DepthTextureMode.Depth;
		}
		public override bool IsRequiredHighDynamicRange()
		{
			return false;
		}
		public override void BuildCommandBuffer( RenderPipeline pipeline,
			CommandBuffer commandBuffer, TargetContext context, IPostProcess nextProcess)
		{
			if( context.CompareSource0ToTarget0() != false)
			{
				int temporary = pipeline.GetTemporaryRT();
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
			commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, context.source0);
			pipeline.DrawFill( commandBuffer, material, 0);
			
			/**/
			commandBuffer.SetRenderTarget( 
				context.target0,
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.Store,
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.DontCare);
			commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, context.source0);
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
