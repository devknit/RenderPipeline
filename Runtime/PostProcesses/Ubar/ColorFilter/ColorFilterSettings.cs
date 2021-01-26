
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderingPipeline
{
	[CreateAssetMenu( menuName="RenderPipeline/ColorFilter", fileName="PostProcessColorFilter", order=1200)]
	public sealed class ColorFilterSettings : Settings<ColorFilterProperties>
	{
	}
	[System.Serializable]
	public sealed class ColorFilterProperties : IUbarProperties
	{
		public static readonly Color kMonochromeDot = new Color( 0.298912f, 0.586611f, 0.114478f, 0.0f);
		public static readonly Color kSepiaMultiply = new Color( 1.07f, 0.74f, 0.43f, 0.0f);
		
		public bool Enabled
		{
			get{ return enabled; }
			set{ enabled = value; }
		}
		public PostProcessEvent Phase
		{
			get{ return PostProcessEvent.BeforeImageEffects; }
		}
		public Color Dot
		{
			get{ return dot; }
			set{ dot = value; }
		}
		public Color Multiply
		{
			get{ return multiply; }
			set{ multiply = value; }
		}
		public Color Add
		{
			get{ return add; }
			set{ add = value; }
		}
		public void ClearCache()
		{
			cacheEnabled = null;
			cacheDot = null;
			cacheAdd = null;
			cacheMultiply = null;
		}
		public bool UpdateProperties( RenderPipeline pipeline, Material material)
		{
			return false;
		}
		public bool UpdateUbarProperties( RenderPipeline pipeline, Material material, bool forcedDisable)
		{
			bool rebuild = false;
			
			if( cacheEnabled != enabled)
			{
				rebuild = true;
				cacheEnabled = enabled;
			}
			if( enabled != false && forcedDisable == false)
			{
				if( cacheDot != dot)
				{
					material.SetColor( kShaderPropertyDot, dot);
					cacheDot = dot;
				}
				if( cacheMultiply != multiply)
				{
					material.SetColor( kShaderPropertyMultiply, multiply);
					cacheMultiply = multiply;
				}
				if( cacheAdd != add)
				{
					material.SetColor( kShaderPropertyAdd, add);
					cacheAdd = add;
				}
				if( material.IsKeywordEnabled( kShaderKeywordColorFilter) == false)
				{
					material.EnableKeyword( kShaderKeywordColorFilter);
				}
			}
			else if( material.IsKeywordEnabled( kShaderKeywordColorFilter) != false)
			{
				material.DisableKeyword( kShaderKeywordColorFilter);
			}
			return rebuild;
		}
		
		const string kShaderKeywordColorFilter = "_COLORFILTER";
		static readonly int kShaderPropertyDot = Shader.PropertyToID( "_ColorFilterDot");
		static readonly int kShaderPropertyMultiply = Shader.PropertyToID( "_ColorFilterMultiply");
		static readonly int kShaderPropertyAdd = Shader.PropertyToID( "_ColorFilterAdd");
		
		[SerializeField]
		bool enabled = true;
		[SerializeField, ColorUsage( true, true)]
		Color dot = kMonochromeDot;
		[SerializeField, ColorUsage( true, true)]
		Color multiply = kSepiaMultiply;
		[SerializeField, ColorUsage( true, true)]
		Color add = Color.clear;
		
		[System.NonSerialized]
		bool? cacheEnabled;
		[System.NonSerialized]
		Color? cacheDot;
		[System.NonSerialized]
		Color? cacheMultiply;
		[System.NonSerialized]
		Color? cacheAdd;
	}
}
