
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	[System.Serializable]
	public sealed partial class FXAA : PostProcess
	{
		public FXAAProperties Properties
		{
			get{ return (sharedSettings != null)? sharedSettings.properties : properties; }
		}
		internal override void Create()
		{
			if( shaderFXAA != null && materialFXAA == null)
			{
				materialFXAA = new Material( shaderFXAA);
			}
		}
		internal override void Dispose()
		{
			if( materialFXAA != null)
			{
				ObjectUtility.Release( materialFXAA);
				materialFXAA = null;
			}
		}
		internal override bool RestoreResources()
		{
			bool rebuild = false;
			
			if( ObjectUtility.IsMissing( materialFXAA) != false)
			{
				materialFXAA = new Material( shaderFXAA);
				rebuild = true;
			}
			return rebuild;
		}
		internal override bool Valid()
		{
			return Properties.Enabled != false && materialFXAA != null;
		}
		internal override void ClearCache()
		{
			Properties.ClearCache();
		}
		internal override bool CheckParameterChange( bool clearCache)
		{
			if( clearCache != false)
			{
				Properties.ClearCache();
			}
			return Properties.CheckParameterChange( materialFXAA);
		}
		internal override DepthTextureMode GetDepthTextureMode()
		{
			return DepthTextureMode.None;
		}
		internal override bool IsHighDynamicRange()
		{
			return false;
		}
		protected override bool OnDuplicate()
		{
			return false;
		}
		internal override void BuildCommandBuffer( 
			CommandBuffer commandBuffer, TargetContext context, 
			System.Func<int, int, int, FilterMode, RenderTextureFormat, int> GetTemporaryRT)
		{
			if( context.CompareSource0ToTarget0() != false)
			{
				int temporary = GetTemporaryRT( -1, -1, 0, FilterMode.Bilinear, TextureUtil.DefaultHDR);
				if( nextProcess == null)
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
			commandBuffer.SetGlobalTexture( kShaderPropertyMainTex, context.source0);
			pipeline.DrawFill( commandBuffer, materialFXAA, 0);
			context.duplicated = false;
		}
		
		[SerializeField]
        Shader shaderFXAA = default;
        [SerializeField]
        FXAASettings sharedSettings = default;
        [SerializeField]
        FXAAProperties properties = default;
        [System.NonSerialized]
		Material materialFXAA;
	}
}
