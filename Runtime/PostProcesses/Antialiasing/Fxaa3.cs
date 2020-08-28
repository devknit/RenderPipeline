
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	[System.Serializable]
	public sealed partial class Fxaa3 : PostProcess
	{
		internal override void Create()
		{
			if( shaderFxaa3 != null && materialFxaa3 == null)
			{
				materialFxaa3 = new Material( shaderFxaa3);
			}
		}
		internal override void Dispose()
		{
			if( materialFxaa3 != null)
			{
				Destroy( materialFxaa3);
				materialFxaa3 = null;
			}
		}
		internal override bool Valid()
		{
			return enabled != false && materialFxaa3 != null;
		}
		internal override DepthTextureMode GetDepthTextureMode()
		{
			return DepthTextureMode.None;
		}
		internal override bool IsHighDynamicRange()
		{
			return false;
		}
		internal override bool CheckParameterChange()
		{
			bool rebuild = false;
			
			if( cacheEnabled != enabled)
			{
				rebuild = true;
				cacheEnabled = enabled;
			}
			if( enabled != false)
			{
				if( cacheEdgeThresholdMin != edgeThresholdMin)
				{
					materialFxaa3.SetFloat( kShaderPropertyEdgeThresholdMin, edgeThresholdMin);
					cacheEdgeThresholdMin = edgeThresholdMin;
				}
				if( cacheEdgeThreshold != edgeThreshold)
				{
					materialFxaa3.SetFloat( kShaderPropertyEdgeThreshold, edgeThreshold);
					cacheEdgeThreshold = edgeThreshold;
				}
				if( cacheEdgeSharpness != edgeSharpness)
				{
					materialFxaa3.SetFloat( kShaderPropertyEdgeSharpness, edgeSharpness);
					cacheEdgeSharpness = edgeSharpness;
				}
			}
			return rebuild;
		}
		protected override bool OnDuplicate()
		{
			return false;
		}
		internal override void BuildCommandBuffer( 
			CommandBuffer commandBuffer, TargetContext context, 
			System.Func<int, int, int, FilterMode, RenderTextureFormat, int> GetTemporaryRT)
		{
			if( context.CompareSource0ToTarget0() != false)
			{
				int temporary = GetTemporaryRT( -1, -1, 0, FilterMode.Bilinear, TextureUtil.DefaultHDR);
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
			commandBuffer.SetRenderTarget( 
				context.target0, 
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.Store,
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.DontCare);
			commandBuffer.SetGlobalTexture( kShaderPropertyMainTex, context.source0);
			pipeline.DrawFill( commandBuffer, materialFxaa3, 0);
			context.duplicated = false;
		}
		
		static readonly int kShaderPropertyMainTex = Shader.PropertyToID( "_MainTex");
		static readonly int kShaderPropertyEdgeThresholdMin = Shader.PropertyToID( "_EdgeThresholdMin");
		static readonly int kShaderPropertyEdgeThreshold = Shader.PropertyToID( "_EdgeThreshold");
		static readonly int kShaderPropertyEdgeSharpness = Shader.PropertyToID( "_EdgeSharpness");
		
		[SerializeField]
        Shader shaderFxaa3 = default;
		[SerializeField]
		float edgeThresholdMin = 0.05f;
		[SerializeField]
        float edgeThreshold = 0.1f;//0.2f;
        [SerializeField]
        float edgeSharpness = 4.0f;
		
		Material materialFxaa3;
		
		bool? cacheEnabled;
		float? cacheEdgeThresholdMin = 0.05f;
        float? cacheEdgeThreshold = 0.2f;
        float? cacheEdgeSharpness = 4.0f;
	}
}
