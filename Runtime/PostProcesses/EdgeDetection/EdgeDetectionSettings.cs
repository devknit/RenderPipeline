
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace RenderPipeline
{
	public enum DetectType : int
	{
		kCheap,
		kThin
	}
	[CreateAssetMenu( menuName="RenderPipeline/EdgeDetection", fileName="PostProcessEdgeDetection", order=1200)]
	public sealed class EdgeDetectionSettings : Settings<EdgeDetectionProperties>
	{
	}
	[System.Serializable]
	public sealed class EdgeDetectionProperties : IGenericProperties
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
		public DetectType DetectType
		{
			get{ return detectType; }
			set{ detectType = value; }
		}
		public Color EdgeColor
		{
			get{ return edgeColor; }
			set{ edgeColor = value; }
		}
		public int SampleDistance
		{
			get{ return sampleDistance; }
			set{ sampleDistance = value; }
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
			cacheDetectType = null;
			cacheEdgeColor = null;
			cacheSampleDistance = null;
			cacheStencilReference = null;
			cacheStencilReadMask = null;
			cacheStencilCompare = null;
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
				if( cacheDetectType != detectType)
				{
					cacheDetectType = detectType;
					rebuild = true;
				}
				if( cacheEdgeColor != edgeColor)
				{
					material.SetColor( kShaderPropertyEdgeColor, edgeColor);
					cacheEdgeColor = edgeColor;
				}
				if( cacheSampleDistance != sampleDistance)
				{
					sampleDistance = Mathf.Clamp( sampleDistance, 0, 5);
					material.SetFloat( kShaderPropertySampleDistance, sampleDistance);
					cacheSampleDistance = sampleDistance;
				}
				if( cacheStencilReference != stencilReference)
				{
					stencilReference = (byte)Mathf.Clamp( stencilReference, 0, 255);
					material.SetInt( kShaderPropertyStencilRef, stencilReference);
					cacheStencilReference = stencilReference;
				}
				if( cacheStencilReadMask != stencilReadMask)
				{
					stencilReadMask = (byte)Mathf.Clamp( stencilReadMask, 0, 255);
					material.SetInt( kShaderPropertyStencilReadMask, stencilReadMask);
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
					material.SetInt( kShaderPropertyStencilComp, (int)stencilCompare);
					cacheStencilCompare = stencilCompare;
				}
			}
			return rebuild;
		}
		internal long GetDepthStencilHashCode()
		{
			return DepthStencil.GetHashCode( stencilReference, stencilReadMask, 255, stencilCompare);
		}
		
		static readonly int kShaderPropertyEdgeColor = Shader.PropertyToID( "_EdgeColor");
		static readonly int kShaderPropertySampleDistance = Shader.PropertyToID( "_SampleDistance");
		static readonly int kShaderPropertyStencilRef = Shader.PropertyToID( "_StencilRef");
		static readonly int kShaderPropertyStencilReadMask = Shader.PropertyToID( "_StencilReadMask");
		static readonly int kShaderPropertyStencilComp = Shader.PropertyToID( "_StencilComp");
		
		[SerializeField]
		bool enabled = true;
		[SerializeField]
		PostProcessEvent phase = PostProcessEvent.BeforeImageEffectsOpaque;
		[SerializeField]
		DetectType detectType = DetectType.kThin;
		[SerializeField]
		Color edgeColor = new Color32( 0, 0, 0, 255);
		[SerializeField, Range( 0, 5)]
		int sampleDistance = 2;
		[SerializeField, Range(0, 255)]
		byte stencilReference = 0;
		[SerializeField, Range(0, 255)]
		byte stencilReadMask = 255;
		[SerializeField]
		CompareFunction stencilCompare = CompareFunction.Always;
		
		[System.NonSerialized]
		bool? cacheEnabled;
		[System.NonSerialized]
        DetectType? cacheDetectType;
        [System.NonSerialized]
        Color? cacheEdgeColor;
        [System.NonSerialized]
        int? cacheSampleDistance;
        [System.NonSerialized]
        byte? cacheStencilReference;
        [System.NonSerialized]
        byte? cacheStencilReadMask;
        [System.NonSerialized]
        CompareFunction? cacheStencilCompare;
	}
}
