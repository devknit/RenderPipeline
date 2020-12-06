
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace RenderPipeline
{
	[CreateAssetMenu( menuName="RenderPipeline/ColorGrading", fileName="PostProcessColorGrading", order=1200)]
	public sealed class ColorGradingSettings : Settings<ColorGradingProperties>
	{
		void OnDisable()
		{
			properties.Dispose();
		}
	}
	[System.Serializable]
	public sealed class ColorGradingProperties : IUbarProperties
	{
		public bool Enabled
		{
			get{ return enabled; }
			set{ enabled = value; }
		}
		public PostProcessEvent Phase
		{
			get{ return PostProcessEvent.BeforeImageEffects; }
		}
		public void ClearCache()
		{
			cacheEnabled = null;
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
				Cache( Time.renderedFrameCount);
				
				if( internalLdrLut == null)
				{
					internalLdrLut = new RenderTexture( kLut2DSize * kLut2DSize, kLut2DSize, 0, GetLutFormat(), RenderTextureReadWrite.Linear)
					{
						name = "Color Grading Strip Lut",
						hideFlags = HideFlags.DontSave,
						filterMode = FilterMode.Bilinear,
						wrapMode = TextureWrapMode.Clamp,
						anisoLevel = 0,
						autoGenerateMips = false,
						useMipMap = false
					};
					internalLdrLut.Create();
				}
				material.SetVector( "_Lut2D_Params", 
					new Vector4( kLut2DSize, 0.5f / (kLut2DSize * kLut2DSize), 
						0.5f / kLut2DSize, kLut2DSize / (kLut2DSize - 1.0f)));
				
				var colorBalance = ColorUtilities.ComputeColorBalance( temperature, tint);
				material.SetVector( "_ColorBalance", colorBalance);
				material.SetVector( "_ColorFilter", colorFilter);
				
				material.SetVector( "_HueSatCon", new Vector3( 
					hueShift / 360.0f, 
					saturation / 100.0f + 1.0f, 
					contrast / 100.0f + 1.0f));
				
				var channelMixerR = new Vector3( mixerRedOutRedIn, mixerRedOutGreenIn, mixerRedOutBlueIn);
				var channelMixerG = new Vector3( mixerGreenOutRedIn, mixerGreenOutGreenIn, mixerGreenOutBlueIn);
				var channelMixerB = new Vector3( mixerBlueOutRedIn, mixerBlueOutGreenIn, mixerBlueOutBlueIn);
				material.SetVector( "_ChannelMixerRed", channelMixerR / 100.0f);
				material.SetVector( "_ChannelMixerGreen", channelMixerG / 100.0f);
				material.SetVector( "_ChannelMixerBlue", channelMixerB / 100.0f);
				
				material.SetVector( "_Lift", ColorUtilities.ColorToLift( lift));
				material.SetVector( "_InvGamma", ColorUtilities.ColorToInverseGamma( gamma));
				material.SetVector( "_Gain", ColorUtilities.ColorToGain( gain));
				
				material.SetFloat( "_Brightness", (brightness + 100.0f) / 100.0f);
			}
			return rebuild;
		}
		public bool UpdateUbarProperties( Material material, bool forcedDisable)
		{
			bool rebuild = false;
			
			if( cacheEnabled != enabled)
			{
				rebuild = true;
				cacheEnabled = enabled;
			}
			if( enabled != false && forcedDisable == false)
			{
				if( material.IsKeywordEnabled( "_COLOR_GRADING_LDR_2D") == false)
				{
					material.EnableKeyword( "_COLOR_GRADING_LDR_2D");
				}
				material.SetVector( "_Lut2D_Params", 
					new Vector3( 
						1.0f / internalLdrLut.width, 
						1.0f / internalLdrLut.height, 
						internalLdrLut.height - 1.0f));
				material.SetTexture( "_Lut2D", internalLdrLut);
			}
			else if( material.IsKeywordEnabled( "_COLOR_GRADING_LDR_2D") != false)
			{
				material.DisableKeyword( "_COLOR_GRADING_LDR_2D");
			}
			return rebuild;
		}
		internal void Dispose()
		{
			if( internalLdrLut != null)
			{
				ObjectUtility.Release( internalLdrLut);
				internalLdrLut = null;
			}
		}
		internal RenderTexture GetInternalStripLut()
		{
			return internalLdrLut;
		}
		internal Texture2D GetCurveTexture( bool hdr)
		{
			if( gradingCurves == null)
			{
				gradingCurves = new Texture2D( Spline.kPrecision, 2, GetCurveFormat(), false, true)
				{
					name = "Internal Curves Texture",
					hideFlags = HideFlags.DontSave,
					anisoLevel = 0,
					wrapMode = TextureWrapMode.Clamp,
					filterMode = FilterMode.Bilinear
				};
			}
			Color[] pixels = this.pixels;
			
			for( int i0 = 0; i0 < Spline.kPrecision; ++i0)
			{
				float x = hueVsHueCurve.cachedData[ i0];
				float y = hueVsSatCurve.cachedData[ i0];
				float z = satVsSatCurve.cachedData[ i0];
				float w = lumVsSatCurve.cachedData[ i0];
				pixels[ i0] = new Color( x, y, z, w);
				
				if( hdr == false)
				{
					float m = masterCurve.cachedData[ i0];
					float r = redCurve.cachedData[ i0];
					float g = greenCurve.cachedData[ i0];
					float b = blueCurve.cachedData[ i0];
					pixels[ i0 + Spline.kPrecision] = new Color( r, g, b, m);
				}
			}
			gradingCurves.SetPixels( pixels);
			gradingCurves.Apply( false, false);
			
			return gradingCurves;
		}
		static RenderTextureFormat GetLutFormat()
		{
			var format = RenderTextureFormat.ARGBHalf;
			
			if( IsRenderTextureFormatSupportedForLinearFiltering( format) == false)
			{
				format = RenderTextureFormat.ARGB2101010;
				
				if( IsRenderTextureFormatSupportedForLinearFiltering( format) == false)
				{
					format = RenderTextureFormat.ARGB32;
				}
			}
			return format;
		}
		static TextureFormat GetCurveFormat()
		{
			var format = TextureFormat.RGBAHalf;
			
			if( SystemInfo.SupportsTextureFormat( format) == false)
			{
				format = TextureFormat.ARGB32;
			}
			return format;
		}
		static bool IsRenderTextureFormatSupportedForLinearFiltering(RenderTextureFormat format)
		{
		#if UNITY_2019_1_OR_NEWER
			GraphicsFormat graphicFormat = GraphicsFormatUtility.GetGraphicsFormat( format, RenderTextureReadWrite.Linear);
			return SystemInfo.IsFormatSupported( graphicFormat, FormatUsage.Linear);
		#else
			return format.IsSupported();
		#endif
		}
		void Cache( int frame)
		{
			masterCurve.Cache( frame);
			redCurve.Cache( frame);
			greenCurve.Cache( frame);
			blueCurve.Cache( frame);
			hueVsHueCurve.Cache( frame);
			hueVsSatCurve.Cache( frame);
			satVsSatCurve.Cache( frame);
			lumVsSatCurve.Cache( frame);
		}
		
		const int kLut2DSize = 32;
		const int kLut3DSize = 33;
		
		[SerializeField]
		bool enabled = true;
		
		[SerializeField, Range( -100, 100)]
		float temperature = 0.0f;
		[SerializeField, Range( -100, 100)]
		float tint = 0.0f;
		[SerializeField, ColorUsage( true, true)]
		Color colorFilter = Color.white;
		[SerializeField, Range( -180, 180)]
		float hueShift = 0.0f;
		[SerializeField, Range( -100, 100)]
		float saturation = 0.0f;
		[SerializeField, Range( -100, 100)]
		float brightness = 0.0f;
		[SerializeField, Range( -100, 100)]
		float contrast = 0.0f;
		
		[SerializeField, Range( -200, 200)]
		float mixerRedOutRedIn = 100.0f;
		[SerializeField, Range( -200, 200)]
		float mixerRedOutGreenIn = 0.0f;
		[SerializeField, Range( -200, 200)]
		float mixerRedOutBlueIn = 0.0f;
		[SerializeField, Range( -200, 200)]
		float mixerGreenOutRedIn = 0.0f;
		[SerializeField, Range( -200, 200)]
		float mixerGreenOutGreenIn = 100.0f;
		[SerializeField, Range( -200, 200)]
		float mixerGreenOutBlueIn = 0.0f;
		[SerializeField, Range( -200, 200)]
		float mixerBlueOutRedIn = 0.0f;
		[SerializeField, Range( -200, 200)]
		float mixerBlueOutGreenIn = 0.0f;
		[SerializeField, Range( -200, 200)]
		float mixerBlueOutBlueIn = 100.0f;
		
		[SerializeField]
		Vector4 lift = new Vector4( 1, 1, 1, 0);
		[SerializeField]
		Vector4 gamma = new Vector4( 1, 1, 1, 0);
		[SerializeField]
		Vector4 gain = new Vector4( 1, 1, 1, 0);
		
		[SerializeField]
		Spline masterCurve = new Spline( new AnimationCurve( new Keyframe( 0, 0, 1, 1), new Keyframe( 1, 1, 1, 1)), 0, false, new Vector2( 0, 1));
		[SerializeField]
		Spline redCurve = new Spline( new AnimationCurve( new Keyframe( 0, 0, 1, 1), new Keyframe( 1, 1, 1, 1)), 0, false, new Vector2( 0, 1));
		[SerializeField]
		Spline greenCurve = new Spline( new AnimationCurve( new Keyframe( 0, 0, 1, 1), new Keyframe( 1, 1, 1, 1)), 0, false, new Vector2( 0, 1));
		[SerializeField]
		Spline blueCurve = new Spline( new AnimationCurve( new Keyframe( 0, 0, 1, 1), new Keyframe( 1, 1, 1, 1)), 0, false, new Vector2( 0, 1));
		[SerializeField]
		Spline hueVsHueCurve = new Spline( new AnimationCurve(), 0.5f, true, new Vector2( 0, 1));
		[SerializeField]
		Spline hueVsSatCurve = new Spline( new AnimationCurve(), 0.5f, true, new Vector2( 0, 1));
		[SerializeField]
		Spline satVsSatCurve = new Spline( new AnimationCurve(), 0.5f, false, new Vector2( 0, 1));
		[SerializeField]
		Spline lumVsSatCurve = new Spline( new AnimationCurve(), 0.5f, false, new Vector2( 0, 1));
		
		[System.NonSerialized]
		bool? cacheEnabled;
		
		
		[System.NonSerialized]
		internal Texture2D gradingCurves;
		[System.NonSerialized]
		readonly Color[] pixels = new Color[ Spline.kPrecision * 2];
		[System.NonSerialized]
		internal RenderTexture internalLdrLut;
	}
}
