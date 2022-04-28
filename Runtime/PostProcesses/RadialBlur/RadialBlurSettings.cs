
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using DG.Tweening;

namespace RenderingPipeline
{
	[CreateAssetMenu( menuName="RenderPipeline/RadialBlur", fileName="PostProcessRadialBlur", order=1200)]
	public sealed class RadialBlurSettings : Settings<RadialBlurProperties>
	{
	}
	[System.Serializable]
	public sealed class RadialBlurProperties : IGenericProperties
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
		public float Intensity
		{
			get{ return intensity; }
			set{ intensity = value; }
		}
		public float Radius
		{
			get{ return radius; }
			set{ radius = value; }
		}
		public Vector2 Center
		{
			get{ return center; }
			set{ center = value; }
		}
		public Vector2 Volume
		{
			get{ return volume; }
			set{ volume = value; }
		}
		public void ClearCache()
		{
			cacheEnabled = null;
			cacheSamples = null;
			cacheIntensity = null;
			cacheRadius = null;
			cacheCenter = null;
			cacheVolume = null;
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
				if( cacheSamples != samples)
				{
					samples = Mathf.Clamp( samples, 4, 32);
					material.SetFloat( kShaderPropertySamples, samples);
					cacheSamples = samples;
				}
				if( cacheIntensity != intensity)
				{
					intensity = Mathf.Clamp( intensity, 0.0f, 1.0f);
					material.SetFloat( kShaderPropertyIntensity, intensity);
					cacheIntensity = intensity;
				}
				if( cacheRadius != radius)
				{
					radius = Mathf.Clamp( radius, 0.0f, 50.0f);
					material.SetFloat( kShaderPropertyRadius, radius);
					cacheRadius = radius;
				}
				if( cacheCenter != center)
				{
					material.SetVector( kShaderPropertyCenter, center);
					cacheCenter = center;
				}
				if( cacheVolume != volume)
				{
					material.SetVector( kShaderPropertyVolume, volume);
					cacheVolume = volume;
				}
			}
			return rebuild;
		}
		static readonly int kShaderPropertySamples = Shader.PropertyToID( "_Samples");
		static readonly int kShaderPropertyIntensity = Shader.PropertyToID( "_Intensity");
		static readonly int kShaderPropertyRadius = Shader.PropertyToID( "_Radius");
		static readonly int kShaderPropertyCenter = Shader.PropertyToID( "_Center");
		static readonly int kShaderPropertyVolume = Shader.PropertyToID( "_Volume");
		
		[SerializeField]
		bool enabled = true;
		[SerializeField]
		PostProcessEvent phase = PostProcessEvent.PostTransparent;
		[SerializeField, Range( 4, 32)]
		int samples = 16;
		[SerializeField, Range( 0, 1)]
		float intensity = 0.1f;
		[SerializeField, Range( 0, 50)]
		float radius = 0.1f;
		[SerializeField]
		Vector2 center = new Vector2( 0.5f, 0.5f);
		[SerializeField]
		Vector2 volume = Vector2.one;
		
		[System.NonSerialized]
		bool? cacheEnabled;
		[System.NonSerialized]
		int? cacheSamples;
		[System.NonSerialized]
		float? cacheIntensity;
		[System.NonSerialized]
		float? cacheRadius;
		[System.NonSerialized]
		Vector2? cacheCenter;
		[System.NonSerialized]
		Vector2? cacheVolume;
	}
}
