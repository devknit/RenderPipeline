
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderingPipeline
{
	[CreateAssetMenu( menuName="RenderPipeline/Overray", fileName="PostProcessOverray", order=1200)]
	public sealed class OverraySettings : Settings<OverrayProperties>
	{
	}
	[System.Serializable]
	public sealed class OverrayProperties : IUbarProperties
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
		public Texture Texture
		{
			set{ texture = value; }
		}
		public Color Color
		{
			get{ return color; }
			set{ color = value; }
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
		public bool ScreenShot( System.Action<bool> onComplete)
		{
			if( isScreenShot == false && onScreenShotComplete == null)
			{
				isScreenShot = true;
				onScreenShotComplete = onComplete;
				return true;
			}
			return false;
		}
		public bool Capture( bool enable)
		{
			if( capture != enable)
			{
				capture = enable;
				return true;
			}
			return false;
		}
		public void ClearCache()
		{
			cacheEnabled = null;
			cacheTexture = Texture2D.whiteTexture;
			cacheColor = null;
			cacheStencilReference = null;
			cacheStencilReadMask = null;
			cacheStencilCompare = null;
		}
		public bool UpdateProperties( RenderPipeline pipeline, Material material)
		{
			return UpdateUbarProperties( pipeline, material, false);
		}
		public bool UpdateUbarProperties( RenderPipeline pipeline, Material material, bool forcedDisable)
		{
			bool rebuild = false;
			
			if( capture != false || isScreenShot != false)
			{
				if( pipeline.Capture( phase, (capture) =>
				{
					if( enabled != false)
					{
						texture = capture;
					}
					onScreenShotComplete?.Invoke( enabled);
					onScreenShotComplete = null;
				}) == false)
				{
					onScreenShotComplete?.Invoke( false);
					onScreenShotComplete = null;
				}
				isScreenShot = false;
			}
			if( cacheEnabled != enabled)
			{
				rebuild = true;
				cacheEnabled = enabled;
			}
			if( enabled != false && forcedDisable == false)
			{
				if( material.IsKeywordEnabled( kShaderKeywordOverray) == false)
				{
					material.EnableKeyword( kShaderKeywordOverray);
				}
				if( cacheTexture != texture)
				{
					material.SetTexture( kShaderPropertyTexture, (texture != null)? texture : Texture2D.whiteTexture);
					cacheTexture = texture;
				}
				if( cacheColor != color)
				{
					material.SetColor( kShaderPropertyColor, color);
					cacheColor = color;
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
			else if( material.IsKeywordEnabled( kShaderKeywordOverray) != false)
			{
				material.DisableKeyword( kShaderKeywordOverray);
			}
			return rebuild;
		}
		internal long GetDepthStencilHashCode()
		{
			return DepthStencil.GetHashCode( stencilReference, stencilReadMask, 255, stencilCompare);
		}
		
		const string kShaderKeywordOverray = "_OVERRAY";
		const string kShaderKeywordOverrayCapture = "_OVERRAY_CAPTURE";
		static readonly int kShaderPropertyTexture = Shader.PropertyToID( "_OverrayTex");
		static readonly int kShaderPropertyColor = Shader.PropertyToID( "_OverrayColor");
		static readonly int kShaderPropertyFlipY = Shader.PropertyToID( "_OverrayFlipY");
		
		[SerializeField]
		bool enabled = true;
		[SerializeField]
		PostProcessEvent phase = PostProcessEvent.BeforeImageEffects;
		[SerializeField]
		Texture texture = default;
		[SerializeField]
		Color color = new Color( 1, 1, 1, 0);
		[SerializeField, Range(0, 255)]
		byte stencilReference = 0;
		[SerializeField, Range(0, 255)]
		byte stencilReadMask = 255;
		[SerializeField]
		CompareFunction stencilCompare = CompareFunction.Always;
		
		[System.NonSerialized]
		bool capture = false;
		[System.NonSerialized]
		bool isScreenShot = false;
		[System.NonSerialized]
		System.Action<bool> onScreenShotComplete;
		
		[System.NonSerialized]
		bool? cacheEnabled;
		[System.NonSerialized]
		Texture cacheTexture;
		[System.NonSerialized]
		Color? cacheColor;
		[System.NonSerialized]
        byte? cacheStencilReference;
        [System.NonSerialized]
        byte? cacheStencilReadMask;
        [System.NonSerialized]
        CompareFunction? cacheStencilCompare;
	}
}
