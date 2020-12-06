
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace RenderPipeline.DepthOfField
{
	[DisallowMultipleComponent]
	public sealed partial class DepthOfField : GenericProcess<DepthOfFieldSettings, DepthOfFieldProperties>
	{
		public override bool UpdateProperties( RenderPipeline pipeline, bool clearCache)
		{
			bool rebuild = false;
			
			if( clearCache != false)
			{
				ClearPropertiesCache();
			}
			return Properties.UpdateProperties( pipeline, material);
		}
		public override PostProcessEvent GetPostProcessEvent()
		{
			return Properties.Phase;
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
			if( context.CompareSource0ToTarget0() != false && nextProcess != null)
			{
				int temporary = pipeline.GetTemporaryRT();
				context.SetTarget0( temporary);
			}
			if( Properties.visualizeFocus != false)
			{
				commandBuffer.SetRenderTarget( 
					context.target0, 
					RenderBufferLoadAction.DontCare,	
					RenderBufferStoreAction.Store,
					RenderBufferLoadAction.DontCare,	
					RenderBufferStoreAction.DontCare);
				pipeline.DrawFill( commandBuffer, material, 7);
			}
			else
			{
				var alphaDepthTarget = new RenderTargetIdentifier( kShaderPropertyAlphaDepthTarget);
				commandBuffer.GetTemporaryRT( kShaderPropertyAlphaDepthTarget, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
				commandBuffer.SetRenderTarget( 
					alphaDepthTarget, 
					RenderBufferLoadAction.DontCare,	
					RenderBufferStoreAction.Store,
					RenderBufferLoadAction.DontCare,	
					RenderBufferStoreAction.DontCare);
				commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, context.source0);
				pipeline.DrawFill( commandBuffer, material, 0);
				
				int pass = (Properties.blurQuality == BlurQuality.kLow)? 2 : 3;
				
				if( Properties.highResolution != false)
				{
					commandBuffer.SetRenderTarget( 
						context.target0, 
						RenderBufferLoadAction.DontCare,	
						RenderBufferStoreAction.Store,
						RenderBufferLoadAction.DontCare,	
						RenderBufferStoreAction.DontCare);
					commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, alphaDepthTarget);
					pipeline.SetViewport( commandBuffer, nextProcess);
					pipeline.DrawFill( commandBuffer, material, pass);
				}
				else
				{
					int lowWidth = pipeline.ScreenWidth >> 1;
					int lowHeight = pipeline.ScreenHeight >> 1;
					var lowBlurTarget = new RenderTargetIdentifier( kShaderPropertyLowBlurTarget);
					var lowDiscTarget = new RenderTargetIdentifier( kShaderPropertyLowDiscTarget);
					commandBuffer.GetTemporaryRT( kShaderPropertyLowBlurTarget, 
						lowWidth, lowHeight, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
					commandBuffer.GetTemporaryRT( kShaderPropertyLowDiscTarget, 
						lowWidth, lowHeight, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
					
					commandBuffer.SetRenderTarget( 
						lowBlurTarget, 
						RenderBufferLoadAction.DontCare,	
						RenderBufferStoreAction.Store,
						RenderBufferLoadAction.DontCare,	
						RenderBufferStoreAction.DontCare);
					commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, alphaDepthTarget);
					pipeline.DrawFill( commandBuffer, material, 1);
					
					commandBuffer.SetRenderTarget( 
						lowDiscTarget, 
						RenderBufferLoadAction.DontCare,	
						RenderBufferStoreAction.Store,
						RenderBufferLoadAction.DontCare,	
						RenderBufferStoreAction.DontCare);
					commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, lowBlurTarget);
					pipeline.DrawFill( commandBuffer, material, pass);
					
					switch( Properties.blurQuality)
					{
						case BlurQuality.kMedium:
						{
							pass = 5;
							break;
						}
						case BlurQuality.kHigh:
						{
							pass = 6;
							break;
						}
						default:
						{
							pass = 4;
							break;
						}
					}
					commandBuffer.SetRenderTarget( 
						context.target0, 
						RenderBufferLoadAction.DontCare,	
						RenderBufferStoreAction.Store,
						RenderBufferLoadAction.DontCare,	
						RenderBufferStoreAction.DontCare);
					commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, alphaDepthTarget);
					commandBuffer.SetGlobalTexture( kShaderPropertyLowRez, lowDiscTarget);
					pipeline.SetViewport( commandBuffer, nextProcess);
					pipeline.DrawFill( commandBuffer, material, pass);
					
					commandBuffer.ReleaseTemporaryRT( kShaderPropertyLowBlurTarget);
					commandBuffer.ReleaseTemporaryRT( kShaderPropertyLowDiscTarget);
				}
				commandBuffer.ReleaseTemporaryRT( kShaderPropertyAlphaDepthTarget);
			}
			context.duplicated = false;
		}
		
		static readonly int kShaderPropertyAlphaDepthTarget = Shader.PropertyToID( "_AlphaDepthTarget");
		static readonly int kShaderPropertyLowBlurTarget = Shader.PropertyToID( "_LowBlurTarget");
		static readonly int kShaderPropertyLowDiscTarget = Shader.PropertyToID( "_LowDiscTarget");
		static readonly int kShaderPropertyLowRez = Shader.PropertyToID( "_LowRez");
	}
}
