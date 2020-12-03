
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace RenderPipeline
{
	[CreateAssetMenu( menuName="RenderPipeline/Mosaic", fileName="PostProcessMosaic", order=1200)]
	public sealed class MosaicSettings : Settings
	{
		[SerializeField]
		public MosaicProperties properties = default;
	}
	[System.Serializable]
	public sealed class MosaicProperties : Properties
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
		public int BlockSize
		{
			get{ return blockSize; }
			set{ blockSize = value; }
		}
		public int StencilReference
		{
			get{ return stencilReference; }
			set{ stencilReference = value; }
		}
		public int StencilReadMask
		{
			get{ return stencilReadMask; }
			set{ stencilReadMask = value; }
		}
		public CompareFunction StencilCompare
		{
			get{ return stencilCompare; }
			set{ stencilCompare = value; }
		}
		public void ClearCache()
		{
			cacheEnabled = null;
			cacheWidth = null;
			cacheHeight = null;
			cacheBlockSize = null;
			cacheStencilReference = null;
			cacheStencilReadMask = null;
			cacheStencilCompare = null;
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
				if( cacheWidth != pipeline.ScreenWidth
				||	cacheHeight != pipeline.ScreenHeight
				||	cacheBlockSize != blockSize)
				{
					if( blockSize < 1)
					{
						blockSize = 1;
					}
					float texelWidth = 1.0f / (float)pipeline.ScreenWidth * blockSize;
					float texelHeight = 1.0f / (float)pipeline.ScreenHeight * blockSize;
					material.SetVector( kShaderPropertyPixelation, new Vector4(
						1.0f / texelWidth, 1.0f / texelHeight, texelWidth, texelHeight));
					cacheWidth = pipeline.ScreenWidth;
					cacheHeight = pipeline.ScreenHeight;
					cacheBlockSize = blockSize;
				}
				if( cacheStencilReference != stencilReference)
				{
					stencilReference = Mathf.Clamp( stencilReference, 0, 255);
					material.SetInt( kShaderPropertyStencilRef, stencilReference);
					cacheStencilReference = stencilReference;
				}
				if( cacheStencilReadMask != stencilReadMask)
				{
					stencilReadMask = Mathf.Clamp( stencilReadMask, 0, 255);
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
		
		static readonly int kShaderPropertyPixelation = Shader.PropertyToID( "_Pixelation");
		static readonly int kShaderPropertyStencilRef = Shader.PropertyToID( "_StencilRef");
		static readonly int kShaderPropertyStencilReadMask = Shader.PropertyToID( "_StencilReadMask");
		static readonly int kShaderPropertyStencilComp = Shader.PropertyToID( "_StencilComp");
		
		[SerializeField]
		bool enabled = true;
		[SerializeField]
		PostProcessEvent phase = PostProcessEvent.BeforeImageEffects;
		[SerializeField]
		int blockSize = 16;
		[SerializeField, Range(0, 255)]
		int stencilReference = 0;
		[SerializeField, Range(0, 255)]
		int stencilReadMask = 255;
		[SerializeField]
		CompareFunction stencilCompare = CompareFunction.Always;
		
		[System.NonSerialized]
		bool? cacheEnabled;
		[System.NonSerialized]
		int? cacheWidth;
		[System.NonSerialized]
		int? cacheHeight;
		[System.NonSerialized]
		int? cacheBlockSize;
		[System.NonSerialized]
		int? cacheStencilReference;
		[System.NonSerialized]
		int? cacheStencilReadMask;
		[System.NonSerialized]
		CompareFunction? cacheStencilCompare;
	}
}
