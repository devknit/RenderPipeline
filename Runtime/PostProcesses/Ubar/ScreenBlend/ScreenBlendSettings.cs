
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using DG.Tweening;

namespace RenderPipeline
{
	[CreateAssetMenu( menuName="RenderPipeline/ScreenBlend", fileName="PostProcessScreenBlend", order=1200)]
	public sealed class ScreenBlendSettings : Settings
	{
		[SerializeField]
		public ScreenBlendProperties properties = default;
	}
	[System.Serializable]
	public sealed class ScreenBlendProperties : Properties
	{
		public bool Enabled
		{
			get{ return enabled; }
			set{ enabled = value; }
		}
		public bool FlipHorizontal
		{
			get{ return flipHorizontal; }
			set{ flipHorizontal = value; }
		}
		public Color Color
		{
			get{ return color; }
			set{ color = value; }
		}
		internal override void ClearCache()
		{
			cacheEnabled = null;
			cacheFlipHorizontal = null;
			cacheColor = null;
		}
		internal bool CheckParameterChange( Material material)
		{
			bool rebuild = false;
			
			if( cacheEnabled != enabled)
			{
				rebuild = true;
				cacheEnabled = enabled;
			}
			if( enabled != false)
			{
				if( cacheFlipHorizontal != flipHorizontal)
				{
					if( flipHorizontal != false)
					{
						if( material.IsKeywordEnabled( kShaderKeywordFlipHorizontal) == false)
						{
							material.EnableKeyword( kShaderKeywordFlipHorizontal);
						}
					}
					else if( material.IsKeywordEnabled( kShaderKeywordFlipHorizontal) != false)
					{
						material.DisableKeyword( kShaderKeywordFlipHorizontal);
					}
					cacheFlipHorizontal = flipHorizontal;
				}
				Color mixedColor = ColorBlending();
				if( cacheColor != mixedColor)
				{
					material.SetColor( kShaderPropertyColor, mixedColor);
					cacheColor = mixedColor;
				}
			}
			return rebuild;
		}
		internal void ApplyBlendColor( Color color, float weight)
		{
			blendColor = color;
			blendColorWeight = weight;
		}
		internal void RestoreBlendColor( float seconds)
		{
			if( seconds > 0.0f)
			{
				restoreTween?.Kill( true);
				Color mixedColor = ColorBlending();
				
				if( mixedColor != color)
				{
					restoreTween = DOTween.To(
						() => mixedColor,
						value => color = value,
						color, seconds);
					color = mixedColor;
				}
			}
			blendColor = Color.clear;
			blendColorWeight = 0.0f;
		}
		Color ColorBlending()
		{
			return blendColor + color * (1.0f - blendColorWeight);
		}
		
		const string kShaderKeywordFlipHorizontal = "FLIPHORIZONTAL";
		static readonly int kShaderPropertyColor = Shader.PropertyToID( "_Color");
		
		[SerializeField]
		bool enabled = true;
		[SerializeField]
		bool flipHorizontal;
		[SerializeField]
		Color color;
		
		[System.NonSerialized]
		bool? cacheEnabled;
		[System.NonSerialized]
		bool? cacheFlipHorizontal;
		[System.NonSerialized]
		Color? cacheColor;
		[System.NonSerialized]
		Color blendColor;
		[System.NonSerialized]
		float blendColorWeight;
		[System.NonSerialized]
		Tween restoreTween;
	}
}
