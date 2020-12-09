
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	[CreateAssetMenu( menuName="RenderPipeline/ChromaticAberration", fileName="PostProcessChromaticAberration", order=1200)]
	public sealed class ChromaticAberrationSettings : Settings<ChromaticAberrationProperties>
	{
		void OnDisable()
		{
			properties.Dispose();
		}
	}
	[System.Serializable]
	public sealed class ChromaticAberrationProperties : IUbarProperties
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
		public float Intensity
		{
			get{ return intensity; }
			set{ intensity = value; }
		}
		public bool FastMode
		{
			get{ return fastMode; }
			set{ fastMode = value; }
		}
		public void ClearCache()
		{
			cacheEnabled = null;
			cacheIntensity = null;
			cacheSpectralLut = null;
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
				if( material.IsKeywordEnabled( kShaderKeywordChromaticAberration) == false)
				{
					material.EnableKeyword( kShaderKeywordChromaticAberration);
				}
				if( cacheSpectralLut != spectralLut || spectralLut == null)
				{
					if( spectralLut == null)
					{
						if( internalSpectralLut == null)
						{
							internalSpectralLut = new Texture2D( 3, 1, TextureFormat.RGB24, false)
							{
								name = "ChromaticAberrationSpectralLut",
								filterMode = FilterMode.Bilinear,
								wrapMode = TextureWrapMode.Clamp,
								anisoLevel = 0,
								hideFlags = HideFlags.DontSave
							};
							internalSpectralLut.SetPixels(new []
							{
								new Color( 1.0f, 0.0f, 0.0f),
								new Color( 0.0f, 1.0f, 0.0f),
								new Color( 0.0f, 0.0f, 1.0f)
							});
							internalSpectralLut.Apply();
						}
						material.SetTexture( kShaderPropertySpectralLut, internalSpectralLut);
					}
					else
					{
						material.SetTexture( kShaderPropertySpectralLut, spectralLut);
					}
					cacheSpectralLut = spectralLut;
				}
				if( cacheIntensity != intensity)
				{
					material.SetFloat( kShaderPropertyIntensity, intensity * 0.05f);
					cacheIntensity = intensity;
				}
				if( fastMode != false)
				{
					if( material.IsKeywordEnabled( kShaderKeywordChromaticAberrationFastMode) == false)
					{
						material.EnableKeyword( kShaderKeywordChromaticAberrationFastMode);
					}
				}
				else if( material.IsKeywordEnabled( kShaderKeywordChromaticAberrationFastMode) != false)
				{
					material.DisableKeyword( kShaderKeywordChromaticAberrationFastMode);
				}
			}
			else
			{
				if( material.IsKeywordEnabled( kShaderKeywordChromaticAberration) != false)
				{
					material.DisableKeyword( kShaderKeywordChromaticAberration);
				}
				if( material.IsKeywordEnabled( kShaderKeywordChromaticAberrationFastMode) != false)
				{
					material.DisableKeyword( kShaderKeywordChromaticAberrationFastMode);
				}
			}
			return rebuild;
		}
		internal void Dispose()
		{
			if( internalSpectralLut != null)
			{
				ObjectUtility.Release( internalSpectralLut);
				internalSpectralLut = null;
			}
		}
		
		const string kShaderKeywordChromaticAberration = "_CHROMATICABERRATION";
		const string kShaderKeywordChromaticAberrationFastMode = "_CHROMATICABERRATION_FASTMODE";
		static readonly int kShaderPropertySpectralLut = Shader.PropertyToID( "_ChromaticAberrationSpectralLut");
		static readonly int kShaderPropertyIntensity = Shader.PropertyToID( "_ChromaticAberrationIntensity");
		
		[SerializeField]
		bool enabled = true;
		[SerializeField]
		Texture2D spectralLut = default;
		[SerializeField, Range( 0, 1)]
		float intensity = 1.0f;
		[SerializeField]
		bool fastMode = true;
		
		[System.NonSerialized]
		bool? cacheEnabled;
		[System.NonSerialized]
		Texture2D cacheSpectralLut;
		[System.NonSerialized]
		float? cacheIntensity;
		[System.NonSerialized]
		internal Texture2D internalSpectralLut;
	}
}
