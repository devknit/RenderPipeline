
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	[CreateAssetMenu( menuName="RenderPipeline/Noise", fileName="PostProcessNoise", order=1200)]
	public sealed class NoiseSettings : Settings<NoiseProperties>
	{
	}
	[System.Serializable]
	public sealed class NoiseProperties : IUbarProperties
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
		public Color Color
		{
			get{ return color; }
			set{ color = value; }
		}
		public float Speed
		{
			get{ return speed; }
			set{ speed = value; }
		}
		public float Interval
		{
			get{ return interval; }
			set{ interval = value; }
		}
		public float Edge0
		{
			get{ return edge0; }
			set{ edge0 = value; }
		}
		public float Edge1
		{
			get{ return edge1; }
			set{ edge1 = value; }
		}
		internal byte StencilReference
		{
			get{ return stencilReference; }
			set{ stencilReference = value; }
		}
		internal byte StencilReadMask
		{
			get{ return stencilReadMask; }
			set{ stencilReadMask = value; }
		}
		internal CompareFunction StencilCompare
		{
			get{ return stencilCompare; }
			set{ stencilCompare = value; }
		}
		public void ClearCache()
		{
			cacheEnabled = null;
			cacheColor = null;
			cacheSpeed = null;
			cacheInterval = null;
			cacheEdge0 = null;
			cacheEdge1 = null;
			cacheStencilReference = null;
			cacheStencilReadMask = null;
			cacheStencilCompare = null;
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
				if( material.IsKeywordEnabled( kShaderKeywordNoise) == false)
				{
					material.EnableKeyword( kShaderKeywordNoise);
				}
				if( cacheColor != color)
				{
					material.SetColor( kShaderPropertyColor, color);
					cacheColor = color;
				}
				if( cacheSpeed != speed
				||	cacheInterval != interval
				||	cacheEdge0 != edge0
				||	cacheEdge1 != edge1)
				{
					material.SetVector( kShaderPropertyParam, new Vector4( interval, edge0, edge1, speed));
					cacheInterval = interval;
					cacheEdge0 = edge0;
					cacheEdge1 = edge1;
					cacheSpeed = speed;
				}
				if( cacheStencilReference != stencilReference)
				{
					stencilReference = (byte)Mathf.Clamp( stencilReference, 0, 255);
					material.SetInt( ShaderProperty.StencilReference, stencilReference);
					cacheStencilReference = stencilReference;
				}
				if( cacheStencilReadMask != stencilReadMask)
				{
					stencilReadMask = (byte)Mathf.Clamp( stencilReadMask, 0, 255);
					material.SetInt( ShaderProperty.StencilReadMask, stencilReadMask);
					cacheStencilReadMask = stencilReadMask;
				}
				if( cacheStencilCompare != stencilCompare)
				{
					if( stencilCompare == CompareFunction.Disabled)
					{
						stencilCompare = CompareFunction.Always;
					}
					bool always = stencilCompare == CompareFunction.Always;
					bool cacheAlways = false;
					
					if( cacheStencilCompare.HasValue != false)
					{
						cacheAlways = cacheStencilCompare.Value == CompareFunction.Always;
					}
					if( cacheAlways != always)
					{
						rebuild = true;
					}
					material.SetInt( ShaderProperty.StencilCompare, (int)stencilCompare);
					cacheStencilCompare = stencilCompare;
				}
			}
			else if( material.IsKeywordEnabled( kShaderKeywordNoise) != false)
			{
				material.DisableKeyword( kShaderKeywordNoise);
			}
			return rebuild;
		}
		public bool UpdateUbarProperties( Material material, bool forcedDisable)
		{
			return UpdateProperties( material, forcedDisable);
		}
		internal long GetDepthStencilHashCode()
		{
			return DepthStencil.GetHashCode( stencilReference, stencilReadMask, 255, stencilCompare);
		}
		
		const string kShaderKeywordNoise = "_NOISE";
		static readonly int kShaderPropertyColor = Shader.PropertyToID( "_NoiseColor");
		static readonly int kShaderPropertyParam = Shader.PropertyToID( "_NoiseParam");
		
		[SerializeField]
		bool enabled = true;
		[SerializeField]
		PostProcessEvent phase = PostProcessEvent.BeforeImageEffectsOpaque;
		[SerializeField]
		Color color = new Color( 0.5f, 0.7f, 0.8f);
		[SerializeField]
		float speed = 1.0f;
		[SerializeField]
		float interval = 5.0f; 
		[SerializeField, Range( 0, 1)]
		float edge0 = 0.0f;
		[SerializeField, Range( 0, 1)]
		float edge1 = 1.0f;
		[SerializeField, Range(0, 255)]
		byte stencilReference = 0;
		[SerializeField, Range(0, 255)]
		byte stencilReadMask = 255;
		[SerializeField]
		CompareFunction stencilCompare = CompareFunction.Always;
		
		[System.NonSerialized]
		bool? cacheEnabled;
		[System.NonSerialized]
		Color? cacheColor;
		[System.NonSerialized]
		float? cacheSpeed;
		[System.NonSerialized]
		float? cacheInterval;
		[System.NonSerialized]
		float? cacheEdge0;
		[System.NonSerialized]
		float? cacheEdge1;
		[System.NonSerialized]
        byte? cacheStencilReference;
        [System.NonSerialized]
        byte? cacheStencilReadMask;
        [System.NonSerialized]
        CompareFunction? cacheStencilCompare;
	}
}
