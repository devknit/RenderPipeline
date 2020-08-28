﻿
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	[System.Serializable]
	public sealed partial class EdgeDetection : PostProcess
	{
	public enum DetectType : int
		{
			kCheap,
			kThin
		}
		internal override void Create()
		{
			if( edgeDetectShader != null && edgeDetectMaterial == null)
			{
				edgeDetectMaterial = new Material( edgeDetectShader);
			}
		}
		internal override void Dispose()
		{
			if( edgeDetectMaterial != null)
			{
				Destroy( edgeDetectMaterial);
				edgeDetectMaterial = null;
			}
		}
		internal override bool Valid()
		{
			return enabled != false && edgeDetectMaterial != null;
		}
		internal override DepthTextureMode GetDepthTextureMode()
		{
			return DepthTextureMode.Depth;
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
				if( cacheDetectType != detectType)
				{
					rebuild = true;
					cacheDetectType = detectType;
				}
				if( cacheColor != color)
				{
					edgeDetectMaterial.SetColor( kShaderPropertyColor, color);
					cacheColor = color;
				}
				if( cacheSampleDistance != sampleDistance)
				{
					if( sampleDistance < 0)
					{
						sampleDistance = 0;
					}
					if( sampleDistance > 5)
					{
						sampleDistance = 5;
					}
					edgeDetectMaterial.SetFloat( kShaderPropertySampleDistance, sampleDistance);
					cacheSampleDistance = sampleDistance;
				}
				if( cacheStencilReference != stencilReference)
				{
					stencilReference = Mathf.Clamp( stencilReference, 0, 255);
					edgeDetectMaterial.SetInt( kShaderPropertyStencilRef, stencilReference);
					cacheStencilReference = stencilReference;
				}
				if( cacheStencilReadMask != stencilReadMask)
				{
					stencilReadMask = Mathf.Clamp( stencilReadMask, 0, 255);
					edgeDetectMaterial.SetInt( kShaderPropertyStencilReadMask, stencilReadMask);
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
					edgeDetectMaterial.SetInt( kShaderPropertyStencilComp, (int)stencilCompare);
					cacheStencilCompare = stencilCompare;
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
					pipeline.Blit( commandBuffer, context.source0, new RenderTargetIdentifier( temporary));
					context.SetTarget0( temporary);
				}
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
			pipeline.DrawFill( commandBuffer, edgeDetectMaterial, (int)detectType);
			context.duplicated = false;
		}
		
		static readonly int kShaderPropertyMainTex = Shader.PropertyToID( "_MainTex");
		static readonly int kShaderPropertyColor = Shader.PropertyToID( "_Color");
		static readonly int kShaderPropertySampleDistance = Shader.PropertyToID( "_SampleDistance");
		static readonly int kShaderPropertyStencilRef = Shader.PropertyToID( "_StencilRef");
		static readonly int kShaderPropertyStencilReadMask = Shader.PropertyToID( "_StencilReadMask");
		static readonly int kShaderPropertyStencilComp = Shader.PropertyToID( "_StencilComp");
		
		[SerializeField]
        Shader edgeDetectShader = default;
		[SerializeField]
		DetectType detectType = DetectType.kThin;
		[SerializeField]
		Color color = new Color32( 63, 13, 16, 255);
		[SerializeField, Range( 0, 5)]
		int sampleDistance = 2;
		[SerializeField, Range(0, 255)]
		int stencilReference = 1;
		[SerializeField, Range(0, 255)]
		int stencilReadMask = 255;
		[SerializeField]
		CompareFunction stencilCompare = CompareFunction.Equal;
		
		Material edgeDetectMaterial;
		
		bool? cacheEnabled;
		DetectType? cacheDetectType;
		Color? cacheColor;
		int? cacheSampleDistance;
		int? cacheStencilReference;
		int? cacheStencilReadMask;
		CompareFunction? cacheStencilCompare;
	}
}
