
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace RenderPipeline.DepthOfField
{
	internal enum BlurQuality : int
	{
		kLow = 0,
		kMedium = 1,
		kHigh = 2,
	}
	[CreateAssetMenu( menuName="RenderPipeline/DepthOfField", fileName="PostProcessDepthOfField", order=1200)]
	public sealed class DepthOfFieldSettings : Settings<DepthOfFieldProperties>
	{
	}
	[System.Serializable]
	public sealed class DepthOfFieldProperties : IGenericProperties
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
			cacheWidth = null;
			cacheHeight = null;
			cacheFocalSize = null;
			cacheAperture = null;
			cacheMaxBlurSize = null;
			cacheBlurQuality = null;
			cacheHighResolution = null;
			cacheVisualizeFocus = null;
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
				bool updateOffsets = true;
				
				if( cacheWidth != pipeline.ScreenWidth
				||	cacheHeight != pipeline.ScreenHeight
				||	cacheHighResolution != highResolution)
				{
					rebuild = true;
					updateOffsets = true;
					cacheWidth = pipeline.ScreenWidth;
					cacheHeight = pipeline.ScreenHeight;
					cacheHighResolution = highResolution;
				}
				if( cacheBlurQuality != blurQuality
				||	cacheVisualizeFocus != visualizeFocus)
				{
					rebuild = true;
					cacheBlurQuality = blurQuality;
					cacheVisualizeFocus = visualizeFocus;
				}
				if( cacheFocalSize != focalSize)
				{
					focalSize = Mathf.Clamp( focalSize, 0.0f, 2.0f);
					cacheFocalSize = focalSize;
				}
				if( cacheAperture != aperture)
				{
					if( aperture < 0.0f)
					{
						aperture = 0.0f;
					}
					cacheAperture = aperture;
				}
				if( cacheMaxBlurSize != maxBlurSize)
				{
					if( maxBlurSize < 0.1f)
					{
						maxBlurSize = 0.1f;
					}
					updateOffsets = true;
					blurWidth = Mathf.Max( maxBlurSize, 0.0f);
					cacheMaxBlurSize = maxBlurSize;
				}
				Camera cacheCamera = pipeline.CacheCamera;
				
				float focalDistance01 = (focalTransform == null)?
					cacheCamera.WorldToViewportPoint( 
						(focalLength - cacheCamera.nearClipPlane) * cacheCamera.transform.forward + 
						cacheCamera.transform.position).z / (cacheCamera.farClipPlane - cacheCamera.nearClipPlane):
						(cacheCamera.WorldToViewportPoint( focalTransform.position)).z / (cacheCamera.farClipPlane);
				material.SetVector( kShaderPropertyCurveParams,
					new Vector4( 1.0f, focalSize, (1.0f / (1.0f - aperture) - 1.0f), focalDistance01));
				
				if( updateOffsets != false)
				{
					material.SetVector( 
						kShaderPropertyOffsets, (highResolution != false)?
						new Vector4( 0.025f, blurWidth * 2.0f, 0, 0) : 
						new Vector4( 0.1f, blurWidth, (pipeline.ScreenWidth / (pipeline.ScreenWidth >> 1)) * blurWidth, 0));
				}
			}
			return rebuild;
		}
		
		static readonly int kShaderPropertyCurveParams = Shader.PropertyToID( "_CurveParams");
		static readonly int kShaderPropertyOffsets = Shader.PropertyToID( "_Offsets");
		
		[SerializeField]
		bool enabled = true;
		[SerializeField]
		PostProcessEvent phase = PostProcessEvent.BeforeImageEffects;
		[SerializeField]
		Transform focalTransform = null;
		[SerializeField]
		float focalLength = 10.0f;
		[SerializeField, Range( 0, 2)]
		float focalSize = 0.05f;
		[SerializeField, Range( 0, 1)]
		float aperture = 0.5f;
		[SerializeField]
		float maxBlurSize = 2.0f;
		[SerializeField]
		internal BlurQuality blurQuality = BlurQuality.kLow;
		[SerializeField]
		internal bool highResolution = false;
		[SerializeField]
		internal bool visualizeFocus = false;
		[System.NonSerialized]
		float blurWidth = 2.0f;
		
		[System.NonSerialized]
		bool? cacheEnabled;
		[System.NonSerialized]
		int? cacheWidth;
		[System.NonSerialized]
		int? cacheHeight;
		[System.NonSerialized]
		float? cacheFocalSize;
		[System.NonSerialized]
		float? cacheAperture;
		[System.NonSerialized]
		float? cacheMaxBlurSize;
		[System.NonSerialized]
		BlurQuality? cacheBlurQuality;
		[System.NonSerialized]
		bool? cacheHighResolution;
		[System.NonSerialized]bool? cacheVisualizeFocus;
	}
	
}
