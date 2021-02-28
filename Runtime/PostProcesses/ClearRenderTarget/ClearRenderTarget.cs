
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderingPipeline
{
	[DisallowMultipleComponent]
	public sealed partial class ClearRenderTarget : GenericProcess<ClearRenderTargetSettings, ClearRenderTargetProperties>
	{
		public override bool Valid()
		{
			return Enabled != false;
		}
		protected override bool OnUpdateProperties( RenderPipeline pipeline, Material material)
		{
			return Properties.UpdateProperties( pipeline, material);
		}
		public override DepthTextureMode GetDepthTextureMode()
		{
			return DepthTextureMode.None;
		}
		public override bool IsRequiredHighDynamicRange()
		{
			return false;
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
