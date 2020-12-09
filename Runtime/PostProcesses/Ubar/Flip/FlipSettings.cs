
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	[CreateAssetMenu( menuName="RenderPipeline/Flip", fileName="PostProcessFlip", order=1200)]
	public sealed class FlipSettings : Settings<FlipProperties>
	{
	}
	[System.Serializable]
	public sealed class FlipProperties : IUbarProperties
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
		public bool X
		{
			get{ return x; }
			set{ x = value; }
		}
		public bool Y
		{
			get{ return y; }
			set{ y = value; }
		}
		public void ClearCache()
		{
			cacheEnabled = null;
			cacheX = null;
			cacheY = null;
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
				if( cacheX != x)
				{
					if( x != false)
					{
						if( material.IsKeywordEnabled( kShaderKeywordFlipX) == false)
						{
							material.EnableKeyword( kShaderKeywordFlipX);
						}
					}
					else if( material.IsKeywordEnabled( kShaderKeywordFlipX) != false)
					{
						material.DisableKeyword( kShaderKeywordFlipX);
					}
					cacheX = x;
				}
				if( cacheY != y)
				{
					if( y != false)
					{
						if( material.IsKeywordEnabled( kShaderKeywordFlipY) == false)
						{
							material.EnableKeyword( kShaderKeywordFlipY);
						}
					}
					else if( material.IsKeywordEnabled( kShaderKeywordFlipY) != false)
					{
						material.DisableKeyword( kShaderKeywordFlipY);
					}
					cacheY = y;
				}
				
			}
			else
			{
				if( material.IsKeywordEnabled( kShaderKeywordFlipX) != false)
				{
					material.DisableKeyword( kShaderKeywordFlipX);
				}
				if( material.IsKeywordEnabled( kShaderKeywordFlipY) != false)
				{
					material.DisableKeyword( kShaderKeywordFlipY);
				}
			}
			return rebuild;
		}
		
		const string kShaderKeywordFlipX = "_FLIP_X";
		const string kShaderKeywordFlipY = "_FLIP_Y";
		
		[SerializeField]
		bool enabled = true;
		[SerializeField]
		bool x;
		[SerializeField]
		bool y;
		
		[System.NonSerialized]
		bool? cacheEnabled;
		[System.NonSerialized]
		bool? cacheX;
		[System.NonSerialized]
		bool? cacheY;
	}
}
