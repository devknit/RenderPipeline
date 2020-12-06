
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	[DisallowMultipleComponent]
	public sealed partial class Noise : UbarSeparableProcess<NoiseSettings, NoiseProperties>
	{
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
				pipeline.DepthStencilBuffer,
				RenderBufferLoadAction.Load,	
				RenderBufferStoreAction.DontCare);
			commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, context.source0);
			pipeline.SetViewport( commandBuffer, nextProcess);
			pipeline.DrawFill( commandBuffer, material, 0);
			context.duplicated = false;
		}
		public override long GetDepthStencilHashCode()
		{
			return Properties.GetDepthStencilHashCode();
		}
	}
}
