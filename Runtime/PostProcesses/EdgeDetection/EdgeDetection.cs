
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderingPipeline
{
	[DisallowMultipleComponent]
	public sealed partial class EdgeDetection : GenericProcess<EdgeDetectionSettings, EdgeDetectionProperties>
	{
		protected override bool OnUpdateProperties( RenderPipeline pipeline, Material material)
		{
			return Properties.UpdateProperties( pipeline, material);
		}
		public override DepthTextureMode GetDepthTextureMode()
		{
			return DepthTextureMode.Depth;
		}
		public override bool IsRequiredHighDynamicRange()
		{
			return false;
		}
		public override bool BuildCommandBuffer( RenderPipeline pipeline,
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
				pipeline.DepthStencilBuffer,
				RenderBufferLoadAction.Load,	
				RenderBufferStoreAction.DontCare);
			commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, context.source0);
			pipeline.SetViewport( commandBuffer, nextProcess);
			pipeline.DrawFill( commandBuffer, material, (int)Properties.DetectType);
			context.duplicated = false;
			return true;
		}
		public override long GetDepthStencilHashCode()
		{
			return Properties.GetDepthStencilHashCode();
		}
	}
}
