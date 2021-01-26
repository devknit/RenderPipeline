
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace RenderingPipeline
{
	[CreateAssetMenu( menuName="RenderPipeline/Glitch", fileName="PostProcessGlitch", order=1200)]
	public sealed class GlitchSettings : Settings<GlitchProperties>
	{
	}
	[System.Serializable]
	public sealed class GlitchProperties : IGenericProperties
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
		public float Intensity
		{
			get{ return intensity; }
			set{ intensity = value; }
		}
		public float TimeScale
		{
			get{ return timeScale; }
			set{ timeScale = value; }
		}
		public Vector2 Slice
		{
			get{ return slice; }
			set{ slice = value; }
		}
		public float SliceX
		{
			get{ return slice.x; }
			set{ slice = new Vector2( value, slice.y); }
		}
		public float SliceY
		{
			get{ return slice.y; }
			set{ slice = new Vector2( slice.x, value); }
		}
		public Vector2 Volume
		{
			get{ return volume; }
			set{ volume = value; }
		}
		public float VolumeX
		{
			get{ return volume.x; }
			set{ volume = new Vector2( value, volume.y); }
		}
		public float VolumeY
		{
			get{ return volume.y; }
			set{ volume = new Vector2( volume.x, value); }
		}
		public Vector3 ChromaticAberration
		{
			get{ return chromaticAberration; }
			set{ chromaticAberration = value; }
		}
		public void ClearCache()
		{
			cacheEnabled = null;
			cacheIntensity = null;
			cacheTimeScale = null;
			cacheSlice = null;
			cacheVolume = null;
			cacheChromaticAberration = null;
		}
		public bool UpdateProperties( RenderPipeline pipeline, Material material)
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
					intensity = Mathf.Clamp( intensity, 0.0f, 0.9999f);
					material.SetFloat( kShaderPropertyIntensity, intensity);
					cacheIntensity = intensity;
				}
				if( cacheTimeScale != timeScale)
				{
					material.SetFloat( kShaderPropertyTimeScale, timeScale);
					cacheTimeScale = timeScale;
				}
				if( cacheSlice != slice || cacheVolume != volume)
				{
					var glitchParam = new Vector4( 
						slice.x, slice.y, volume.x, volume.y);
					material.SetVector( kShaderPropertyGlitchParam, glitchParam);
					cacheSlice = slice;
					cacheVolume = volume;
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
		Vector2 slice = new Vector2( 1, 1000);
		[SerializeField]
		Vector2 volume = new Vector2( 10, 1);
		[SerializeField]
		Vector3 chromaticAberration = new Vector3( 5, 3, 1);
		
		[System.NonSerialized]
		bool? cacheEnabled;
		[System.NonSerialized]
		float? cacheIntensity;
		[System.NonSerialized]
		float? cacheTimeScale;
		[System.NonSerialized]
		Vector2? cacheSlice;
		[System.NonSerialized]
		Vector2? cacheVolume;
		[System.NonSerialized]
		Vector3? cacheChromaticAberration;
	}
	
}
