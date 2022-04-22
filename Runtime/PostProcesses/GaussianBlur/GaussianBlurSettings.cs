
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace RenderingPipeline
{
	[CreateAssetMenu( menuName="RenderPipeline/GaussianBlur", fileName="PostProcessGaussianBlur", order=1200)]
	public sealed class GaussianBlurSettings : Settings<GaussianBlurProperties>
	{
	}
	[System.Serializable]
	public sealed class GaussianBlurProperties : IGenericProperties
	{
		public bool Enabled
		{
			get{ return enabled; }
			set{ enabled = value; }
		}
		public PostProcessEvent Phase
		{
			get{ return PostProcessEvent.PostOpaque; }
		}
		public int ScreenWidth
		{
			get{ return cacheScreenWidth ?? 0; }
		}
		public int ScreenHeight
		{
			get{ return cacheScreenHeight ?? 0; }
		}
		public void ClearCache()
		{
			cacheEnabled = null;
			cacheIntensity = null;
		}
		public bool UpdateProperties( RenderPipeline pipeline, Material material, GaussianBlurResources resources)
		{
			int updateFlag = kUpdateFlagNone;
			
			if( cacheEnabled != enabled)
			{
				updateFlag = kUpdateFlagRebuild;
				cacheEnabled = enabled;
			}
			if( enabled != false)
			{
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
				if( (updateFlag & kUpdateFlagSigmaInPixel) != 0)
				{
					resources.UpdateSigmaInPixel( material, sigmaInPixel);
				}
				if( (updateFlag & kUpdateFlagDescriptors) != 0)
				{
					resources.UpdateDescriptors( material, pipeline.ScreenWidth, pipeline.ScreenHeight, 
						RenderTextureFormat.ARGB32, downSampleLevel, downSampleCount, combineStartLevel);
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
		const int kUpdateFlagSigmaInPixel = 1 << 1;
		const int kUpdateFlagCombineComposition = 1 << 2;
		const int kUpdateFlagDescriptors = 1 << 3;
		const int kUpdateFlagGaussianBlurMesh = 1 << 4;
		
		[SerializeField]
		bool enabled = true;
		[SerializeField]
		float sigmaInPixel = 3.0f;
		[SerializeField]
		float intensity = 1.0f;
		[SerializeField] 
		float intensityMultiplier = 2.0f;
		[SerializeField, Range( 0, 4)] 
		int downSampleLevel = 2;
		[SerializeField, Range( 1, 7)] 
		int downSampleCount = 3;
		[SerializeField, Range( 0, 6)]
		int combineStartLevel = 1;
		
		[System.NonSerialized]
		bool? cacheEnabled;
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
