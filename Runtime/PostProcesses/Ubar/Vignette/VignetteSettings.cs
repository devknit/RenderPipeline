
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	[CreateAssetMenu( menuName="RenderPipeline/Vignette", fileName="PostProcessVignette", order=1200)]
	public sealed class VignetteSettings : Settings<VignetteProperties>
	{
	}
	[System.Serializable]
	public sealed class VignetteProperties : IUbarProperties
	{
		public bool Enabled
		{
			get{ return enabled; }
			set{ enabled = value; }
		}
		public PostProcessEvent Phase
		{
			get{ return PostProcessEvent.BeforeImageEffects; }
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
		public float Intensity
		{
			get{ return intensity; }
			set{ intensity = value; }
		}
		public float Smoothness
		{
			get{ return smoothness; }
			set{ smoothness = value; }
		}
		public float Roundness
		{
			get{ return roundness; }
			set{ roundness = value; }
		}
		public bool Rounded
		{
			get{ return rounded; }
			set{ rounded = value; }
		}
		public void ClearCache()
		{
			cacheEnabled = null;
			cacheColor = null;
			cacheCenter = null;
			cacheIntensity = null;
			cacheSmoothness = null;
			cacheRoundness = null;
			cacheRounded = null;
		}
		public bool UpdateProperties( Material material, bool forcedDisable)
		{
			bool rebuild = false;
			
			if( cacheEnabled != enabled)
			{
				rebuild = true;
				cacheEnabled = enabled;
			}
			if( enabled != false && forcedDisable == false)
			{
				if( material.IsKeywordEnabled( kShaderKeywordVignette) == false)
				{
					material.EnableKeyword( kShaderKeywordVignette);
				}
				if( cacheColor != color)
				{
					material.SetColor( kShaderPropertyColor, color);
					cacheColor = color;
				}
				if( cacheCenter != center)
				{
					material.SetVector( kShaderPropertyCenter, new Vector4( center.x, center.y, 0, 0));
					cacheCenter = center;
				}
				if( cacheIntensity != intensity
				||	cacheSmoothness != smoothness
				||	cacheRoundness != roundness
				||	cacheRounded != rounded)
				{
					intensity = Mathf.Clamp( intensity, 0.0f, 1.0f);
					smoothness = Mathf.Clamp( smoothness, 0.0f, 1.0f);
					roundness = Mathf.Clamp( roundness, 0.0f, 1.0f);
					material.SetVector( kShaderPropertyParam, 
						new Vector4( 
							intensity * 3.0f, 
							Mathf.Max( 1e-4f, smoothness * 5.0f), 
							Mathf.Lerp( 6.0f, 1.0f, roundness), 
							(rounded == false)? 0.0f : 1.0f));
					cacheIntensity = intensity;
					cacheSmoothness = smoothness;
					cacheRoundness = roundness;
					cacheRounded = rounded;
				}
			}
			else if( material.IsKeywordEnabled( kShaderKeywordVignette) != false)
			{
				material.DisableKeyword( kShaderKeywordVignette);
			}
			return rebuild;
		}
		
		const string kShaderKeywordVignette = "_VIGNETTE";
		static readonly int kShaderPropertyColor = Shader.PropertyToID( "_VignetteColor");
		static readonly int kShaderPropertyCenter = Shader.PropertyToID( "_VignetteCenter");
		static readonly int kShaderPropertyParam = Shader.PropertyToID( "_VignetteParam");
		
		[SerializeField]
		bool enabled = true;
		[SerializeField]
		Color color = Color.black;
		[SerializeField]
		Vector2 center = new Vector2( 0.5f, 0.5f);
		[SerializeField, Range( 0, 1)]
		float intensity = 0.2f; 
		[SerializeField, Range( 0, 1)]
		float smoothness = 1.0f;
		[SerializeField, Range( 0, 1)]
		float roundness = 1.0f;
		[SerializeField]
		bool rounded = false;
		
		[System.NonSerialized]
		bool? cacheEnabled;
		[System.NonSerialized]
		Color? cacheColor;
		[System.NonSerialized]
		Vector2? cacheCenter;
		[System.NonSerialized]
		float? cacheIntensity;
		[System.NonSerialized]
		float? cacheSmoothness;
		[System.NonSerialized]
		float? cacheRoundness;
		[System.NonSerialized]
		bool? cacheRounded;
	}
}
