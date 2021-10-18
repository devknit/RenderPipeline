#define WITH_CLEARRENDERTARGET

using UnityEngine;
using UnityEngine.Rendering;

namespace RenderingPipeline
{
	[DisallowMultipleComponent]
	public sealed partial class MotionBlur : GenericProcess<MotionBlurSettings, MotionBlurProperties>
	{
		protected override bool OnUpdateProperties( RenderPipeline pipeline, Material material)
		{
			int updateFlags = Properties.UpdateProperties( pipeline, material);
			
			if( (updateFlags & MotionBlurProperties.kDescriptor) != 0)
			{
				UpdateDescriptors( Properties.ScreenWidth, Properties.ScreenHeight);
			}
			return (updateFlags & MotionBlurProperties.kRebuild) != 0;
		}
		public override DepthTextureMode GetDepthTextureMode()
		{
			return DepthTextureMode.Depth | DepthTextureMode.MotionVectors;
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
			
			/* Pass 0 */
			var velocityTarget = new RenderTargetIdentifier( kShaderPropertyVelocityTarget);
			commandBuffer.GetTemporaryRT( 
				kShaderPropertyVelocityTarget, 
				velocityDescriptor, FilterMode.Bilinear);
			commandBuffer.SetRenderTarget( 
				velocityTarget, 
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.Store,
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.DontCare);
		#if WITH_CLEARRENDERTARGET
			commandBuffer.ClearRenderTarget( false, true, Color.clear, 0);
		#endif
			pipeline.DrawFill( commandBuffer, material, 0);
			
			/* Pass 1 */
			var halfTarget = new RenderTargetIdentifier( kShaderPropertyHalfTarget);
			commandBuffer.GetTemporaryRT( 
				kShaderPropertyHalfTarget, 
				halfDescriptor, FilterMode.Bilinear);
			commandBuffer.SetRenderTarget( 
				halfTarget, 
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.Store,
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.DontCare);
		#if WITH_CLEARRENDERTARGET
			commandBuffer.ClearRenderTarget( false, true, Color.clear, 0);
		#endif
			commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, velocityTarget);
			pipeline.DrawFill( commandBuffer, material, 1);
			
			/* Pass 2 */
			var quarterTarget = new RenderTargetIdentifier( kShaderPropertyQuarterTarget);
			commandBuffer.GetTemporaryRT( 
				kShaderPropertyQuarterTarget, 
				quarterDescriptor, FilterMode.Bilinear);
			commandBuffer.SetRenderTarget( 
				quarterTarget, 
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.Store,
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.DontCare);
		#if WITH_CLEARRENDERTARGET
			commandBuffer.ClearRenderTarget( false, true, Color.clear, 0);
		#endif
			commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, halfTarget);
			pipeline.DrawFill( commandBuffer, material, 2);
			
