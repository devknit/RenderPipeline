
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using DG.Tweening;

namespace RenderingPipeline
{
	[CreateAssetMenu( menuName="RenderPipeline/MotionBlur", fileName="PostProcessMotionBlur", order=1200)]
	public sealed class MotionBlurSettings : Settings<MotionBlurProperties>
	{
	}
	[System.Serializable]
	public sealed class MotionBlurProperties : IGenericProperties
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
		public int Samples
		{
			get{ return samples; }
			set{ samples = value; }
		}
		public float ShutterAngle
		{
			get{ return shutterAngle; }
			set{ shutterAngle = value; }
		}
		public int? TileSize
		{
			get{ return cacheTileSize;}
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
			return (cacheScreenWidth ?? 0) > 0 && (cacheScreenHeight ?? 0) > 0
			&& SystemInfo.SupportsRenderTextureFormat( RenderTextureFormat.RGHalf) != false;
		}
		public void ClearCache()
		{
			cacheEnabled = null;
			cacheSamples = null;
			cacheShutterAngle = null;
			cacheMaxBlurPixels = null;
			cacheTileSize = null;
			cacheScreenWidth = null;
			cacheScreenHeight = null;
		}
		public int UpdateProperties( RenderPipeline pipeline, Material material)
		{
			int updateFlags = 0;
			
			if( cacheEnabled != enabled)
			{
				updateFlags |= kEnabled;
				cacheEnabled = enabled;
			}
			if( enabled != false)
			{
				if( cacheScreenWidth != pipeline.ScreenWidth
				||	cacheScreenHeight != pipeline.ScreenHeight)
				{
					cacheScreenWidth = pipeline.ScreenWidth;
					cacheScreenHeight = pipeline.ScreenHeight;
					updateFlags |= kChangeScreen;
				}
				if( (updateFlags & kChangeScreen) != 0)
				{
					int maxBlurPixels = (int)(kMaxBlurRadius * pipeline.ScreenHeight / 100);
					if( cacheMaxBlurPixels != maxBlurPixels)
					{
						int tileSize = ((maxBlurPixels - 1) / 8 + 1) * 8;
						Vector2 tileMaxOffs = Vector2.one * (tileSize / 8.0f - 1.0f) * -0.5f;
						material.SetVector( kShaderPropertyTileMaxOffs, tileMaxOffs);
						material.SetFloat( kShaderPropertyTileMaxLoop, (int)(tileSize / 8.0f));
						material.SetFloat( kShaderPropertyMaxBlurRadius, maxBlurPixels);
						material.SetFloat( kShaderPropertyRcpMaxBlurRadius, 1.0f / maxBlurPixels);
						cacheMaxBlurPixels = maxBlurPixels;
						cacheTileSize = tileSize;
						updateFlags |= kChangeMaxBlurPixels;
					}
				}
				if( cacheShutterAngle != shutterAngle)
				{
					shutterAngle = Mathf.Clamp( shutterAngle, 0, 360);
					material.SetFloat( kShaderPropertyVelocityScale, shutterAngle / 360);
					cacheShutterAngle = shutterAngle;
					updateFlags |= kChangeShutterAngle;
				}
				if( cacheSamples != samples)
				{
					material.SetFloat( kShaderPropertyLoopCount, Mathf.Clamp( samples / 2, 1, 64));
					cacheSamples = samples;
					updateFlags |= kChangeSamples;
				}
			}
			return updateFlags;
		}
		const int kEnabled = 1 << 0;
		const int kChangeScreen = 1 << 1;
		const int kChangeMaxBlurPixels = 1 << 2;
		const int kChangeShutterAngle = 1 << 3;
		const int kChangeSamples = 1 << 4;
		internal const int kRebuild = kEnabled;
		internal const int kDescriptor = kChangeScreen | kChangeMaxBlurPixels;
		
		const float kMaxBlurRadius = 5.0f;
		static readonly int kShaderPropertyMaxBlurRadius = Shader.PropertyToID( "_MaxBlurRadius");
		static readonly int kShaderPropertyRcpMaxBlurRadius = Shader.PropertyToID( "_RcpMaxBlurRadius");
		static readonly int kShaderPropertyVelocityScale = Shader.PropertyToID( "_VelocityScale");
		static readonly int kShaderPropertyTileMaxLoop = Shader.PropertyToID( "_TileMaxLoop");
		static readonly int kShaderPropertyTileMaxOffs = Shader.PropertyToID( "_TileMaxOffs");
		static readonly int kShaderPropertyLoopCount = Shader.PropertyToID( "_LoopCount");
		
		[SerializeField]
		bool enabled = true;
		[SerializeField]
		PostProcessEvent phase = PostProcessEvent.BeforeImageEffects;
		[SerializeField, Range( 4, 32)]
		int samples = 10;
		[SerializeField, Range( 0, 360)]
		float shutterAngle = 270;
		
		[System.NonSerialized]
		bool? cacheEnabled;
		[System.NonSerialized]
		int? cacheSamples;
		[System.NonSerialized]
		float? cacheShutterAngle;
		[System.NonSerialized]
		int? cacheMaxBlurPixels;
		
		[System.NonSerialized]
		int? cacheTileSize;
		[System.NonSerialized]
		int? cacheScreenWidth;
		[System.NonSerialized]
		int? cacheScreenHeight;
	}
}
