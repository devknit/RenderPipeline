﻿
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace RenderingPipeline
{
	[CreateAssetMenu( menuName="RenderPipeline/SSAO", fileName="PostProcessSSAO", order=1200)]
	public sealed class SSAOSettings : Settings<SSAOProperties>
	{
	}
	[System.Serializable]
	public sealed class SSAOProperties : IGenericProperties
	{
		public bool Enabled
		{
			get{ return enabled; }
			set{ enabled = value; }
		}
		public PostProcessEvent Phase
		{
			get{ return phase; }
		}
		public bool FastMode
		{
			get{ return fastMode; }
			set{ fastMode = value; }
		}
		public bool DebugMode
		{
			get{ return debugMode; }
			set{ debugMode = value; }
		}
		public float Intensity
		{
			get{ return intensity; }
			set{ intensity = value; }
		}
		public float BlurAmount
		{
			get{ return blurAmount; }
			set{ blurAmount = value; }
		}
		public float Radius
		{
			get{ return radius; }
			set{ radius = value; }
		}
		public float Area
		{
			get{ return area; }
			set{ area = value; }
		}
		public float IgnoreDistance
		{
			get{ return ignoreDistance; }
			set{ ignoreDistance = value; }
		}
		public void ClearCache()
		{
			cacheEnabled = null;
			cacheFastMode = null;
			cacheIntensity = null;
			cacheBlurAmount = null;
			cacheRadius = null;
			cacheArea = null;
			cacheIgnoreDistance = null;
		}
		public bool UpdateProperties( RenderPipeline pipeline, Material material, System.Action<int, int> callback)
		{
			bool rebuild = false;
			
			if( cacheEnabled != enabled)
			{
				rebuild = true;
				cacheEnabled = enabled;
			}
			if( enabled != false)
			{
				if( cacheScreenWidth != pipeline.ScreenWidth
				||	cacheScreenHeight != pipeline.ScreenHeight)
				{
					callback?.Invoke( pipeline.ScreenWidth, pipeline.ScreenHeight);
					cacheScreenWidth = pipeline.ScreenWidth;
					cacheScreenHeight = pipeline.ScreenHeight;
					rebuild = true;
				}
				if( cacheFastMode != fastMode)
				{
					if( fastMode != false)
					{
						if( material.IsKeywordEnabled( kShaderKeywordFastMode) == false)
						{
							material.EnableKeyword( kShaderKeywordFastMode);
						}
					}
					else if( material.IsKeywordEnabled( kShaderKeywordFastMode) != false)
					{
						material.DisableKeyword( kShaderKeywordFastMode);
					}
				}
				if( cacheDebugMode != debugMode)
				{
					cacheDebugMode = debugMode;
					rebuild = true;
				}
				if( cacheIntensity != intensity)
				{
					material.SetFloat( kShaderPropertyIntensity, intensity);
					cacheIntensity = intensity;
				}
				if( cacheBlurAmount != blurAmount)
				{
					material.SetFloat( kShaderPropertyBlurAmount, blurAmount);
					cacheBlurAmount = blurAmount;
				}
				if( cacheRadius != radius)
				{
					material.SetFloat( kShaderPropertyRadius, radius);
					cacheRadius = radius;
				}
				if( cacheArea != area)
				{
					material.SetFloat( kShaderPropertyArea, area);
					cacheArea = area;
				}
				if( cacheIgnoreDistance != ignoreDistance)
				{
					material.SetFloat( kShaderPropertyDistance, ignoreDistance);
					cacheIgnoreDistance = ignoreDistance;
				}
			}
			return rebuild;
		}
		
		const string kShaderKeywordFastMode = "FASTMODE";
		static readonly int kShaderPropertyIntensity = Shader.PropertyToID( "_Intensity");
		static readonly int kShaderPropertyBlurAmount = Shader.PropertyToID( "_BlurAmount");
		static readonly int kShaderPropertyRadius = Shader.PropertyToID( "_Radius");
		static readonly int kShaderPropertyArea = Shader.PropertyToID( "_Area");
		static readonly int kShaderPropertyDistance = Shader.PropertyToID( "_IgnoreDistance");
		
		[SerializeField]
		bool enabled = true;
		[SerializeField]
		PostProcessEvent phase = PostProcessEvent.PostOpaque;
		[SerializeField]
		bool fastMode = true;
		[SerializeField]
		bool debugMode = false;
		[SerializeField, Range( 0, 3)]
		float intensity = 1.0f;
		[SerializeField, Range( 0, 3)]
		float blurAmount = 1.0f;
		[SerializeField, Range( 0, 1)]
		float radius = 1.0f;
		[SerializeField, Range( 0, 4)]
		float area = 1.0f;
		[SerializeField, Range( 0, 5)]
		float ignoreDistance = 0.0f;
		
		[System.NonSerialized]
		bool? cacheEnabled;
		[System.NonSerialized]
		bool? cacheFastMode;
		[System.NonSerialized]
		bool? cacheDebugMode;
		[System.NonSerialized]
		float? cacheIntensity;
		[System.NonSerialized]
		float? cacheBlurAmount;
		[System.NonSerialized]
		float? cacheRadius;
		[System.NonSerialized]
		float? cacheArea;
		[System.NonSerialized]
		float? cacheIgnoreDistance;
		[System.NonSerialized]
		int? cacheScreenWidth;
		[System.NonSerialized]
		int? cacheScreenHeight;
	}
}
