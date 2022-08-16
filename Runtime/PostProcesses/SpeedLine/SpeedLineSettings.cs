
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using DG.Tweening;

namespace RenderingPipeline
{
	public enum SpeedLineType
	{
		Radial,
		Horizontal,
		Vertical,
		// Wave,
	}
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
		public SpeedLineType Type
		{
			get{ return type; }
			set{ type = value; }
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
		public Vector2 AxisMask
		{
			get{ return axisMask; }
			set{ axisMask = value; }
		}
		public float Tiling
		{
			get{ return tiling; }
			set{ tiling = value; }
		}
		public float Sparse
		{
			get{ return sparse; }
			set{ sparse = value; }
		}
		public float Remap
		{
			get{ return remap; }
			set{ remap = value; }
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
			cacheType = null;
			cacheColor = null;
			cacheCenter = null;
			cacheTiling = null;
			cacheSparse = null;
			cacheRemap = null;
			cacheRadialScale = null;
			cacheAxisMask = null;
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
				if( cacheType != type)
				{
					switch( type)
					{
						case SpeedLineType.Horizontal:
						{
							if( material.IsKeywordEnabled( kShaderKeywordPatternHorizontal) == false)
							{
								material.EnableKeyword( kShaderKeywordPatternHorizontal);
							}
							if( material.IsKeywordEnabled( kShaderKeywordPatternVertical) != false)
							{
								material.DisableKeyword( kShaderKeywordPatternVertical);
							}
							break;
						}
						case SpeedLineType.Vertical:
						{
							if( material.IsKeywordEnabled( kShaderKeywordPatternHorizontal) != false)
							{
								material.DisableKeyword( kShaderKeywordPatternHorizontal);
							}
							if( material.IsKeywordEnabled( kShaderKeywordPatternVertical) == false)
							{
								material.EnableKeyword( kShaderKeywordPatternVertical);
							}
							break;
						}
						default:
						{
							if( material.IsKeywordEnabled( kShaderKeywordPatternHorizontal) != false)
							{
								material.DisableKeyword( kShaderKeywordPatternHorizontal);
							}
							if( material.IsKeywordEnabled( kShaderKeywordPatternVertical) != false)
							{
								material.DisableKeyword( kShaderKeywordPatternVertical);
							}
							break;
						}
					}
					cacheType = type;
				}
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
				if( cacheAxisMask != axisMask)
				{
					material.SetVector( kShaderPropertyAxisMask, axisMask);
					cacheAxisMask = axisMask;
				}
				if( cacheTiling != tiling)
				{
					material.SetFloat( kShaderPropertyTiling, tiling);
					cacheTiling = tiling;
				}
				if( cacheSparse != sparse)
				{
					sparse = Mathf.Max( sparse, 0.0f);
					material.SetFloat( kShaderPropertySparse, sparse);
					cacheSparse = sparse;
				}
				if( cacheRemap != remap)
				{
					remap = Mathf.Clamp01( remap);
					material.SetFloat( kShaderPropertyRemap, remap);
					cacheRemap = remap;
				}
				if( cacheRadialScale != radialScale)
				{
					radialScale = Mathf.Clamp( radialScale, 0.0f, 10.0f);
					material.SetFloat( kShaderPropertyRadialScale, radialScale);
					cacheRadialScale = radialScale;
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
		const string kShaderKeywordPatternHorizontal = "_PATTERN_HORIZONTAL";
		const string kShaderKeywordPatternVertical = "_PATTERN_VERTICAL";
		static readonly int kShaderPropertyColor = Shader.PropertyToID( "_Color");
		static readonly int kShaderPropertyCenter = Shader.PropertyToID( "_Center");
		static readonly int kShaderPropertyAxisMask = Shader.PropertyToID( "_AxisMask");
		static readonly int kShaderPropertyTiling = Shader.PropertyToID( "_Tiling");
		static readonly int kShaderPropertySparse = Shader.PropertyToID( "_Sparse");
		static readonly int kShaderPropertyRemap = Shader.PropertyToID( "_Remap");
		static readonly int kShaderPropertyRadialScale = Shader.PropertyToID( "_RadialScale");
		static readonly int kShaderPropertySmoothWidth = Shader.PropertyToID( "_SmoothWidth");
		static readonly int kShaderPropertySmoothBorder = Shader.PropertyToID( "_SmoothBorder");
		static readonly int kShaderPropertyAnimationSpeed = Shader.PropertyToID( "_AnimationSpeed");
		
		[SerializeField]
		PostProcessEvent phase = PostProcessEvent.PostTransparent;
		[SerializeField]
		SpeedLineType type = SpeedLineType.Radial;
		[SerializeField]
		Color color = Color.white;
		[SerializeField]
		Vector2 center = new Vector2( 0.5f, 0.5f);
		[SerializeField]
		Vector2 axisMask = Vector2.one;
		[SerializeField]
		float tiling = 200;
		[SerializeField]
		float sparse = 3.0f;
		[SerializeField, Range( 0, 1)]
		float remap = 0.5f;
		[SerializeField, Range( 0, 10)]
		float radialScale = 0.5f;
		[SerializeField]
		float smoothWidth = 0.45f;
		[SerializeField]
		float smoothBorder = 0.3f;
		[SerializeField]
		float animationSpeed = 3.0f;
		
		[System.NonSerialized]
		SpeedLineType? cacheType;
		[System.NonSerialized]
		Color? cacheColor;
		[System.NonSerialized]
		bool? cacheEnabled;
		[System.NonSerialized]
		Vector2? cacheCenter;
		[System.NonSerialized]
		Vector2? cacheAxisMask;
		[System.NonSerialized]
		float? cacheTiling;
		[System.NonSerialized]
		float? cacheSparse;
		[System.NonSerialized]
		float? cacheRemap;
		[System.NonSerialized]
		float? cacheRadialScale;
		[System.NonSerialized]
		float? cacheSmoothWidth;
		[System.NonSerialized]
		float? cacheSmoothBorder;
		[System.NonSerialized]
		float? cacheAnimationSpeed;
	}
}
