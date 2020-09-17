
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	[System.Serializable]
	public sealed partial class Fxaa3 : PostProcess
	{
		public Fxaa3Properties Properties
		{
			get{ return (sharedSettings != null)? sharedSettings.properties : properties; }
		}
		internal override void Create()
		{
			if( shaderFxaa3 != null && materialFxaa3 == null)
			{
				materialFxaa3 = new Material( shaderFxaa3);
			}
		}
		internal override void Dispose()
		{
			if( materialFxaa3 != null)
			{
				ObjectUtility.Release( materialFxaa3);
				materialFxaa3 = null;
			}
		}
		internal override bool RestoreResources()
		{
			bool rebuild = false;
			
			if( ObjectUtility.IsMissing( materialFxaa3) != false)
			{
				materialFxaa3 = new Material( shaderFxaa3);
				rebuild = true;
			}
			return rebuild;
		}
		internal override bool Valid()
		{
			return Properties.Enabled != false && materialFxaa3 != null;
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
			return Properties.CheckParameterChange( materialFxaa3);
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
			pipeline.DrawFill( commandBuffer, materialFxaa3, 0);
			context.duplicated = false;
		}
		
		static readonly int kShaderPropertyEdgeThresholdMin = Shader.PropertyToID( "_EdgeThresholdMin");
		static readonly int kShaderPropertyEdgeThreshold = Shader.PropertyToID( "_EdgeThreshold");
		static readonly int kShaderPropertyEdgeSharpness = Shader.PropertyToID( "_EdgeSharpness");
		
		[SerializeField]
        Shader shaderFxaa3 = default;
        [SerializeField]
        Fxaa3Settings sharedSettings = default;
        [SerializeField]
        Fxaa3Properties properties = default;
        [System.NonSerialized]
		Material materialFxaa3;
	}
}
