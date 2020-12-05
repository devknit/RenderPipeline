
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	[DisallowMultipleComponent]
	public sealed partial class Mosaic : PostProcess
	{
		public MosaicProperties Properties
		{
			get{ return (sharedSettings != null && useSharedProperties != false)? sharedSettings.properties : properties; }
		}
		public override void Create()
		{
			if( shader != null && material == null)
			{
				material = new Material( shader);
			}
		}
		public override void Dispose()
		{
			if( material != null)
			{
				ObjectUtility.Release( material);
				material = null;
			}
		}
		public override bool RestoreMaterials()
		{
			bool rebuild = false;
			
			if( shader != null && material == null)
			{
				material = new Material( shader);
				rebuild = true;
			}
			return rebuild;
		}
		public override bool Valid()
		{
			return ((sharedSettings != null)? sharedSettings.properties : properties).Enabled != false && material != null;
		}
		public override void ClearPropertiesCache()
		{
			sharedSettings?.properties.ClearCache();
			properties.ClearCache();
		}
		public override bool UpdateProperties( RenderPipeline pipeline, bool clearCache)
		{
			if( clearCache != false)
			{
				ClearPropertiesCache();
			}
			return Properties.CheckParameterChange( pipeline, material);
		}
		public override PostProcessEvent GetPostProcessEvent()
		{
			return Properties.Phase;
		}
		public override DepthTextureMode GetDepthTextureMode()
		{
			return DepthTextureMode.None;
		}
		public override bool IsRequiredHighDynamicRange()
		{
			return false;
		}
		public override void BuildCommandBuffer( RenderPipeline pipeline,
			CommandBuffer commandBuffer, TargetContext context, IPostProcess nextProcess)
		{
			if( context.CompareSource0ToTarget0() != false)
			{
				int temporary = pipeline.GetTemporaryRT();
				if( nextProcess == null)
				{
					commandBuffer.SetRenderTarget( 
						new RenderTargetIdentifier( temporary), 
						RenderBufferLoadAction.DontCare,
						RenderBufferStoreAction.Store,
						RenderBufferLoadAction.DontCare,
						RenderBufferStoreAction.DontCare);
					commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, context.source0);
					pipeline.SetViewport( commandBuffer, nextProcess);
					pipeline.DrawCopy( commandBuffer);
					context.SetSource0( temporary);
				}
				else
				{
					commandBuffer.SetRenderTarget( 
						new RenderTargetIdentifier( temporary), 
						RenderBufferLoadAction.DontCare,
						RenderBufferStoreAction.Store,
						RenderBufferLoadAction.DontCare,
						RenderBufferStoreAction.DontCare);
					commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, context.target0);
					pipeline.SetViewport( commandBuffer, nextProcess);
					pipeline.DrawCopy( commandBuffer);
					context.SetTarget0( temporary);
				}
			}
			else
			{
				commandBuffer.SetRenderTarget( 
					context.target0, 
					RenderBufferLoadAction.DontCare,
					RenderBufferStoreAction.Store,
					RenderBufferLoadAction.DontCare,
					RenderBufferStoreAction.DontCare);
				commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, context.source0);
				pipeline.SetViewport( commandBuffer, nextProcess);
				pipeline.DrawCopy( commandBuffer);
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
		
		[SerializeField]
		MosaicSettings sharedSettings = default;
		[SerializeField]
		MosaicProperties properties = default;
		[SerializeField]
		bool useSharedProperties = true;
		[SerializeField]
		Shader shader = default;
		[System.NonSerialized]
		Material material;
	}
}
