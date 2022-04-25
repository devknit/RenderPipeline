
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace RenderingPipeline
{
	[CreateAssetMenu( menuName="RenderPipeline/Bloom", fileName="PostProcessBloom", order=1200)]
	public sealed class BloomSettings : Settings<BloomProperties>
	{
	}
	[System.Serializable]
	public sealed class BloomProperties : IGenericProperties
	{
		public bool Enabled
		{
			get{ return blendWeight > 0; }
			set{ blendWeight = (value == false)? 0 : 1; }
		}
		public float BlendWeight
		{
			get{ return blendWeight; }
			set{ blendWeight = Mathf.Clamp01( value); }
		}
		public PostProcessEvent Phase
		{
			get{ return phase; }
		}
		public RenderTextureFormat Format
		{
			get{ return format; }
		}
		public float Thresholds
		{
			get{ return thresholds; }
			set{ thresholds = value; }
		}
		public float SigmaInPixel
		{
			get{ return sigmaInPixel; }
			set{ sigmaInPixel = value; }
		}
		public float Intensity
		{
			get{ return intensity; }
			set{ intensity = value; }
		}
		public float IntensityMultiplier
		{
			get{ return intensityMultiplier; }
			set{ intensityMultiplier = value; }
		}
		public int DownSampleLevel
		{
			get{ return downSampleLevel; }
			set{ downSampleLevel = value; }
		}
		public int DownSampleCount
		{
			get{ return downSampleCount; }
			set{ downSampleCount = value; }
		}
		public int CombineStartLevel
		{
			get{ return combineStartLevel; }
			set{ combineStartLevel = value; }
		}
		public int ScreenWidth
		{
			get{ return cacheScreenWidth ?? 0; }
		}
		public int ScreenHeight
		{
			get{ return cacheScreenHeight ?? 0; }
		}
		public bool Verify()
		{
			return (cacheScreenWidth ?? 0) > 0 && (cacheScreenHeight ?? 0) > 0;
		}
		public void ClearCache()
		{
			cacheBlendWeight = null;
			cacheThresholds = null;
			cacheSigmaInPixel = null;
			cacheIntensity = null;
			cacheIntensityMultiplier = null;
			cacheDownSampleLevel = null;
			cacheDownSampleCount = null;
			cacheCombineStartLevel = null;
			cacheScreenWidth = null;
			cacheScreenHeight = null;
		}
		public bool UpdateProperties( RenderPipeline pipeline, Material material, GaussianBlurResources resources)
		{
			int updateFlag = kUpdateFlagNone;
			
			if( cacheBlendWeight != blendWeight)
			{
				bool cacheEnabled = cacheBlendWeight > 0;
				bool enabled = blendWeight > 0;
				
				if( cacheEnabled != enabled)
				{
					updateFlag |= kUpdateFlagRebuild;
				}
				updateFlag |= kUpdateFlagBlendWeight;
				cacheBlendWeight = blendWeight;
			}
			if( blendWeight > 0)
			{
				if( cacheThresholds != thresholds)
				{
					updateFlag |= kUpdateFlagThresholds;
					cacheThresholds = thresholds;
				}
				if( cacheSigmaInPixel != sigmaInPixel)
				{
					updateFlag |= kUpdateFlagSigmaInPixel | kUpdateFlagGaussianBlurMesh | kUpdateFlagRebuild;
					cacheSigmaInPixel = sigmaInPixel;
				}
				if( cacheIntensity != intensity)
				{
					updateFlag |= kUpdateFlagCombineComposition;
					cacheIntensity = intensity;
				}
				if( cacheIntensityMultiplier != intensityMultiplier)
				{
					updateFlag |= kUpdateFlagCombineComposition;
					cacheIntensityMultiplier = intensityMultiplier;
				}
				if( cacheDownSampleLevel != downSampleLevel)
				{
					updateFlag |= kUpdateFlagGaussianBlurMesh | kUpdateFlagDescriptors | kUpdateFlagCombineComposition | kUpdateFlagRebuild;
					cacheDownSampleLevel = downSampleLevel;
				}
				if( cacheDownSampleCount != downSampleCount)
				{
					updateFlag |= kUpdateFlagGaussianBlurMesh | kUpdateFlagDescriptors | kUpdateFlagCombineComposition | kUpdateFlagRebuild;
					cacheDownSampleCount = downSampleCount;
				}
				if( cacheCombineStartLevel != combineStartLevel)
				{
					updateFlag |= kUpdateFlagGaussianBlurMesh | kUpdateFlagDescriptors | kUpdateFlagCombineComposition | kUpdateFlagRebuild;
					cacheCombineStartLevel = combineStartLevel;
				}
				if( cacheScreenWidth != pipeline.ScreenWidth
				||	cacheScreenHeight != pipeline.ScreenHeight)
				{
					updateFlag |= kUpdateFlagGaussianBlurMesh | kUpdateFlagDescriptors | kUpdateFlagCombineComposition | kUpdateFlagRebuild;
					cacheScreenWidth = pipeline.ScreenWidth;
					cacheScreenHeight = pipeline.ScreenHeight;
				}
				if( (updateFlag & kUpdateFlagBlendWeight) != 0)
				{
					resources.UpdateBlendWeight( material, blendWeight, GaussianBlurResources.BlendMode.Add);
				}
				if( (updateFlag & kUpdateFlagThresholds) != 0)
				{
					if( SystemInfo.SupportsRenderTextureFormat( RenderTextureFormat.ARGBHalf) != false
					||	SystemInfo.SupportsRenderTextureFormat( RenderTextureFormat.ARGBFloat) != false
					||	SystemInfo.SupportsRenderTextureFormat( RenderTextureFormat.RGB111110Float) != false)
					{
						if( material.IsKeywordEnabled( kShaderKeywordLDR) != false)
						{
							material.DisableKeyword( kShaderKeywordLDR);
						}
						material.SetFloat( kShaderPropertyThresholds, thresholds);
					}
					else
					{
						if( material.IsKeywordEnabled( kShaderKeywordLDR) == false)
						{
							material.EnableKeyword( kShaderKeywordLDR);
						}
						var colorTransform = new Vector4();
						
						if( thresholds >= 1.0f)
						{
							colorTransform.x = 0.0f;
							colorTransform.y = 0.0f;
						}
						else
						{
							colorTransform.x = 1.0f / (1.0f - thresholds);
							colorTransform.y = -thresholds / (1.0f - thresholds);
						}
						material.SetVector( kShaderPropertyColorTransform, colorTransform);
					}
				}
				if( (updateFlag & kUpdateFlagSigmaInPixel) != 0)
				{
					resources.UpdateSigmaInPixel( material, sigmaInPixel);
				}
				if( (updateFlag & kUpdateFlagDescriptors) != 0)
				{
					resources.UpdateDescriptors( material, pipeline.ScreenWidth, pipeline.ScreenHeight, 
						format, downSampleLevel, downSampleCount, combineStartLevel);
				}
				if( (updateFlag & kUpdateFlagGaussianBlurMesh) != 0)
				{
					resources.UpdateGaussianBlurHorizontalMesh();
					resources.UpdateGaussianBlurVerticalMesh();
				}
				if( (updateFlag & kUpdateFlagCombineComposition) != 0)
				{
					resources.UpdateCombineComposition( material, intensity, intensityMultiplier);
				}
			}
			return (updateFlag & kUpdateFlagRebuild) != 0;
		}
		const int kUpdateFlagNone = 0;
		const int kUpdateFlagRebuild = 1 << 0;
		const int kUpdateFlagBlendWeight = 1 << 1;
		const int kUpdateFlagThresholds = 1 << 2;
		const int kUpdateFlagSigmaInPixel = 1 << 3;
		const int kUpdateFlagCombineComposition = 1 << 4;
		const int kUpdateFlagDescriptors = 1 << 5;
		const int kUpdateFlagGaussianBlurMesh = 1 << 6;
		
		const string kShaderKeywordLDR = "LDR";
		static readonly int kShaderPropertyThresholds = Shader.PropertyToID( "_Thresholds");
		static readonly int kShaderPropertyColorTransform = Shader.PropertyToID( "_ColorTransform");
		
		[SerializeField]
		PostProcessEvent phase = PostProcessEvent.PostTransparent;
		[SerializeField]
		RenderTextureFormat format = RenderTextureFormat.DefaultHDR;
		[SerializeField, Range( 0, 1)]
		float blendWeight = 1.0f;
		[SerializeField]
		float thresholds = 1.0f;
		[SerializeField]
		float sigmaInPixel = 3.0f;
		[SerializeField]
		float intensity = 5.0f;
		[SerializeField] 
		float intensityMultiplier = 1.5f;
		[SerializeField, Range( 0, 4)] 
		int downSampleLevel = 2;
		[SerializeField, Range( 1, 7)] 
		int downSampleCount = 7;
		[SerializeField, Range( 0, 6)]
		int combineStartLevel = 0;
		
		[System.NonSerialized]
		float? cacheBlendWeight;
		[System.NonSerialized]
		float? cacheThresholds;
		[System.NonSerialized]
		float? cacheSigmaInPixel;
		[System.NonSerialized]
		float? cacheIntensity;
		[System.NonSerialized]
		float? cacheIntensityMultiplier;
		[System.NonSerialized]
		int? cacheDownSampleLevel;
		[System.NonSerialized]
		int? cacheDownSampleCount;
		[System.NonSerialized]
		int? cacheCombineStartLevel;
		[System.NonSerialized]
		int? cacheScreenWidth;
		[System.NonSerialized]
		int? cacheScreenHeight;
	}
}
