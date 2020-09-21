
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	[System.Serializable]
	public sealed partial class Mosaic : PostProcess
	{
		internal override void Create()
		{
			if( shader != null && material == null)
			{
				material = new Material( shader);
			}
		}
		internal override void Dispose()
		{
			if( material != null)
			{
				ObjectUtility.Release( material);
				material = null;
			}
		}
		internal override bool RestoreResources()
		{
			bool rebuild = false;
			
			if( shader != null && material == null)
			{
				material = new Material( shader);
				rebuild = true;
			}
			return rebuild;
		}
		internal override bool Valid()
		{
			return enabled != false && material != null;
		}
		internal override void ClearCache()
		{
			cacheEnabled = null;
			cacheWidth = null;
			cacheHeight = null;
			cacheBlockSize = null;
			cacheStencilReference = null;
			cacheStencilReadMask = null;
			cacheStencilCompare = null;
		}
		internal override bool CheckParameterChange( bool clearCache)
		{
			bool rebuild = false;
			
			if( clearCache != false)
			{
				ClearCache();
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
		internal override DepthTextureMode GetDepthTextureMode()
		{
			return DepthTextureMode.None;
		}
		internal override bool IsHighDynamicRange()
		{
			return false;
		}
		protected override bool OnDuplicate()
		{
			return nextProcess != null;
			//return true;
		}
		internal override void BuildCommandBuffer( 
			CommandBuffer commandBuffer, TargetContext context, 
			System.Func<int, int, int, FilterMode, RenderTextureFormat, int> GetTemporaryRT)
		{
			if( context.duplicated != false)
			{
				context.Copy( TargetType.kSource0, TargetType.kSource1);
			}
			else if( context.CompareSource0ToTarget0() != false)
			{
				var renderTextureFormat = RenderTextureFormat.ARGB32;
				
				if( SystemInfo.SupportsRenderTextureFormat( RenderTextureFormat.RGB111110Float) != false)
				{
					renderTextureFormat = RenderTextureFormat.RGB111110Float;
				}
				int temporary = GetTemporaryRT( -1, -1, 0, FilterMode.Bilinear, renderTextureFormat);
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
			commandBuffer.SetGlobalTexture( kShaderPropertyMainTex, context.source0);
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
