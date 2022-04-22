#define WITH_CLEARRENDERTARGET

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
			/* mipmap */
			var brightnessExtractionTarget = new RenderTargetIdentifier( GaussianBlurResources.kShaderPropertyBrightnessExtractionTarget);
			commandBuffer.GetTemporaryRT( GaussianBlurResources.kShaderPropertyBrightnessExtractionTarget, m_Resources.brightnessExtractionDescriptor, FilterMode.Bilinear);
			
			commandBuffer.SetRenderTarget( 
				brightnessExtractionTarget,
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.Store,
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.DontCare);
		#if WITH_CLEARRENDERTARGET
			commandBuffer.ClearRenderTarget( false, true, Color.clear, 0);
		#endif
			commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, context.source0);
			commandBuffer.DrawMesh( m_Resources.brightnessExtractionMesh, Matrix4x4.identity, material, 0, 0);
			
			/* blur horizontal */
			var gaussianBlurHorizontalTarget = new RenderTargetIdentifier( GaussianBlurResources.kShaderPropertyGaussianBlurHorizontalTarget);
			commandBuffer.GetTemporaryRT( GaussianBlurResources.kShaderPropertyGaussianBlurHorizontalTarget, m_Resources.blurDescriptor, FilterMode.Bilinear);
			
			commandBuffer.SetRenderTarget( 
				gaussianBlurHorizontalTarget,
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.Store,
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.DontCare);
		#if WITH_CLEARRENDERTARGET
			commandBuffer.ClearRenderTarget( false, true, Color.clear, 0);
		#endif
			commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, brightnessExtractionTarget);
			commandBuffer.DrawMesh( m_Resources.blurHorizontalMesh, Matrix4x4.identity, material, 0, 1);
			
			/* blur vertical */
			var gaussianBlurVerticalTarget = new RenderTargetIdentifier( GaussianBlurResources.kShaderPropertyGaussianBlurVerticalTarget);
			commandBuffer.GetTemporaryRT( GaussianBlurResources.kShaderPropertyGaussianBlurVerticalTarget, m_Resources.blurDescriptor, FilterMode.Bilinear);
			
			commandBuffer.SetRenderTarget( 
				gaussianBlurVerticalTarget,
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.Store,
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.DontCare);
		#if WITH_CLEARRENDERTARGET
			commandBuffer.ClearRenderTarget( false, true, Color.clear, 0);
		#endif
			commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, GaussianBlurResources.kShaderPropertyGaussianBlurHorizontalTarget);
			commandBuffer.DrawMesh( m_Resources.blurVerticalMesh, Matrix4x4.identity, material, 0, 1);
			
			/* combine */
			if( m_Resources.combinePassCount > 1)
			{
				commandBuffer.SetRenderTarget( 
					gaussianBlurHorizontalTarget,
					RenderBufferLoadAction.DontCare,	
					RenderBufferStoreAction.Store,
					RenderBufferLoadAction.DontCare,	
					RenderBufferStoreAction.DontCare);
			#if WITH_CLEARRENDERTARGET
				commandBuffer.ClearRenderTarget( false, true, Color.clear, 0);
			#endif
				commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, gaussianBlurVerticalTarget);
				commandBuffer.DrawMesh( m_Resources.combineMesh, Matrix4x4.identity, material, 0, 2);
				commandBuffer.SetGlobalTexture( GaussianBlurResources.kShaderPropertyBlurTex, gaussianBlurVerticalTarget);
				commandBuffer.SetGlobalTexture( GaussianBlurResources.kShaderPropertyBlurCombinedTex, GaussianBlurResources.kShaderPropertyGaussianBlurHorizontalTarget);
			}
			else
			{
				commandBuffer.SetGlobalTexture( GaussianBlurResources.kShaderPropertyBlurCombinedTex, gaussianBlurVerticalTarget);
			}
			commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, context.source0);
			
			commandBuffer.SetRenderTarget( 
				context.target0,
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.Store,
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.DontCare);
			pipeline.SetViewport( commandBuffer, nextProcess);
			pipeline.DrawFill( commandBuffer, material, 3);
			context.duplicated = false;
			
			commandBuffer.ReleaseTemporaryRT( GaussianBlurResources.kShaderPropertyGaussianBlurVerticalTarget);
			commandBuffer.ReleaseTemporaryRT( GaussianBlurResources.kShaderPropertyGaussianBlurHorizontalTarget);
			commandBuffer.ReleaseTemporaryRT( GaussianBlurResources.kShaderPropertyBrightnessExtractionTarget);
			return true;
		}
		GaussianBlurResources m_Resources;
	}
}
