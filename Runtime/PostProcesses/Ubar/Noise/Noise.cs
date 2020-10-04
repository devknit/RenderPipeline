
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	[System.Serializable]
	public sealed partial class Noise : UbarProperty
	{
		public NoiseProperties Properties
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
		public override bool UpdateProperties( RenderPipeline pipeline, bool clearCache)
		{
			if( clearCache != false)
			{
				Properties.ClearCache();
			}
			return Properties.UpdateProperties( material, false);
		}
		public override PostProcessEvent GetPostProcessEvent()
		{
			return PostProcessEvent.BeforeImageEffectsOpaque;
		}
		public override DepthTextureMode GetDepthTextureMode()
		{
			return DepthTextureMode.None;
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
					commandBuffer.Blit( context.source0, temporary);
					context.SetTarget0( temporary);
				}
			}
			commandBuffer.SetRenderTarget( 
				context.target0, 
				(Properties.StencilCompare != CompareFunction.Always)? 
					RenderBufferLoadAction.Load    :
					RenderBufferLoadAction.DontCare,
				RenderBufferStoreAction.Store,
				(pipeline.OverrideCameraDepthTexture != false)?
					context.depthBuffer : BuiltinRenderTextureType.RenderTexture,
				RenderBufferLoadAction.Load,	
				RenderBufferStoreAction.DontCare);
			commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, context.source0);
			pipeline.DrawFill( commandBuffer, material, 0);
			context.duplicated = false;
		}
		public override long GetDepthStencilHashCode()
		{
			return Properties.GetDepthStencilHashCode();
		}
		internal override IUbarProperties GetProperties()
		{
			return Properties;
		}
		
		[SerializeField]
        NoiseSettings sharedSettings = default;
        [SerializeField]
        NoiseProperties properties = default;
        [SerializeField]
		Shader shader = default;
		[System.NonSerialized]
		Material material;
	}
}
