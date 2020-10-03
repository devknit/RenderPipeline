
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace RenderPipeline
{
	[CreateAssetMenu( menuName="RenderPipeline/Bloom", fileName="PostProcessBloom", order=1200)]
	public sealed class BloomSettings : Settings
	{
		public static BloomSettings Instance
		{
			get;
			private set;
		}
		void OnEnable()
		{
			if( Instance == null)
			{
				Instance = this;
			}
		}
		void OnDisable()
		{
			if( Instance == this)
			{
				Instance = null;
			}
		}
		
		[SerializeField]
		public BloomProperties properties = default;
	}
	[System.Serializable]
	public sealed class BloomProperties : Properties
	{
		public bool Enabled
		{
			get
			{
				if( enabled != false)
				{
					if( (cacheScreenWidth ?? 0) > 0
					&&	(cacheScreenHeight ?? 0) > 0)
					{
						return true;
					}
				}
				return false;
			}
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
		internal int CheckParameterChange()
		{
			int updateFlags = 0;
			
			if( cacheEnabled != enabled)
			{
				updateFlags |= kRebuild;
				cacheEnabled = enabled;
			}
			if( enabled != false)
			{
				if( cacheScreenWidth != Screen.width
				||	cacheScreenHeight != Screen.height)
				{
					cacheScreenWidth = Screen.width;
					cacheScreenHeight = Screen.height;
					updateFlags |= kChangeScreen;
				}
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
					updateFlags |= kChangeDownSampleLevel;
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
					updateFlags |= kChangeDownSampleCount;
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
		internal const int kChangeDownSampleLevel = 1 << 5;
		internal const int kChangeDownSampleCount = 1 << 6;
		internal const int kChangeCombineStartLevel = 1 << 7;
		internal const int kChangeScreen = 1 << 8;
		
		internal const int kChangeBrightnessExtractionMesh = 1 << 9;
		internal const int kChangeGaussianBlurMesh = 1 << 10;
		internal const int kChangeCombineMesh = 1 << 11;
		internal const int kChangeBloomRects = 1 << 12;
		internal const int kChangeCombinePassCount = 1 << 13;
		internal const int kChangeBloomRectCount = 1 << 14;
		
		internal const int kVerifyDescriptors = 
			kChangeDownSampleLevel |
			kChangeDownSampleCount |
			kChangeScreen;
		internal const int kVerifyCombinePassCount = 
			kChangeBloomRects |
			kChangeCombineStartLevel |
			kChangeDownSampleLevel;
		internal const int kVerifyCombineComposition = 
			kChangeIntensity |
			kChangeIntensityMultiplier |
			kChangeBloomRects |
			kChangeCombinePassCount;
		
		[SerializeField]
		bool enabled = true;
		[SerializeField]
		float thresholds = 1.0f;
		[SerializeField]
		float sigmaInPixel = 3.0f; /**/
		[SerializeField]
		float intensity = 5.0f;
		[SerializeField] 
		float intensityMultiplier = 1.5f;
		[SerializeField, Range( 0, 4)] 
		int downSampleLevel = 2;	 /**/
		[SerializeField, Range( 1, 7)] 
		int downSampleCount = 7;	 /**/
		[SerializeField, Range( 0, 6)]
		int combineStartLevel = 0;
		
		[System.NonSerialized]
		bool? cacheEnabled;
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
