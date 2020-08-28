
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace RenderPipeline
{
	[CreateAssetMenu( menuName="RenderPipeline/Fxaa3", fileName="PostProcessFxaa3", order=1200)]
	public sealed class Fxaa3Settings : Settings
	{
		[SerializeField]
		public Fxaa3Properties properties = default;
	}
	[System.Serializable]
	public sealed class Fxaa3Properties : Properties
	{
		public bool Enabled
		{
			get{ return enabled; }
			set{ enabled = value; }
		}
		public float EdgeThresholdMin
		{
			get{ return edgeThresholdMin; }
			set{ edgeThresholdMin = value; }
		}
		public float EdgeThreshold
		{
			get{ return edgeThreshold; }
			set{ edgeThreshold = value; }
		}
		public float EdgeSharpness
		{
			get{ return edgeSharpness; }
			set{ edgeSharpness = value; }
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
				if( cacheEdgeThresholdMin != edgeThresholdMin)
				{
					material.SetFloat( kShaderPropertyEdgeThresholdMin, edgeThresholdMin);
					cacheEdgeThresholdMin = edgeThresholdMin;
				}
				if( cacheEdgeThreshold != edgeThreshold)
				{
					material.SetFloat( kShaderPropertyEdgeThreshold, edgeThreshold);
					cacheEdgeThreshold = edgeThreshold;
				}
				if( cacheEdgeSharpness != edgeSharpness)
				{
					material.SetFloat( kShaderPropertyEdgeSharpness, edgeSharpness);
					cacheEdgeSharpness = edgeSharpness;
				}
			}
			return rebuild;
		}
		
		static readonly int kShaderPropertyEdgeThresholdMin = Shader.PropertyToID( "_EdgeThresholdMin");
		static readonly int kShaderPropertyEdgeThreshold = Shader.PropertyToID( "_EdgeThreshold");
		static readonly int kShaderPropertyEdgeSharpness = Shader.PropertyToID( "_EdgeSharpness");
		
		[SerializeField]
		bool enabled = true;
		[SerializeField]
		float edgeThresholdMin = 0.05f;
		[SerializeField]
        float edgeThreshold = 0.1f;//0.2f;
        [SerializeField]
        float edgeSharpness = 4.0f;
		
		bool? cacheEnabled;
		float? cacheEdgeThresholdMin = 0.05f;
        float? cacheEdgeThreshold = 0.2f;
        float? cacheEdgeSharpness = 4.0f;
	}
}
