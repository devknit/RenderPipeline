
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderingPipeline
{
	[DisallowMultipleComponent]
	public sealed partial class Mosaic : GenericProcess<MosaicSettings, MosaicProperties>
	{
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
			if( Properties.StencilCompare != CompareFunction.Always)
			{
				RenderTextureFormat maskRenderTextureFormat = RenderTextureFormat.R8;
				
				if( SystemInfo.SupportsRenderTextureFormat( maskRenderTextureFormat) == false)
				{
					maskRenderTextureFormat = RenderTextureFormat.ARGB32;
				}
				var maskTextureDescriptor = new RenderTextureDescriptor(
					pipeline.ScreenWidth,
					pipeline.ScreenHeight,
					maskRenderTextureFormat, 0);
				maskTextureDescriptor.useMipMap = true;
				maskTextureDescriptor.autoGenerateMips = true;
				
				commandBuffer.GetTemporaryRT( kShaderPropertyMaskTarget, maskTextureDescriptor, FilterMode.Bilinear);
				var maskTarget = new RenderTargetIdentifier( kShaderPropertyMaskTarget);
				
				commandBuffer.SetRenderTarget( 
					maskTarget, 
					RenderBufferLoadAction.DontCare,
					RenderBufferStoreAction.Store,
					pipeline.DepthStencilBuffer,
					RenderBufferLoadAction.Load,
				#if UNITY_WEBGL
					RenderBufferStoreAction.Resolve);
				#else
					RenderBufferStoreAction.DontCare);
				#endif
				commandBuffer.ClearRenderTarget( false, true, Color.clear, 0);
				pipeline.DrawFill( commandBuffer, material, 0);
				
				commandBuffer.SetRenderTarget( 
					context.target0,
					RenderBufferLoadAction.Load,	
					RenderBufferStoreAction.Store,
					RenderBufferLoadAction.DontCare,	
					RenderBufferStoreAction.DontCare);
				commandBuffer.SetGlobalTexture( kShaderPropertyMaskTex, maskTarget);
				commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, context.source0);
				pipeline.SetViewport( commandBuffer, nextProcess);
				pipeline.DrawFill( commandBuffer, material, 1);
				commandBuffer.ReleaseTemporaryRT( kShaderPropertyMaskTarget);
			}
			else
			{
				commandBuffer.SetRenderTarget( 
					context.target0,
					RenderBufferLoadAction.Load,	
					RenderBufferStoreAction.Store,
					RenderBufferLoadAction.DontCare,	
					RenderBufferStoreAction.DontCare);
				commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, context.source0);
				pipeline.SetViewport( commandBuffer, nextProcess);
				pipeline.DrawFill( commandBuffer, material, 2);
			}
			context.duplicated = false;
			return true;
		}
		public static readonly int kShaderPropertyMaskTarget = Shader.PropertyToID( "Mosaic::Mask");
		static readonly int kShaderPropertyMaskTex = Shader.PropertyToID( "_MaskTex");
	}
}
