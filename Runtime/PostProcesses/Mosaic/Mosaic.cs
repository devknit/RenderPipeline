
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	[System.Serializable]
	public sealed partial class Mosaic : InternalProcess
	{
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
			return enabled != false && material != null;
		}
		public override void ClearPropertiesCache()
		{
			cacheEnabled = null;
			cacheWidth = null;
			cacheHeight = null;
			cacheBlockSize = null;
			cacheStencilReference = null;
			cacheStencilReadMask = null;
			cacheStencilCompare = null;
		}
		public override PostProcessEvent GetPostProcessEvent()
		{
			return PostProcessEvent.BeforeImageEffects;
		}
		public override bool UpdateProperties( RenderPipeline pipeline, bool clearCache)
		{
			bool rebuild = false;
			
			if( clearCache != false)
			{
				ClearPropertiesCache();
			}
			if( cacheEnabled != enabled)
			{
				rebuild = true;
				cacheEnabled = enabled;
			}
			if( enabled != false)
			{
				if( cacheWidth != Screen.width
				||	cacheHeight != Screen.height
				||	cacheBlockSize != blockSize)
				{
					if( blockSize < 1)
					{
						blockSize = 1;
					}
					float texelWidth = 1.0f / (float)Screen.width * blockSize;
					float texelHeight = 1.0f / (float)Screen.height * blockSize;
					material.SetVector( kShaderPropertyPixelation, new Vector4(
						1.0f / texelWidth, 1.0f / texelHeight, texelWidth, texelHeight));
					cacheWidth = Screen.width;
					cacheHeight = Screen.height;
					cacheBlockSize = blockSize;
				}
				if( cacheStencilReference != stencilReference)
				{
					stencilReference = Mathf.Clamp( stencilReference, 0, 255);
					material.SetInt( kShaderPropertyStencilRef, stencilReference);
					cacheStencilReference = stencilReference;
				}
				if( cacheStencilReadMask != stencilReadMask)
				{
					stencilReadMask = Mathf.Clamp( stencilReadMask, 0, 255);
					material.SetInt( kShaderPropertyStencilReadMask, stencilReadMask);
					cacheStencilReadMask = stencilReadMask;
				}
				if( cacheStencilCompare != stencilCompare)
				{
					if( stencilCompare == CompareFunction.Disabled)
					{
						stencilCompare = CompareFunction.Always;
					}
					bool always = stencilCompare == CompareFunction.Always;
					bool cacheAlways = false;
					
					if( cacheStencilCompare.HasValue != false)
					{
						cacheAlways = cacheStencilCompare.Value == CompareFunction.Always;
					}
					if( cacheAlways != always)
					{
						rebuild = true;
					}
					material.SetInt( kShaderPropertyStencilComp, (int)stencilCompare);
					cacheStencilCompare = stencilCompare;
				}
			}
			return rebuild;
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
			if( context.duplicated != false)
			{
				context.Copy( TargetType.kSource0, TargetType.kSource1);
			}
			else if( context.CompareSource0ToTarget0() != false)
			{
				int temporary = pipeline.GetTemporaryRT();
				if( nextProcess == null)
				{
					pipeline.Blit( commandBuffer, context.source0, new RenderTargetIdentifier( temporary));
					context.SetSource0( temporary);
				}
				else
				{
					pipeline.Blit( commandBuffer, context.target0, new RenderTargetIdentifier( temporary));
					context.SetTarget0( temporary);
				}
			}
			else
			{
				commandBuffer.Blit( context.source0, context.target0);
			}
			commandBuffer.SetRenderTarget( 
				context.target0, 
				(stencilCompare != CompareFunction.Always)? 
					RenderBufferLoadAction.Load    :
					RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.Store,
				context.depthBuffer,
				RenderBufferLoadAction.Load,	
				RenderBufferStoreAction.DontCare);
			commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, context.source0);
			pipeline.DrawFill( commandBuffer, material, 0);
			context.duplicated = false;
		}
		
		static readonly int kShaderPropertyPixelation = Shader.PropertyToID( "_Pixelation");
		static readonly int kShaderPropertyStencilRef = Shader.PropertyToID( "_StencilRef");
		static readonly int kShaderPropertyStencilReadMask = Shader.PropertyToID( "_StencilReadMask");
		static readonly int kShaderPropertyStencilComp = Shader.PropertyToID( "_StencilComp");
		
		[SerializeField]
        Shader shader = default;
        [SerializeField]
        int blockSize = 16;
		[SerializeField, Range(0, 255)]
		int stencilReference = 0;
		[SerializeField, Range(0, 255)]
		int stencilReadMask = 255;
		[SerializeField]
		CompareFunction stencilCompare = CompareFunction.Equal;
		
		Material material;
		
		bool? cacheEnabled;
		int? cacheWidth;
		int? cacheHeight;
		int? cacheBlockSize;
		int? cacheStencilReference;
		int? cacheStencilReadMask;
		CompareFunction? cacheStencilCompare;
	}
}
