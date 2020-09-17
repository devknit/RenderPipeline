
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace RenderPipeline
{
	[System.Serializable]
	public sealed partial class DepthOfField : PostProcess
	{
		public enum BlurQuality : int
		{
			kLow = 0,
			kMedium = 1,
			kHigh = 2,
		}
		internal override void Create()
		{
			if( depthOfFieldShader != null && depthOfFieldMaterial == null)
			{
				depthOfFieldMaterial = new Material( depthOfFieldShader);
			}
		}
		internal override void Dispose()
		{
			if( depthOfFieldMaterial != null)
			{
				ObjectUtility.Release( depthOfFieldMaterial);
				depthOfFieldMaterial = null;
			}
		}
		internal override bool RestoreResources()
		{
			bool rebuild = false;
			
			if( ObjectUtility.IsMissing( depthOfFieldMaterial) != false)
			{
				depthOfFieldMaterial = new Material( depthOfFieldShader);
				rebuild = true;
			}
			return rebuild;
		}
		internal override bool Valid()
		{
			return enabled != false && depthOfFieldMaterial != null;
		}
		internal override DepthTextureMode GetDepthTextureMode()
		{
			return DepthTextureMode.Depth;
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
				cacheWidth = null;
				cacheHeight = null;
				cacheFocalSize = null;
				cacheAperture = null;
				cacheMaxBlurSize = null;
				cacheBlurQuality = null;
				cacheHighResolution = null;
				cacheVisualizeFocus = null;
			}
			if( cacheEnabled != enabled)
			{
				rebuild = true;
				cacheEnabled = enabled;
			}
			if( enabled != false)
			{
				bool updateOffsets = true;
				
				if( cacheWidth != Screen.width
				||	cacheHeight != Screen.height
				||	cacheHighResolution != highResolution)
				{
					rebuild = true;
					updateOffsets = true;
					cacheWidth = Screen.width;
					cacheHeight = Screen.height;
					cacheHighResolution = highResolution;
				}
				if( cacheBlurQuality != blurQuality
				||	cacheVisualizeFocus != visualizeFocus)
				{
					rebuild = true;
					cacheBlurQuality = blurQuality;
					cacheVisualizeFocus = visualizeFocus;
				}
				if( cacheFocalSize != focalSize)
				{
					focalSize = Mathf.Clamp( focalSize, 0.0f, 2.0f);
					cacheFocalSize = focalSize;
				}
				if( cacheAperture != aperture)
				{
					if( aperture < 0.0f)
					{
						aperture = 0.0f;
					}
					cacheAperture = aperture;
				}
				if( cacheMaxBlurSize != maxBlurSize)
				{
					if( maxBlurSize < 0.1f)
					{
						maxBlurSize = 0.1f;
					}
					updateOffsets = true;
					blurWidth = Mathf.Max( maxBlurSize, 0.0f);
					cacheMaxBlurSize = maxBlurSize;
				}
				Camera cacheCamera = pipeline.CacheCamera;
				
				float focalDistance01 = (focalTransform == null)?
					cacheCamera.WorldToViewportPoint( 
						(focalLength - cacheCamera.nearClipPlane) * cacheCamera.transform.forward + 
						cacheCamera.transform.position).z / (cacheCamera.farClipPlane - cacheCamera.nearClipPlane):
						(cacheCamera.WorldToViewportPoint( focalTransform.position)).z / (cacheCamera.farClipPlane);
				depthOfFieldMaterial.SetVector( kShaderPropertyCurveParams,
					new Vector4( 1.0f, focalSize, (1.0f / (1.0f - aperture) - 1.0f), focalDistance01));
				
				if( updateOffsets != false)
				{
					depthOfFieldMaterial.SetVector( 
						kShaderPropertyOffsets, (highResolution != false)?
						new Vector4( 0.025f, blurWidth * 2.0f, 0, 0) : 
						new Vector4( 0.1f, blurWidth, (Screen.width / (Screen.width >> 1)) * blurWidth, 0));
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
			if( context.CompareSource0ToTarget0() != false && nextProcess != null)
			{
				var renderTextureFormat = RenderTextureFormat.ARGB32;
				
				if( SystemInfo.SupportsRenderTextureFormat( RenderTextureFormat.RGB111110Float) != false)
				{
					renderTextureFormat = RenderTextureFormat.RGB111110Float;
				}
				int temporary = GetTemporaryRT( -1, -1, 0, FilterMode.Bilinear, renderTextureFormat);
				context.SetTarget0( temporary);
			}
			if( visualizeFocus != false)
			{
				commandBuffer.SetRenderTarget( 
					context.target0, 
					RenderBufferLoadAction.DontCare,	
					RenderBufferStoreAction.Store,
					RenderBufferLoadAction.DontCare,	
					RenderBufferStoreAction.DontCare);
				pipeline.DrawFill( commandBuffer, depthOfFieldMaterial, 7);
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
				commandBuffer.SetGlobalTexture( kShaderPropertyMainTex, context.source0);
				pipeline.DrawFill( commandBuffer, depthOfFieldMaterial, 0);
				
				int pass = (blurQuality == BlurQuality.kLow)? 2 : 3;
				
				if( highResolution != false)
				{
					commandBuffer.SetRenderTarget( 
						context.target0, 
						RenderBufferLoadAction.DontCare,	
						RenderBufferStoreAction.Store,
						RenderBufferLoadAction.DontCare,	
						RenderBufferStoreAction.DontCare);
					commandBuffer.SetGlobalTexture( kShaderPropertyMainTex, alphaDepthTarget);
					pipeline.DrawFill( commandBuffer, depthOfFieldMaterial, pass);
				}
				else
				{
					int lowWidth = Screen.width >> 1;
					int lowHeight = Screen.height >> 1;
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
					commandBuffer.SetGlobalTexture( kShaderPropertyMainTex, alphaDepthTarget);
					pipeline.DrawFill( commandBuffer, depthOfFieldMaterial, 1);
					
					commandBuffer.SetRenderTarget( 
						lowDiscTarget, 
						RenderBufferLoadAction.DontCare,	
						RenderBufferStoreAction.Store,
						RenderBufferLoadAction.DontCare,	
						RenderBufferStoreAction.DontCare);
					commandBuffer.SetGlobalTexture( kShaderPropertyMainTex, lowBlurTarget);
					pipeline.DrawFill( commandBuffer, depthOfFieldMaterial, pass);
					
					switch( blurQuality)
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
					commandBuffer.SetGlobalTexture( kShaderPropertyMainTex, alphaDepthTarget);
					commandBuffer.SetGlobalTexture( kShaderPropertyLowRez, lowDiscTarget);
					pipeline.DrawFill( commandBuffer, depthOfFieldMaterial, pass);
					
					commandBuffer.ReleaseTemporaryRT( kShaderPropertyLowBlurTarget);
					commandBuffer.ReleaseTemporaryRT( kShaderPropertyLowDiscTarget);
				}
				commandBuffer.ReleaseTemporaryRT( kShaderPropertyAlphaDepthTarget);
			}
			context.duplicated = false;
		}
		
		static readonly int kShaderPropertyCurveParams = Shader.PropertyToID( "_CurveParams");
		static readonly int kShaderPropertyOffsets = Shader.PropertyToID( "_Offsets");
		static readonly int kShaderPropertyAlphaDepthTarget = Shader.PropertyToID( "_AlphaDepthTarget");
		static readonly int kShaderPropertyLowBlurTarget = Shader.PropertyToID( "_LowBlurTarget");
		static readonly int kShaderPropertyLowDiscTarget = Shader.PropertyToID( "_LowDiscTarget");
		static readonly int kShaderPropertyLowRez = Shader.PropertyToID( "_LowRez");
		
		[SerializeField]
        Shader depthOfFieldShader = default;
		[SerializeField]
		Transform focalTransform = null;
		[SerializeField]
		float focalLength = 10.0f;
		[SerializeField, Range( 0, 2)]
		float focalSize = 0.05f;
		[SerializeField, Range( 0, 1)]
		float aperture = 0.5f;
		[SerializeField]
		float maxBlurSize = 2.0f;
		[SerializeField]
		BlurQuality blurQuality = BlurQuality.kLow;
		[SerializeField]
		bool highResolution = false;
		[SerializeField]
		bool visualizeFocus = false;
		
		Material depthOfFieldMaterial;
		float blurWidth = 2.0f;
		
		bool? cacheEnabled;
		int? cacheWidth;
		int? cacheHeight;
		float? cacheFocalSize;
		float? cacheAperture;
		float? cacheMaxBlurSize;
		BlurQuality? cacheBlurQuality;
		bool? cacheHighResolution;
		bool? cacheVisualizeFocus;
	}
}
