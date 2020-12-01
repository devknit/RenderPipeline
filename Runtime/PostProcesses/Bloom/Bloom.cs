#if !UNITY_EDITOR
	#if UNITY_IOS || UNITY_ANDROID
		#define WITH_CLEARRENDERTARGET
	#endif
#endif

using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace RenderPipeline
{
	[System.Serializable]
	public sealed partial class Bloom : PostProcess
	{
		public BloomProperties Properties
		{
			get{ return (sharedSettings != null && useSharedProperties != false)? sharedSettings.properties : properties; }
		}
		public override void Create()
		{
			if( shader != null && material == null)
			{
				material = new Material( shader);
			}
			brightnessExtractionMesh = new Mesh();
			brightnessExtractionMesh.MarkDynamic();
			blurHorizontalMesh = new Mesh();
			blurHorizontalMesh.MarkDynamic();
			blurVerticalMesh = new Mesh();
			blurVerticalMesh.MarkDynamic();
			combineMesh = new Mesh();
			combineMesh.MarkDynamic();
		}
		public override void Dispose()
		{
			if( material != null)
			{
				ObjectUtility.Release( material);
				material = null;
			}
			if( brightnessExtractionMesh != null)
			{
				ObjectUtility.Release( brightnessExtractionMesh);
				brightnessExtractionMesh = null;
			}
			if( blurHorizontalMesh != null)
			{
				ObjectUtility.Release( blurHorizontalMesh);
				blurHorizontalMesh = null;
			}
			if( blurVerticalMesh != null)
			{
				ObjectUtility.Release( blurVerticalMesh);
				blurVerticalMesh = null;
			}
			if( combineMesh != null)
			{
				ObjectUtility.Release( combineMesh);
				combineMesh = null;
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
			Properties.ClearCache();
			cacheCombinePassCount = null;
			cacheBloomRectCount = null;
		#if UNITY_EDITOR
			cacheSharedSettings = null;
		#endif
		}
		public override bool UpdateProperties( RenderPipeline pipeline, bool clearCache)
		{
			int updateFlags = 0;
			
			if( clearCache != false)
			{
				Properties.ClearCache();
				cacheCombinePassCount = null;
				cacheBloomRectCount = null;
			#if UNITY_EDITOR
				cacheSharedSettings = null;
			#endif
			}
		#if UNITY_EDITOR
			if( cacheSharedSettings != sharedSettings)
			{
				sharedSettings.properties.ClearCache();
				cacheSharedSettings = sharedSettings;
			}
		#endif
			updateFlags |= Properties.CheckParameterChange( pipeline);
			
			if( (updateFlags & BloomProperties.kVerifyDescriptors) != 0)
			{
				updateFlags |= UpdateDescriptors( Properties.ScreenWidth, Properties.ScreenHeight);
			}
			if( updateFlags != 0 && bloomRects != null)
			{
				if( UpdateResources( updateFlags) != false)
				{
					updateFlags |= BloomProperties.kRebuild;
				}
			}
			return (updateFlags & BloomProperties.kRebuild) != 0;
		}
		public override PostProcessEvent GetPostProcessEvent()
		{
			return PostProcessEvent.BeforeImageEffects;
		}
		public override DepthTextureMode GetDepthTextureMode()
		{
			return DepthTextureMode.None;
		}
		public override bool IsRequiredHighDynamicRange()
		{
			return true;
		}
		public override void BuildCommandBuffer( RenderPipeline pipeline,
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
			var brightnessExtractionTarget = new RenderTargetIdentifier( kShaderPropertyBrightnessExtractionTarget);
			commandBuffer.GetTemporaryRT( kShaderPropertyBrightnessExtractionTarget, brightnessExtractionDescriptor, FilterMode.Bilinear);
			
			commandBuffer.SetRenderTarget( 
				brightnessExtractionTarget,
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.Store,
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.DontCare);
		#if WITH_CLEARRENDERTARGET
			commandBuffer.ClearRenderTarget( false, true, Color.black, 0);
		#endif
			commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, context.source0);
			commandBuffer.DrawMesh( brightnessExtractionMesh, Matrix4x4.identity, material, 0, 0);
		
			var gaussianBlurHorizontalTarget = new RenderTargetIdentifier( kShaderPropertyGaussianBlurHorizontalTarget);
			commandBuffer.GetTemporaryRT( kShaderPropertyGaussianBlurHorizontalTarget, blurDescriptor, FilterMode.Bilinear);
			
			commandBuffer.SetRenderTarget( 
				kShaderPropertyGaussianBlurHorizontalTarget,
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.Store,
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.DontCare);
		#if WITH_CLEARRENDERTARGET
			commandBuffer.ClearRenderTarget( false, true, Color.black, 0);
		#endif
			commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, brightnessExtractionTarget);
			commandBuffer.DrawMesh( blurHorizontalMesh, Matrix4x4.identity, material, 0, 1);
			
			var gaussianBlurVerticalTarget = new RenderTargetIdentifier( kShaderPropertyGaussianBlurVerticalTarget);
			commandBuffer.GetTemporaryRT( kShaderPropertyGaussianBlurVerticalTarget, blurDescriptor, FilterMode.Bilinear);
			
			commandBuffer.SetRenderTarget( 
				gaussianBlurVerticalTarget,
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.Store,
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.DontCare);
		#if WITH_CLEARRENDERTARGET
			commandBuffer.ClearRenderTarget( false, true, Color.black, 0);
		#endif
			commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, kShaderPropertyGaussianBlurHorizontalTarget);
			commandBuffer.DrawMesh( blurVerticalMesh, Matrix4x4.identity, material, 0, 1);
			
			if( combinePassCount > 1)
			{
				commandBuffer.SetRenderTarget( 
					gaussianBlurHorizontalTarget,
					RenderBufferLoadAction.DontCare,	
					RenderBufferStoreAction.Store,
					RenderBufferLoadAction.DontCare,	
					RenderBufferStoreAction.DontCare);
			#if WITH_CLEARRENDERTARGET
				commandBuffer.ClearRenderTarget( false, true, Color.black, 0);
			#endif
				commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, gaussianBlurVerticalTarget);
				commandBuffer.DrawMesh( combineMesh, Matrix4x4.identity, material, 0, 2);
				commandBuffer.SetGlobalTexture( kShaderPropertyBloomTex, gaussianBlurVerticalTarget);
				commandBuffer.SetGlobalTexture( kShaderPropertyBloomCombinedTex, kShaderPropertyGaussianBlurHorizontalTarget);
			}
			else
			{
				commandBuffer.SetGlobalTexture( kShaderPropertyBloomCombinedTex, gaussianBlurVerticalTarget);
			}
			commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, context.source0);
			
			if( ((nextProcess as InternalProcess)?.DuplicateMRT() ?? false) != false)
			{
				int temporary = pipeline.GetTemporaryRT();
				context.SetTarget1( temporary);
				commandBuffer.SetRenderTarget( 
					new RenderTargetBinding( 
					new []{ context.target0, context.target1 },
					new []{ RenderBufferLoadAction.DontCare, RenderBufferLoadAction.DontCare },
					new []{ RenderBufferStoreAction.Store, RenderBufferStoreAction.Store },
					context.depthBuffer,
					RenderBufferLoadAction.DontCare,
					RenderBufferStoreAction.DontCare));
				pipeline.SetViewport( commandBuffer, nextProcess);
				pipeline.DrawFill( commandBuffer, material, 4);
				context.duplicated = true;
			}
			else
			{
				commandBuffer.SetRenderTarget( 
					context.target0,
					RenderBufferLoadAction.DontCare,	
					RenderBufferStoreAction.Store,
					RenderBufferLoadAction.DontCare,	
					RenderBufferStoreAction.DontCare);
				pipeline.SetViewport( commandBuffer, nextProcess);
				pipeline.DrawFill( commandBuffer, material, 3);
				context.duplicated = false;
			}
			commandBuffer.ReleaseTemporaryRT( kShaderPropertyGaussianBlurVerticalTarget);
			commandBuffer.ReleaseTemporaryRT( kShaderPropertyGaussianBlurHorizontalTarget);
			commandBuffer.ReleaseTemporaryRT( kShaderPropertyBrightnessExtractionTarget);
		}
		int UpdateDescriptors( int width, int height)
		{
			int updateFlags = 0;
			
			if( width > 0 && height > 0)
			{
				var renderTextureFormat = RenderTextureFormat.ARGB32;
				
				if( SystemInfo.SupportsRenderTextureFormat( RenderTextureFormat.DefaultHDR) != false)
				{
					renderTextureFormat = RenderTextureFormat.DefaultHDR;
				}
				int topBloomWidth = width >> Properties.DownSampleLevel;
				int topBloomHeight = height >> Properties.DownSampleLevel;
				
				brightnessExtractionDescriptor = new RenderTextureDescriptor(
					TextureUtil.ToPow2RoundUp( topBloomWidth), 
					TextureUtil.ToPow2RoundUp( topBloomHeight), 
					renderTextureFormat, 0);
				brightnessExtractionDescriptor.useMipMap = true;
				brightnessExtractionDescriptor.autoGenerateMips = true;
				brightnessNetWidth = width >> Properties.DownSampleLevel;
				brightnessNetHeight = height >> Properties.DownSampleLevel;
				brightnessOffsetX = (brightnessExtractionDescriptor.width - brightnessNetWidth) / 2;
				brightnessOffsetY = (brightnessExtractionDescriptor.height - brightnessNetHeight) / 2;
				
				int bloomWidth, bloomHeight;
				
				bloomRects = CalculateBloomRenderTextureArrangement(
					out bloomWidth,
					out bloomHeight,
					topBloomWidth,
					topBloomHeight,
					16,
					Properties.DownSampleCount);
				
				blurDescriptor = new RenderTextureDescriptor(
					bloomWidth, bloomHeight, renderTextureFormat, 0);
				blurDescriptor.useMipMap = false;
				blurDescriptor.autoGenerateMips = false;
				
				combineDescriptor = new RenderTextureDescriptor(
					bloomWidth, bloomHeight, renderTextureFormat, 0);
				combineDescriptor.useMipMap = false;
				combineDescriptor.autoGenerateMips = false;
				
				updateFlags |= BloomProperties.kChangeBrightnessExtractionMesh;
				updateFlags |= BloomProperties.kChangeGaussianBlurMesh;
				updateFlags |= BloomProperties.kChangeCombineMesh;
				updateFlags |= BloomProperties.kChangeBloomRects;
			}
			return updateFlags;
		}
		BloomRect[] CalculateBloomRenderTextureArrangement(
			out int dstWidth,
			out int dstHeight,
			int width,
			int height,
			int padding,
			int levelCount)
		{
			var rects = new List<BloomRect>();
			bool right = height > width;
			int x = padding;
			int y = padding;
			int maxX = 0;
			int maxY = 0;
			
			while( levelCount > 0 && width > 0 && height > 0)
			{
				var rect = new BloomRect();
				rect.x = x;
				rect.y = y;
				rect.width = width;
				rect.height = height;
				rect.uvTransformShaderPropertyId = Shader.PropertyToID( "_BloomUvTransform" + rects.Count);
				rect.weightShaderPropertyId = Shader.PropertyToID( "_BloomWeight" + rects.Count);
				rects.Add( rect);
				maxX = Mathf.Max( maxX, x + width + padding);
				maxY = Mathf.Max( maxY, y + height + padding);
				if( right != false)
				{
					x += width + padding;
				}
				else
				{
					y += height + padding;
				}
				right = !right;
				
				width /= 2;
				height /= 2;
				
				if( width < 0 || height < 0)
				{
					break;
				}
				levelCount--;
			}
			dstWidth = maxX;
			dstHeight = maxY;
			
			return rects.ToArray();
		}
		float CalculateGaussianSamples( float sigma)
		{
			float w0 = Gauss( sigma, 0.0f) * 0.5f;
			float w1 = Gauss( sigma, 1.0f);
			float w2 = Gauss( sigma, 2.0f);
			float w3 = Gauss( sigma, 3.0f);
			float w4 = Gauss( sigma, 4.0f);
			float w5 = Gauss( sigma, 5.0f);
			float w6 = Gauss( sigma, 6.0f);
			float w7 = Gauss( sigma, 7.0f);
			
			float w01 = w0 + w1;
			float x01 = (w01 != 0.0f)? 0.0f + (w1 / w01) : 0.0f;
			float w23 = w2 + w3;
			float x23 = (w23 != 0.0f)? 2.0f + (w3 / w23) : 0.0f;
			float w45 = w4 + w5;
			float x45 = (w45 != 0.0f)? 4.0f + (w5 / w45) : 0.0f;
			float w67 = w6 + w7;
			float x67 = (w67 != 0.0f)? 6.0f + (w7 / w67) : 0.0f;
			float wSum = (w01 + w23 + w45 + w67) * 2.0f;
			float iSum = (wSum != 0.0f)? 1.0f / wSum : 0.0f;
			
			w01 *= iSum;
			w23 *= iSum;
			w45 *= iSum;
			w67 *= iSum;
			
			blurSample0.offset = x01;
			blurSample0.weight = w01;
			blurSample1.offset = x23;
			blurSample1.weight = w23;
			blurSample2.offset = x45;
			blurSample2.weight = w45;
			blurSample3.offset = x67;
			blurSample3.weight = w67;
			
			float x0123 = x01 - x23;
			return (x0123 != 0.0f)? (x01 * 2.0f) / Mathf.Abs( x0123) : 0.0f;
		}
		static float Gauss( float sigma, float x)
		{
			if( sigma == 0.0)
			{
				return 0.0f;
			}
			return Mathf.Exp( -(x * x) / (sigma * sigma * 2.0f));
		}
		
		const string kShaderKeywordLDR = "LDR";
		
		const string kShaderKeywordCombineSample1 = "COMBINE_SAMPLE1";
		const string kShaderKeywordCombineSample2 = "COMBINE_SAMPLE2";
		const string kShaderKeywordCombineSample4 = "COMBINE_SAMPLE4";
		const string kShaderKeywordCompositionSample1 = "COMPOSITION_SAMPLE1";
		const string kShaderKeywordCompositionSample2 = "COMPOSITION_SAMPLE2";
		const string kShaderKeywordCompositionSample4 = "COMPOSITION_SAMPLE4";
		const string kShaderKeywordCompositionCombined = "COMPOSITION_COMBINED";
		
		static readonly int kShaderPropertyBrightnessExtractionTarget = Shader.PropertyToID( "_BrightnessExtractionTarget");
		static readonly int kShaderPropertyGaussianBlurHorizontalTarget = Shader.PropertyToID( "_GaussianBlurHorizontalTarget");
		static readonly int kShaderPropertyGaussianBlurVerticalTarget = Shader.PropertyToID( "_GaussianBlurVerticalTarget");
		static readonly int kShaderPropertyCombineTarget = Shader.PropertyToID( "_CombineTarget");
		static readonly int kShaderPropertyBloomTex = Shader.PropertyToID( "_BloomTex");
		static readonly int kShaderPropertyBloomCombinedTex = Shader.PropertyToID( "_BloomCombinedTex");
		static readonly int kShaderPropertyThresholds = Shader.PropertyToID( "_Thresholds");
		static readonly int kShaderPropertyColorTransform = Shader.PropertyToID( "_ColorTransform");
		static readonly int kShaderPropertyInvertOffsetScale01 = Shader.PropertyToID( "_InvertOffsetScale01");
		static readonly int[] kShaderPropertyUvTransforms = new int[]
		{
			Shader.PropertyToID( "_UvTransform0"),
			Shader.PropertyToID( "_UvTransform1"),
			Shader.PropertyToID( "_UvTransform2"),
			Shader.PropertyToID( "_UvTransform3"),
			Shader.PropertyToID( "_UvTransform4"),
			Shader.PropertyToID( "_UvTransform5"),
			Shader.PropertyToID( "_UvTransform6")
		};
		static readonly int[] kShaderPropertyWeights = new int[]
		{
			Shader.PropertyToID( "_Weight0"),
			Shader.PropertyToID( "_Weight1"),
			Shader.PropertyToID( "_Weight2"),
			Shader.PropertyToID( "_Weight3"),
			Shader.PropertyToID( "_Weight4"),
			Shader.PropertyToID( "_Weight5"),
			Shader.PropertyToID( "_Weight6")
		};
		static readonly int kShaderPropertyBloomWeightCombined = Shader.PropertyToID( "_BloomWeightCombined");
		static readonly int kShaderPropertyBloomUvTransformCombined = Shader.PropertyToID( "_BloomUvTransformCombined");
		
        [SerializeField]
        BloomSettings sharedSettings = default;
        [SerializeField]
        BloomProperties properties = default;
		[SerializeField]
		bool useSharedProperties = true;
        [SerializeField]
		Shader shader = default;
		
		Material material;
		
		Mesh brightnessExtractionMesh;
		Mesh blurHorizontalMesh;
		Mesh blurVerticalMesh;
		Mesh combineMesh;
		
		RenderTextureDescriptor brightnessExtractionDescriptor;
		RenderTextureDescriptor blurDescriptor;
		RenderTextureDescriptor combineDescriptor;
		
		BlurSample blurSample0;
		BlurSample blurSample1;
		BlurSample blurSample2;
		BlurSample blurSample3;
		BloomRect[] bloomRects;
		
		int brightnessOffsetX;
		int brightnessOffsetY;
		int brightnessNetWidth;
		int brightnessNetHeight;
		int combinePassCount;
		int bloomRectCount;
		int? cacheCombinePassCount;
		int? cacheBloomRectCount;
		
	#if UNITY_EDITOR
		BloomSettings cacheSharedSettings;
	#endif
	}
	class BloomRect
	{
		public int x;
		public int y;
		public int width;
		public int height;
		public int uvTransformShaderPropertyId;
		public int weightShaderPropertyId;
	}
	struct BlurSample
	{
		public float offset;
		public float weight;
	}
}
