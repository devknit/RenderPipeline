
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderingPipeline
{
	[CreateAssetMenu( menuName="RenderPipeline/RectMask", fileName="PostProcessRectMask", order=1200)]
	public sealed class RectMaskSettings : Settings<RectMaskProperties>
	{
	}
	[System.Serializable]
	public sealed class RectMaskProperties : IUbarProperties
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
		public Rect Rect
		{
			get{ return rect; }
			set{ rect = value; }
		}
		public float Smoothness
		{
			get{ return smoothness; }
			set{ smoothness = value; }
		}
		public void ClearCache()
		{
			cacheEnabled = null;
			cacheColor = null;
			cacheRect = null;
			cacheSmoothness = null;
		}
		public bool UpdateProperties( RenderPipeline pipeline, Material material)
		{
			return false;
		}
		public bool UpdateUbarProperties( RenderPipeline pipeline, Material material, bool forcedDisable)
		{
			bool rebuild = false;
			
			if( cacheEnabled != enabled)
			{
				rebuild = true;
				cacheEnabled = enabled;
			}
			if( enabled != false && forcedDisable == false)
			{
				if( material.IsKeywordEnabled( kShaderKeywordRectMask) == false)
				{
					material.EnableKeyword( kShaderKeywordRectMask);
				}
				if( cacheColor != color)
				{
					material.SetColor( kShaderPropertyColor, color);
					cacheColor = color;
				}
				if( cacheRect != rect)
				{
					material.SetVector( kShaderPropertyRect, new Vector4( rect.x, rect.y, rect.width, rect.height));
					cacheRect = rect;
				}
				if( cacheSmoothness != smoothness)
				{
					smoothness = Mathf.Clamp( smoothness, 0.0f, 1.0f);
					material.SetFloat( kShaderPropertySmoothness, smoothness);
					cacheSmoothness = smoothness;
				}
			}
			else if( material.IsKeywordEnabled( kShaderKeywordRectMask) != false)
			{
				material.DisableKeyword( kShaderKeywordRectMask);
			}
			return rebuild;
		}
		
		const string kShaderKeywordRectMask = "_RECTMASK";
		static readonly int kShaderPropertyColor = Shader.PropertyToID( "_RectMaskColor");
		static readonly int kShaderPropertyRect = Shader.PropertyToID( "_RectMaskRect");
		static readonly int kShaderPropertySmoothness = Shader.PropertyToID( "_RectMaskSmoothness");
		
		[SerializeField]
		bool enabled = true;
		[SerializeField]
		PostProcessEvent phase = PostProcessEvent.PostTransparent;
		[SerializeField]
		Color color = Color.black;
		[SerializeField]
		Rect rect = new Rect( 0, 0, 1, 1);
		[SerializeField, Range( 0, 1)]
		float smoothness = 1.0f;
		
		[System.NonSerialized]
		bool? cacheEnabled;
		[System.NonSerialized]
		Color? cacheColor;
		[System.NonSerialized]
		Rect? cacheRect;
		[System.NonSerialized]
		float? cacheSmoothness;
	}
}