			/* Pass 2 */
			var octaveTarget = new RenderTargetIdentifier( kShaderPropertyOctaveTarget);
			commandBuffer.GetTemporaryRT( 
				kShaderPropertyOctaveTarget, 
				octaveDescriptor, FilterMode.Bilinear);
			commandBuffer.SetRenderTarget( 
				octaveTarget, 
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.Store,
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.DontCare);
		#if WITH_CLEARRENDERTARGET
			commandBuffer.ClearRenderTarget( false, true, Color.clear, 0);
		#endif
			commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, quarterTarget);
			pipeline.DrawFill( commandBuffer, material, 2);
			
			/* Pass 3 */
			var tileTarget = new RenderTargetIdentifier( kShaderPropertyTileTarget);
			commandBuffer.GetTemporaryRT( 
				kShaderPropertyTileTarget, 
				tileDescriptor, FilterMode.Bilinear);
			commandBuffer.SetRenderTarget( 
				tileTarget, 
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.Store,
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.DontCare);
		#if WITH_CLEARRENDERTARGET
			commandBuffer.ClearRenderTarget( false, true, Color.clear, 0);
		#endif
			commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, octaveTarget);
			pipeline.DrawFill( commandBuffer, material, 3);
			
			/* Pass 4 */
			var neighborTarget = new RenderTargetIdentifier( kShaderPropertyNeighborTarget);
			commandBuffer.GetTemporaryRT( 
				kShaderPropertyNeighborTarget, 
				neighborDescriptor, FilterMode.Bilinear);
			commandBuffer.SetRenderTarget( 
				neighborTarget, 
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.Store,
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.DontCare);
		#if WITH_CLEARRENDERTARGET
			commandBuffer.ClearRenderTarget( false, true, Color.clear, 0);
		#endif
			commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, tileTarget);
			pipeline.DrawFill( commandBuffer, material, 4);
			
			/* Pass 5 */
			commandBuffer.SetRenderTarget( 
				context.target0,
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.Store,
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.DontCare);
			commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, context.source0);
			pipeline.SetViewport( commandBuffer, nextProcess);
			pipeline.DrawFill( commandBuffer, material, 5);
			context.duplicated = false;
			
			commandBuffer.ReleaseTemporaryRT( kShaderPropertyVelocityTarget);
			commandBuffer.ReleaseTemporaryRT( kShaderPropertyHalfTarget);
			commandBuffer.ReleaseTemporaryRT( kShaderPropertyQuarterTarget);
			commandBuffer.ReleaseTemporaryRT( kShaderPropertyOctaveTarget);
			commandBuffer.ReleaseTemporaryRT( kShaderPropertyTileTarget);
			commandBuffer.ReleaseTemporaryRT( kShaderPropertyNeighborTarget);
			return true;
		}
		void UpdateDescriptors( int width, int height)
		{
			if( width > 0 && height > 0 && Properties.TileSize.HasValue != false)
			{
				var velocityTextureFormat = RenderTextureFormat.ARGB32;
				int tileSize = Properties.TileSize.Value;
				
				if( SystemInfo.SupportsRenderTextureFormat( RenderTextureFormat.ARGB2101010) != false)
				{
					velocityTextureFormat = RenderTextureFormat.ARGB2101010;
				}
				velocityDescriptor = new RenderTextureDescriptor( width, height, velocityTextureFormat, 0);
				velocityDescriptor.useMipMap = false;
				velocityDescriptor.autoGenerateMips = false;
				
				halfDescriptor = new RenderTextureDescriptor( width / 2, height / 2, RenderTextureFormat.RGHalf, 0);
				halfDescriptor.useMipMap = false;
				halfDescriptor.autoGenerateMips = false;
				
				quarterDescriptor = new RenderTextureDescriptor( width / 4, height / 4, RenderTextureFormat.RGHalf, 0);
				quarterDescriptor.useMipMap = false;
				quarterDescriptor.autoGenerateMips = false;
				
				octaveDescriptor = new RenderTextureDescriptor( width / 8, height / 8, RenderTextureFormat.RGHalf, 0);
				octaveDescriptor.useMipMap = false;
				octaveDescriptor.autoGenerateMips = false;
				
				tileDescriptor = new RenderTextureDescriptor( width / tileSize, height / tileSize, RenderTextureFormat.RGHalf, 0);
				tileDescriptor.useMipMap = false;
				tileDescriptor.autoGenerateMips = false;
				
				neighborDescriptor = new RenderTextureDescriptor( width / tileSize, height / tileSize, RenderTextureFormat.RGHalf, 0);
				neighborDescriptor.useMipMap = false;
				neighborDescriptor.autoGenerateMips = false;
			}
		}
		
		static readonly int kShaderPropertyVelocityTarget = Shader.PropertyToID( "_VelocityTex");
		static readonly int kShaderPropertyHalfTarget = Shader.PropertyToID( "_HalfTex");
		static readonly int kShaderPropertyQuarterTarget = Shader.PropertyToID( "_QuarterTex");
		static readonly int kShaderPropertyOctaveTarget = Shader.PropertyToID( "_OctaveTex");
		static readonly int kShaderPropertyTileTarget = Shader.PropertyToID( "_TileTex");
		static readonly int kShaderPropertyNeighborTarget = Shader.PropertyToID( "_NeighborTex");
		
		RenderTextureDescriptor velocityDescriptor;
		RenderTextureDescriptor halfDescriptor;
		RenderTextureDescriptor quarterDescriptor;
		RenderTextureDescriptor octaveDescriptor;
		RenderTextureDescriptor tileDescriptor;
		RenderTextureDescriptor neighborDescriptor;
	}
}
