﻿
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace RenderingPipeline
{
	[CreateAssetMenu( menuName="RenderPipeline/CameraMotionBlur", fileName="PostProcessCameraMotionBlur", order=1200)]
	public sealed class CameraMotionBlurSettings : Settings<CameraMotionBlurProperties>
	{
	}
	[System.Serializable]
	public sealed class CameraMotionBlurProperties : IGenericProperties
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
		public float Distance
		{
			get{ return distance; }
			set{ distance = value; }
		}
		public int DownSample
		{
			get{ return downSample; }
			set{ downSample = value; }
		}
		public CameraMotionBlur.Quality SampleQuality
		{
			get{ return sampleQuality; }
			set{ sampleQuality = value; }
		}
		public void ClearCache()
		{
			cacheEnabled = null;
			cacheDistance = null;
			cacheDownSample = null;
			cacheSampleQuality = null;
			cacheViewProjection = null;
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
				viewProjection = pipeline.CacheCamera.projectionMatrix * pipeline.CacheCamera.worldToCameraMatrix;
				if( cacheViewProjection.HasValue == false)
				{
					material.SetMatrix( kShaderPropertyCurrentToPreviousViewProjection, Matrix4x4.identity);
					cacheViewProjection = viewProjection;
				}
				else
				{
					Matrix4x4 currentToPreviousViewProjection = cacheViewProjection.Value * viewProjection.inverse;
					material.SetMatrix( kShaderPropertyCurrentToPreviousViewProjection, currentToPreviousViewProjection);
					cacheViewProjection = viewProjection;
				}
				if( cacheDownSample != downSample
				||	cacheScreenWidth != pipeline.ScreenWidth
				||	cacheScreenHeight != pipeline.ScreenHeight)
				{
					callback?.Invoke( pipeline.ScreenWidth / downSample, pipeline.ScreenHeight / downSample);
					cacheDownSample = downSample;
					cacheScreenWidth = pipeline.ScreenWidth;
					cacheScreenHeight = pipeline.ScreenHeight;
					rebuild = true;
				}
				if( cacheDistance != distance)
				{
					material.SetFloat( kShaderPropertyDistance, 1.0f - distance);
					cacheDistance = distance;
				}
				if( cacheSampleQuality != sampleQuality)
				{
					string keyword;
					
					if( material.IsKeywordEnabled( kShaderKeywordLowQuality) != false)
					{
						material.DisableKeyword( kShaderKeywordLowQuality);
					}
					if( material.IsKeywordEnabled( kShaderKeywordMiddleQuality) != false)
					{
						material.DisableKeyword( kShaderKeywordMiddleQuality);
					}
					if( material.IsKeywordEnabled( kShaderKeywordHighQuality) != false)
					{
						material.DisableKeyword( kShaderKeywordHighQuality);
					}
					if( TryGetQualityKeyword( sampleQuality, out keyword) != false)
					{
						material.EnableKeyword( keyword);
					}
					cacheSampleQuality = sampleQuality;
				}
			}
			return rebuild;
		}
		bool TryGetQualityKeyword( CameraMotionBlur.Quality quality, out string keyword)
		{
			switch( sampleQuality)
			{
				case CameraMotionBlur.Quality.kLow:
				{
					keyword = kShaderKeywordLowQuality;
					return true;
				}
				case CameraMotionBlur.Quality.kMiddle:
				{
					keyword = kShaderKeywordMiddleQuality;
					return true;
				}
				case CameraMotionBlur.Quality.kHigh:
				{
					keyword = kShaderKeywordHighQuality;
					return true;
				}
			}
			keyword = string.Empty;
			return false;
		}
		
		const string kShaderKeywordLowQuality = "QUALITY_LOW";
		const string kShaderKeywordMiddleQuality = "QUALITY_MIDDLE";
		const string kShaderKeywordHighQuality = "QUALITY_HIGH";
		static readonly int kShaderPropertyDistance = Shader.PropertyToID( "_Distance");
		static readonly int kShaderPropertyCurrentToPreviousViewProjection = Shader.PropertyToID( "_CurrentToPreviousViewProjectionMatrix");
		
		[SerializeField]
		bool enabled = true;
		[SerializeField]
		PostProcessEvent phase = PostProcessEvent.BeforeImageEffects;
		[SerializeField, Range( 0, 1)]
		float distance = 0.0f;
		[SerializeField, Range( 1, 5)]
		int downSample = 2;
		[SerializeField]
		CameraMotionBlur.Quality sampleQuality = default;
		[System.NonSerialized]
		Matrix4x4 viewProjection;
		
		[System.NonSerialized]
		bool? cacheEnabled;
		[System.NonSerialized]
		float? cacheDistance;
		[System.NonSerialized]
		int? cacheDownSample;
		[System.NonSerialized]
		CameraMotionBlur.Quality? cacheSampleQuality;
		[System.NonSerialized]
		Matrix4x4? cacheViewProjection;
		[System.NonSerialized]
		int? cacheScreenWidth;
		[System.NonSerialized]
		int? cacheScreenHeight;
	}
}
