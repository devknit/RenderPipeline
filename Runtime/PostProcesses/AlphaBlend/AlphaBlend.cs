
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	[System.Serializable]
	public sealed partial class AlphaBlend : PostProcess
	{
		internal override void Create()
		{
			if( alphaBlendShader != null && alphaBlendMaterial == null)
			{
				alphaBlendMaterial = new Material( alphaBlendShader);
			}
		}
		internal override void Dispose()
		{
			if( alphaBlendMaterial != null)
			{
				Release( alphaBlendMaterial);
				alphaBlendMaterial = null;
			}
		}
		internal override bool Valid()
		{
			return enabled != false && alphaBlendMaterial != null;
		}
		internal override DepthTextureMode GetDepthTextureMode()
		{
			return DepthTextureMode.None;
		}
		internal override bool IsHighDynamicRange()
		{
			return false;
		}
		internal override bool CheckParameterChange( bool clearCache)
		{
			bool rebuild = false;
			
			if( clearCache != false)
			{
				cacheEnabled = null;
				cacheColor = null;
			}
			if( cacheEnabled != enabled)
			{
				rebuild = true;
				cacheEnabled = enabled;
			}
			if( enabled != false)
			{
				if( cacheColor != color)
				{
					alphaBlendMaterial.SetColor( kShaderPropertyColor, color);
					cacheColor = color;
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
			pipeline.DrawFill( commandBuffer, alphaBlendMaterial, 0);
			context.duplicated = false;
		}
		
		[SerializeField]
        Shader alphaBlendShader = default;
		[SerializeField]
		Color color = Color.clear;
		
		Material alphaBlendMaterial;
		
		bool? cacheEnabled;
		Color? cacheColor;
	}
}
