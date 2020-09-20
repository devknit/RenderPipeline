
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace RenderPipeline
{
	[CreateAssetMenu( menuName="RenderPipeline/FXAA", fileName="PostProcessFXAA", order=1200)]
	public sealed class FXAASettings : Settings
	{
		[SerializeField]
		public FXAAProperties properties = default;
	}
	[System.Serializable]
	public sealed class FXAAProperties : Properties
	{
		public bool Enabled
		{
			get{ return enabled; }
			set{ enabled = value; }
		}
		public float Threshold
		{
			get{ return threshold; }
			set{ threshold = value; }
		}
		public float Sharpness
		{
			get{ return sharpness; }
			set{ sharpness = value; }
		}
		internal override void ClearCache()
		{
			cacheEnabled = null;
			cacheThreshold = null;
			cacheSharpness = null;
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
				if( cacheThreshold != threshold)
				{
					material.SetFloat( kShaderPropertyThreshold, threshold);
					cacheThreshold = threshold;
				}
				if( cacheSharpness != sharpness)
				{
					material.SetFloat( kShaderPropertySharpness, sharpness);
					cacheSharpness = sharpness;
				}
			}
			return rebuild;
		}
		
		static readonly int kShaderPropertyThreshold = Shader.PropertyToID( "_Threshold");
		static readonly int kShaderPropertySharpness = Shader.PropertyToID( "_Sharpness");
		
		[SerializeField]
		bool enabled = true;
		[SerializeField]
        float threshold = 0.1f;//0.2f;
        [SerializeField]
        float sharpness = 4.0f;
		
		[System.NonSerialized]
		bool? cacheEnabled;
		[System.NonSerialized]
        float? cacheThreshold;
        [System.NonSerialized]
        float? cacheSharpness;
	}
}
