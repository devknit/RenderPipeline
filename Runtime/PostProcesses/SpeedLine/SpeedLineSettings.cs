
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using DG.Tweening;

namespace RenderingPipeline
{
	[CreateAssetMenu( menuName="RenderPipeline/SpeedLine", fileName="PostProcessSpeedLine", order=1200)]
	public sealed class SpeedLineSettings : Settings<SpeedLineProperties>
	{
	}
	[System.Serializable]
	public sealed class SpeedLineProperties : IGenericProperties
	{
		public bool Enabled
		{
			get{ return color.a > 0; }
			set
			{
				if( value == false)
				{
					if( color.a > 0)
					{
						color = new Color( color.r, color.g, color.b, 0); 
					}
				}
				else if( color.a <= 0)
				{
					color = new Color( color.r, color.g, color.b, 1);
				}
			}
		}
		public PostProcessEvent Phase
		{
			get{ return phase; }
		}
		public Color Color
		{
			get{ return color; }
			set{ color = value; }
		}
		public Vector2 Center
		{
			get{ return center; }
			set{ center = value; }
		}
		public Vector2 AxisVolume
		{
			get{ return axisVolume; }
			set{ axisVolume = value; }
		}
		public float Tiling
		{
			get{ return tiling; }
			set{ tiling = value; }
		}
		public float RadialScale
		{
			get{ return radialScale; }
			set{ radialScale = value; }
		}
		public float SmoothWidth
		{
			get{ return smoothWidth; }
			set{ smoothWidth = value; }
		}
		public float SmoothBorder
		{
			get{ return smoothBorder; }
			set{ smoothBorder = value; }
		}
		public float AnimationSpeed
		{
			get{ return animationSpeed; }
			set{ animationSpeed = value; }
		}
		public void ClearCache()
		{
			cacheEnabled = null;
			cacheColor = null;
			cacheCenter = null;
			cacheAxisVolume = null;
			cacheTiling = null;
			cacheRadialScale = null;
			cacheToneVolume = null;
			cacheToneBorder = null;
			cacheSmoothWidth = null;
			cacheSmoothBorder = null;
			cacheAnimationSpeed = null;
		}
		public bool UpdateProperties( RenderPipeline pipeline, Material material)
		{
			bool isEnable = Enabled;
			bool rebuild = false;
			
			if( cacheEnabled != isEnable)
			{
				rebuild = true;
				cacheEnabled = isEnable;
			}
			if( isEnable != false)
			{
				if( cacheColor != color)
				{
					material.SetColor( kShaderPropertyColor, color);
					cacheColor = color;
				}
				if( cacheCenter != center)
				{
					material.SetVector( kShaderPropertyCenter, center);
					cacheCenter = center;
				}
				if( cacheAxisVolume != axisVolume)
				{
					material.SetVector( kShaderPropertyAxisVolume, axisVolume);
					cacheAxisVolume = axisVolume;
				}
				if( cacheTiling != tiling)
				{
					material.SetFloat( kShaderPropertyTiling, tiling);
					cacheTiling = tiling;
				}
				if( cacheRadialScale != radialScale)
				{
					radialScale = Mathf.Clamp( radialScale, 0.0f, 10.0f);
					material.SetFloat( kShaderPropertyRadialScale, radialScale);
					cacheRadialScale = radialScale;
				}
				if( cacheToneVolume != toneVolume)
				{
					material.SetFloat( kShaderPropertyToneVolume, toneVolume);
					cacheToneVolume = toneVolume;
				}
				if( cacheToneBorder != toneBorder)
				{
					material.SetFloat( kShaderPropertyToneBorder, toneBorder);
					cacheToneBorder = toneBorder;
				}
				if( cacheSmoothWidth != smoothWidth)
				{
					material.SetFloat( kShaderPropertySmoothWidth, smoothWidth);
					cacheSmoothWidth = smoothWidth;
				}
				if( cacheSmoothBorder != smoothBorder)
				{
					material.SetFloat( kShaderPropertySmoothBorder, smoothBorder);
					cacheSmoothBorder = smoothBorder;
				}
				if( cacheAnimationSpeed != animationSpeed)
				{
					material.SetFloat( kShaderPropertyAnimationSpeed, animationSpeed);
					cacheAnimationSpeed = animationSpeed;
				}
			}
			return rebuild;
		}
		static readonly int kShaderPropertyColor = Shader.PropertyToID( "_Color");
		static readonly int kShaderPropertyCenter = Shader.PropertyToID( "_Center");
		static readonly int kShaderPropertyAxisVolume = Shader.PropertyToID( "_AxisVolume");
		static readonly int kShaderPropertyTiling = Shader.PropertyToID( "_Tiling");
		static readonly int kShaderPropertyRadialScale = Shader.PropertyToID( "_RadialScale");
		static readonly int kShaderPropertyToneVolume = Shader.PropertyToID( "_ToneVolume");
		static readonly int kShaderPropertyToneBorder = Shader.PropertyToID( "_ToneBorder");
		static readonly int kShaderPropertySmoothWidth = Shader.PropertyToID( "_SmoothWidth");
		static readonly int kShaderPropertySmoothBorder = Shader.PropertyToID( "_SmoothBorder");
		static readonly int kShaderPropertyAnimationSpeed = Shader.PropertyToID( "_AnimationSpeed");
		
		[SerializeField]
		PostProcessEvent phase = PostProcessEvent.PostTransparent;
		[SerializeField]
		Color color = Color.black;
		[SerializeField]
		Vector2 center = new Vector2( 0.5f, 0.5f);
		[SerializeField]
		Vector2 axisVolume = Vector2.one;
		[SerializeField]
		float tiling = 200;
		[SerializeField, Range( 0, 10)]
		float radialScale = 0.1f;
		[SerializeField, Range( 0, 1)]
		float toneVolume = 0;
		[SerializeField, Range( 0, 1)]
		float toneBorder = 0.5f;
		[SerializeField]
		float smoothWidth = 0.3f;
		[SerializeField]
		float smoothBorder = 0.3f;
		[SerializeField]
		float animationSpeed = 3;
		
		[System.NonSerialized]
		bool? cacheEnabled;
		[System.NonSerialized]
		Color? cacheColor;
		[System.NonSerialized]
		Vector2? cacheCenter;
		[System.NonSerialized]
		Vector2? cacheAxisVolume;
		[System.NonSerialized]
		float? cacheTiling;
		[System.NonSerialized]
		float? cacheRadialScale;
		[System.NonSerialized]
		float? cacheToneVolume;
		[System.NonSerialized]
		float? cacheToneBorder;
		[System.NonSerialized]
		float? cacheSmoothWidth;
		[System.NonSerialized]
		float? cacheSmoothBorder;
		[System.NonSerialized]
		float? cacheAnimationSpeed;
	}
}
