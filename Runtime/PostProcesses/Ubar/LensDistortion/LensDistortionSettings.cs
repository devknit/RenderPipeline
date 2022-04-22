
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderingPipeline
{
	[CreateAssetMenu( menuName="RenderPipeline/LensDistortion", fileName="PostProcessLensDistortion", order=1200)]
	public sealed class LensDistortionSettings : Settings<LensDistortionProperties>
	{
	}
	[System.Serializable]
	public sealed class LensDistortionProperties : IUbarProperties
	{
		public bool Enabled
		{
			get{ return enabled; }
			set{ enabled = value; }
		}
		public PostProcessEvent Phase
		{
			get{ return PostProcessEvent.PostTransparent; }
		}
		public float Amount
		{
			get{ return amount; }
			set{ amount = value; }
		}
		public float CenterX
		{
			get{ return centerX; }
			set{ centerX = value; }
		}
		public float CenterY
		{
			get{ return centerY; }
			set{ centerY = value; }
		}
		public float IntensityX
		{
			get{ return intensityX; }
			set{ intensityX = value; }
		}
		public float IntensityY
		{
			get{ return intensityY; }
			set{ intensityY = value; }
		}
		public float Scale
		{
			get{ return scale; }
			set{ scale = value; }
		}
		public void ClearCache()
		{
			cacheEnabled = null;
			cacheAmount = null;
			cacheCenterX = null;
			cacheCenterY = null;
			cacheIntensityX = null;
			cacheIntensityY = null;
			cacheScale = null;
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
				if( cacheAmount != amount
				||	cacheScale != scale)
				{
					float value = 1.6f * Mathf.Max( Mathf.Abs( amount), 1.0f);
					float theta = Mathf.Deg2Rad * Mathf.Min( 160f, value);
					float sigma = 2.0f * Mathf.Tan( theta * 0.5f);
					
					material.SetVector( kShaderPropertyAmount, 
						new Vector4( (amount >= 0.0f)? theta : 1.0f / theta, sigma, 1.0f / scale, amount));
					cacheAmount = amount;
					cacheScale = scale;
				}
				if( cacheCenterX != centerX
				||	cacheCenterY != centerY
				||	cacheIntensityX != intensityX
				||	cacheIntensityY != intensityY)
				{
					material.SetVector( kShaderPropertyCenterScale, 
						new Vector4( centerX, centerY, intensityX, intensityY));
					cacheCenterX = centerX;
					cacheCenterY = centerY;
					cacheIntensityX = intensityX;
					cacheIntensityY = intensityY;
				}
				if( Mathf.Approximately( amount, 0.0f) == false && (intensityX > 0.0f || intensityY > 0.0f))
				{
					if( material.IsKeywordEnabled( kShaderKeywordLensDistortion) == false)
					{
						material.EnableKeyword( kShaderKeywordLensDistortion);
					}
				}
				else if( material.IsKeywordEnabled( kShaderKeywordLensDistortion) != false)
				{
					material.DisableKeyword( kShaderKeywordLensDistortion);
				}
			}
			else if( material.IsKeywordEnabled( kShaderKeywordLensDistortion) != false)
			{
				material.DisableKeyword( kShaderKeywordLensDistortion);
			}
			return rebuild;
		}
		
		const string kShaderKeywordLensDistortion = "_LENSDISTORTION";
		static readonly int kShaderPropertyAmount = Shader.PropertyToID( "_LensDistortionAmount");
		static readonly int kShaderPropertyCenterScale = Shader.PropertyToID( "_LensDistortionCenterScale");
		
		[SerializeField]
		bool enabled = true;
		[SerializeField, Range( -100, 100)]
		float amount = 0.0f;
		[SerializeField, Range( -1, 1)]
		float centerX = 0.0f;
		[SerializeField, Range( -1, 1)]
		float centerY = 0.0f;
		[SerializeField, Range( 0, 1)]
		float intensityX = 1.0f;
		[SerializeField, Range( 0, 1)]
		float intensityY = 1.0f;
		[SerializeField, Range( 0.01f, 5.0f)]
		float scale = 1.0f;
		
		[System.NonSerialized]
		bool? cacheEnabled;
		[System.NonSerialized]
		float? cacheAmount;
		[System.NonSerialized]
		float? cacheCenterX;
		[System.NonSerialized]
		float? cacheCenterY;
		[System.NonSerialized]
		float? cacheIntensityX;
		[System.NonSerialized]
		float? cacheIntensityY;
		[System.NonSerialized]
		float? cacheScale;
	}
}
