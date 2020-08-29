
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace RenderPipeline
{
	[CreateAssetMenu( menuName="RenderPipeline/Bloom", fileName="PostProcessBloom", order=1200)]
	public sealed class BloomSettings : Settings
	{
		[SerializeField]
		public BloomProperties properties = default;
	}
	[System.Serializable]
	public sealed class BloomProperties : Properties
	{
		public bool Enabled
		{
			get{ return enabled; }
			set{ enabled = value; }
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
		internal override void ClearCache()
		{
			cacheEnabled = null;
			cacheThresholds = null;
			cacheSigmaInPixel = null;
			cacheIntensity = null;
			cacheIntensityMultiplier = null;
			cacheDownSampleLevel = null;
			cacheDownSampleCount = null;
			cacheCombineStartLevel = null;
		}
		internal int CheckParameterChange()
		{
			int updateFlags = 0;
			
			if( cacheEnabled != enabled)
			{
				cacheEnabled = enabled;
				updateFlags |= kRebuild;
			}
			if( enabled != false)
			{
				if( cacheThresholds != thresholds)
				{
					cacheThresholds = thresholds;
					updateFlags |= kChangeThresholds;
				}
				if( cacheSigmaInPixel != sigmaInPixel)
				{
					cacheSigmaInPixel = sigmaInPixel;
					updateFlags |= kChangeSigmaInPixel;
				}
				if( cacheIntensity != intensity)
				{
					cacheIntensity = intensity;
					updateFlags |= kChangeIntensity;
				}
				if( cacheIntensityMultiplier != intensityMultiplier)
				{
					cacheIntensityMultiplier = intensityMultiplier;
					updateFlags |= kChangeIntensityMultiplier;
				}
				if( cacheDownSampleLevel != downSampleLevel)
				{
					if( downSampleLevel < 0)
					{
						downSampleLevel = 0;
					}
					if( downSampleLevel > 4)
					{
						downSampleLevel = 4;
					}
					cacheDownSampleLevel = downSampleLevel;
					updateFlags |= kChangeDescriptors;
				}
				if( cacheDownSampleCount != downSampleCount)
				{
					if( downSampleCount < 1)
					{
						downSampleCount = 1;
					}
					if( downSampleCount > 7)
					{
						downSampleCount = 7;
					}
					cacheDownSampleCount = downSampleCount;
					updateFlags |= kChangeDescriptors;
				}
				if( cacheCombineStartLevel != combineStartLevel)
				{
					if( combineStartLevel < 0)
					{
						combineStartLevel = 0;
					}
					if( combineStartLevel > 6)
					{
						combineStartLevel = 6;
					}
					cacheCombineStartLevel = combineStartLevel;
					updateFlags |= kChangeCombineStartLevel;
				}
			}
			return updateFlags;
		}
		internal const int kRebuild = 1 << 0;
		internal const int kChangeThresholds = 1 << 1;
		internal const int kChangeSigmaInPixel = 1 << 2;
		internal const int kChangeIntensity = 1 << 3;
		internal const int kChangeIntensityMultiplier = 1 << 4;
		internal const int kChangeCombineStartLevel = 1 << 5;
		internal const int kChangeDescriptors = 1 << 6;
		
		internal const int kChangeCombinePassCount = 
			kChangeThresholds |
			kChangeDescriptors |
			kChangeCombineStartLevel;
		internal const int kChangeCombineComposition = 
			kChangeCombinePassCount |
			kChangeIntensity |
			kChangeIntensityMultiplier;
		internal const int kChangeAll = 0x7fffffff;
		
		[SerializeField]
		bool enabled = true;
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
		
		bool? cacheEnabled;
		float? cacheThresholds;
		float? cacheSigmaInPixel;
		float? cacheIntensity;
		float? cacheIntensityMultiplier;
		int? cacheDownSampleLevel;
		int? cacheDownSampleCount;
		int? cacheCombineStartLevel;
	}
}
