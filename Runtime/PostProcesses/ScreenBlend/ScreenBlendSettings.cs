
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

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
				if( cacheColor != color)
				{
					material.SetColor( kShaderPropertyColor, color);
					cacheColor = color;
				}
			}
			return rebuild;
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
	}
}
