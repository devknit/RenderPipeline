
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	[System.Serializable]
	public sealed partial class ScreenBlend : PostProcess
	{
		public ScreenBlendProperties Properties
		{
			get{ return (sharedSettings != null)? sharedSettings.properties : properties; }
		}
		public override void Create()
		{
			if( shader != null && material == null)
			{
				material = new Material( shader);
			}
		}
		public override void Dispose()
		{
			if( material != null)
			{
				ObjectUtility.Release( material);
				material = null;
			}
		}
		public override bool RestoreMaterials()
		{
			bool rebuild = false;
			
			if( shader != null && material == null)
			{
				material = new Material( shader);
				rebuild = true;
			}
			return rebuild;
		}
		public override bool Valid()
		{
			return Properties.Enabled != false && material != null;
		}
		public override void ClearPropertiesCache()
		{
			Properties.ClearCache();
		}
		public override bool UpdateProperties( bool clearCache)
		{
			if( clearCache != false)
			{
				Properties.ClearCache();
			}
			return Properties.CheckParameterChange( material);
		}
		public override CameraEvent GetCameraEvent()
		{
			return CameraEvent.BeforeImageEffects;
		}
		public override DepthTextureMode GetDepthTextureMode()
		{
			return DepthTextureMode.None;
		}
		public override bool IsHighDynamicRange()
		{
			return false;
		}
		public override void BuildCommandBuffer( 
			CommandBuffer commandBuffer, TargetContext context, 
			System.Func<int, int, int, FilterMode, RenderTextureFormat, int> GetTemporaryRT)
		{
			if( context.CompareSource0ToTarget0() != false)
			{
				int temporary = GetTemporaryRT( -1, -1, 0, FilterMode.Bilinear, TextureUtil.DefaultHDR);
				if( NextProcess == null)
				{
					commandBuffer.Blit( context.source0, temporary);
					context.SetSource0( temporary);
				}
				else
				{
					context.SetTarget0( temporary);
				}
			}
			commandBuffer.SetRenderTarget( 
				context.target0, 
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.Store,
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.DontCare);
			commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, context.source0);
			Pipeline.DrawFill( commandBuffer, material, 0);
			context.duplicated = false;
		}
		const string kShaderKeywordFlipHorizontal = "FLIPHORIZONTAL";
		
		[SerializeField]
        ScreenBlendSettings sharedSettings = default;
        [SerializeField]
        ScreenBlendProperties properties = default;
		[SerializeField]
        Shader shader = default;
		[System.NonSerialized]
		Material material;
	}
}
