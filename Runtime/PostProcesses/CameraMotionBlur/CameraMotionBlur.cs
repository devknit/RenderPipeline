﻿
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderingPipeline
{
	[DisallowMultipleComponent]
	public sealed partial class CameraMotionBlur : GenericProcess<CameraMotionBlurSettings, CameraMotionBlurProperties>
	{
		public enum Quality
		{
			kLow,
			kMiddle,
			kHigh
		}
		protected override bool OnUpdateProperties( RenderPipeline pipeline, Material material)
		{
			return Properties.UpdateProperties( pipeline, material, (width, height) => 
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
			pipeline.SetViewport( commandBuffer, nextProcess);
			pipeline.DrawFill( commandBuffer, material, 1);
			
			commandBuffer.ReleaseTemporaryRT( kShaderTargetBlur);
			context.duplicated = false;
			return true;
		}
		
		static readonly int kShaderTargetBlur = Shader.PropertyToID( "_CameraMotionBlurTarget");
		static readonly int kShaderPropertyBlurTex = Shader.PropertyToID( "_BlurTex");
		
		[System.NonSerialized]
		RenderTextureDescriptor descriptor;
	}
}
