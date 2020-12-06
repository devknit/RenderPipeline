
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using DG.Tweening;

namespace RenderPipeline
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
		public void ClearCache()
		{
			cacheEnabled = null;
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
				material.SetFloat( "_Samples", samples);
				material.SetFloat( "_Intensity", intensity);
				material.SetVector( "_Center", center);
				material.SetFloat( "_Radius", radius);
			}
			return rebuild;
		}
		
		[SerializeField]
		bool enabled = true;
		[SerializeField]
		PostProcessEvent phase = PostProcessEvent.BeforeImageEffects;
		[SerializeField, Range( 4, 32)]
		int samples = 16;
		[SerializeField, Range( 0, 1)]
		float intensity = 1.0f;
		[SerializeField]
		Vector2 center = new Vector2( 0.5f, 0.5f);
		[SerializeField, Range( 0, 50)]
		float radius = 0.1f;
		
		[System.NonSerialized]
		bool? cacheEnabled;
	}
}
