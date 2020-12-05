
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace RenderPipeline
{
	[CreateAssetMenu( menuName="RenderPipeline/Glitch", fileName="PostProcessGlitch", order=1200)]
	public sealed class GlitchSettings : Settings
	{
		[SerializeField]
		public GlitchProperties properties = default;
	}
	[System.Serializable]
	public sealed class GlitchProperties : Properties
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
		public void ClearCache()
		{
			cacheEnabled = null;
			cacheIntensity = null;
			cacheTimeScale = null;
			cacheStepThreshold = null;
			cacheOffsetVolume = null;
			cacheChromaticAberration = null;
		}
		internal bool CheckParameterChange( RenderPipeline pipeline, Material material)
		{
			bool rebuild = false;
			
			if( cacheEnabled != enabled)
			{
				rebuild = true;
				cacheEnabled = enabled;
			}
			if( enabled != false)
			{
				if( cacheIntensity != intensity)
				{
					material.SetFloat( kShaderPropertyIntensity, intensity);
					cacheIntensity = intensity;
				}
				if( cacheTimeScale != timeScale)
				{
					material.SetFloat( kShaderPropertyTimeScale, timeScale);
					cacheTimeScale = timeScale;
				}
				if( cacheStepThreshold != stepThreshold
				||	cacheOffsetVolume != offsetVolume)
				{
					var glitchParam = new Vector4( 
						stepThreshold.x, stepThreshold.y, 
						offsetVolume.x, offsetVolume.y);
					material.SetVector( kShaderPropertyGlitchParam, glitchParam);
					cacheStepThreshold = stepThreshold;
					cacheOffsetVolume = offsetVolume;
				}
				if( cacheChromaticAberration != chromaticAberration)
				{
					material.SetVector( kShaderPropertyChromaticAberration, chromaticAberration);
					cacheChromaticAberration = chromaticAberration;
				}
			}
			return rebuild;
		}
		
		static readonly int kShaderPropertyIntensity = Shader.PropertyToID( "_Intensity");
		static readonly int kShaderPropertyTimeScale = Shader.PropertyToID( "_TimeScale");
		static readonly int kShaderPropertyGlitchParam = Shader.PropertyToID( "_GlitchParam");
		static readonly int kShaderPropertyChromaticAberration = Shader.PropertyToID( "_ChromaticAberration");
		
		[SerializeField]
		bool enabled = true;
		[SerializeField]
		PostProcessEvent phase = PostProcessEvent.BeforeImageEffects;
		
		[SerializeField, Range( 0.0f, 0.9999f)]
		float intensity = 0.025f;
		[SerializeField]
		float timeScale = 1.0f;
		[SerializeField]
		Vector2 stepThreshold = new Vector2( 1, 1000);
		[SerializeField]
		Vector2 offsetVolume = new Vector2( 10, 1);
		[SerializeField]
		Vector3 chromaticAberration = new Vector3( 5, 3, 1);
		
		[System.NonSerialized]
		bool? cacheEnabled;
		[System.NonSerialized]
		float? cacheIntensity;
		[System.NonSerialized]
		float? cacheTimeScale;
		[System.NonSerialized]
		Vector2? cacheStepThreshold;
		[System.NonSerialized]
		Vector2? cacheOffsetVolume;
		[System.NonSerialized]
		Vector3? cacheChromaticAberration;
	}
	
}
