﻿
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderingPipeline
{
	[DisallowMultipleComponent]
	public sealed partial class GaussianBlur : GenericProcess<GaussianBlurSettings, GaussianBlurProperties>
	{
		public override void Create()
		{
			base.Create();
			m_Resources = new GaussianBlurResources();
			m_Resources.Create();
		}
		public override void Dispose()
		{
			base.Dispose();
			m_Resources.Dispose();
			m_Resources = null;
		}
		protected override bool OnUpdateProperties( RenderPipeline pipeline, Material material)
		{
			return Properties.UpdateProperties( pipeline, material, m_Resources);
		}
		public override DepthTextureMode GetDepthTextureMode()
		{
			return DepthTextureMode.None;
		}
		public override bool IsRequiredHighDynamicRange()
		{
			return false;
		}
		public override bool Valid()
		{
			return base.Valid() && Properties.BlendWeight > 0;
		}
		public override bool BuildCommandBuffer( RenderPipeline pipeline,
			CommandBuffer commandBuffer, TargetContext context, IPostProcess nextProcess)
		{
			return m_Resources.BuildCommandBuffer( pipeline, commandBuffer, context, nextProcess, material);
		}
		GaussianBlurResources m_Resources;
	}
}
