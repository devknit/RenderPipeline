
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderingPipeline
{
	[DisallowMultipleComponent]
	public sealed partial class ClearRenderTarget : CommandProcess<ClearRenderTargetProperties>
	{
		protected override bool OnUpdateProperties( RenderPipeline pipeline)
		{
			return Properties.UpdateProperties( pipeline, null);
		}
		public override bool BuildCommandBuffer( RenderPipeline pipeline,
			CommandBuffer commandBuffer, TargetContext context, IPostProcess nextProcess)
		{
			commandBuffer.ClearRenderTarget( 
				Properties.ClearDepth,
				Properties.ClearColor,
				Properties.Color,
				Properties.Depth);
			context.duplicated = false;
			return false;
		}
	}
}
