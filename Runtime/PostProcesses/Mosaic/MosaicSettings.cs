
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace RenderingPipeline
{
	[CreateAssetMenu( menuName="RenderPipeline/Mosaic", fileName="PostProcessMosaic", order=1200)]
	public sealed class MosaicSettings : Settings<MosaicProperties>
	{
	}
	[System.Serializable]
	public sealed class MosaicProperties : IGenericProperties
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
			cacheTargetLevel = null;
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
				if( cacheWidth != pipeline.ScreenWidth
				||	cacheHeight != pipeline.ScreenHeight
				||	cacheTargetLevel != targetLevel)
				{
					if( stencilCompare != CompareFunction.Always)
					{
						rebuild = true;
					}
					int size = Mathf.Min( pipeline.ScreenWidth, pipeline.ScreenHeight);
					int currentLevel = 12;
					
					for( int i0 = 4096; i0 > size; i0 /= 2)
					{
						--currentLevel;
					}
					int level = Mathf.Clamp( currentLevel - targetLevel, 1, 10);
					float blockSize = Mathf.Pow( 2, level);
					float texelWidth = 1.0f / (float)pipeline.ScreenWidth * blockSize;
					float texelHeight = 1.0f / (float)pipeline.ScreenHeight * blockSize;
					material.SetVector( kShaderPropertyPixelation, new Vector4(
						1.0f / texelWidth, 1.0f / texelHeight, texelWidth, texelHeight));
					material.SetFloat( kShaderPropertyMipmapLevel, level);
					cacheWidth = pipeline.ScreenWidth;
					cacheHeight = pipeline.ScreenHeight;
					cacheTargetLevel = targetLevel;
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
		static readonly int kShaderPropertyMipmapLevel = Shader.PropertyToID( "_MipmapLevel");
		static readonly int kShaderPropertyStencilRef = Shader.PropertyToID( "_StencilRef");
		static readonly int kShaderPropertyStencilReadMask = Shader.PropertyToID( "_StencilReadMask");
		static readonly int kShaderPropertyStencilComp = Shader.PropertyToID( "_StencilComp");
		
		[SerializeField]
		bool enabled = true;
		[SerializeField]
		PostProcessEvent phase = PostProcessEvent.PostTransparent;
		[SerializeField]
		int targetLevel = 5;
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
		int? cacheTargetLevel;
		[System.NonSerialized]
		int? cacheStencilReference;
		[System.NonSerialized]
		int? cacheStencilReadMask;
		[System.NonSerialized]
		CompareFunction? cacheStencilCompare;
	}
}
