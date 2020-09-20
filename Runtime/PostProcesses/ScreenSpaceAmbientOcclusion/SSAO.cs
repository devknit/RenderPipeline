
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	[System.Serializable]
	public sealed partial class SSAO : PostProcess
	{
		public SSAOProperties Properties
		{
			get{ return (sharedSettings != null)? sharedSettings.properties : properties; }
		}
		internal override void Create()
		{
			if( shaderSSAO != null && materialSSAO == null)
			{
				materialSSAO = new Material( shaderSSAO);
			}
		}
		internal override void Dispose()
		{
			if( materialSSAO != null)
			{
				ObjectUtility.Release( materialSSAO);
				materialSSAO = null;
			}
		}
		internal override bool RestoreResources()
		{
			bool rebuild = false;
			
			if( ObjectUtility.IsMissing( materialSSAO) != false)
			{
				materialSSAO = new Material( shaderSSAO);
				rebuild = true;
			}
			return rebuild;
		}
		internal override bool Valid()
		{
			return Properties.Enabled != false && materialSSAO != null;
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
			return Properties.CheckParameterChange( materialSSAO, (width, height) => 
			{
				shadeDescriptor = new RenderTextureDescriptor( width, height, TextureUtil.DefaultHDR);
				shadeDescriptor.useMipMap = false;
				shadeDescriptor.autoGenerateMips = false;
				
				halfDescriptor = new RenderTextureDescriptor( width / 2, height / 2, TextureUtil.DefaultHDR);
				halfDescriptor.useMipMap = false;
				halfDescriptor.autoGenerateMips = false;
				
				quarterDescriptor = new RenderTextureDescriptor( width / 4, height / 4, TextureUtil.DefaultHDR);
				quarterDescriptor.useMipMap = false;
				quarterDescriptor.autoGenerateMips = false;
			});
		}
		internal override DepthTextureMode GetDepthTextureMode()
		{
			return DepthTextureMode.Depth;
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
			if( Properties.DebugMode != false)
			{
				commandBuffer.SetRenderTarget( 
					context.target0,
					RenderBufferLoadAction.DontCare,	
					RenderBufferStoreAction.Store,
					RenderBufferLoadAction.DontCare,	
					RenderBufferStoreAction.DontCare);
				commandBuffer.SetGlobalTexture( kShaderPropertyMainTex, context.source0);
				pipeline.DrawFill( commandBuffer, materialSSAO, 0);
			}
			else
			{
				/**/
				var shadeTarget = new RenderTargetIdentifier( kShaderTargetShade);
				commandBuffer.GetTemporaryRT( kShaderTargetShade, shadeDescriptor, FilterMode.Bilinear);
				
				commandBuffer.SetRenderTarget( 
					shadeTarget, 
					RenderBufferLoadAction.DontCare,	
					RenderBufferStoreAction.Store,
					RenderBufferLoadAction.DontCare,	
					RenderBufferStoreAction.DontCare);
				if( SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal)
				{
					commandBuffer.ClearRenderTarget( false, true, Color.black, 0);
				}
				commandBuffer.SetGlobalTexture( kShaderPropertyMainTex, context.source0);
				pipeline.DrawFill( commandBuffer, materialSSAO, 0);
				
				/**/
				var halfTarget = new RenderTargetIdentifier( kShaderTargetHalf);
				commandBuffer.GetTemporaryRT( kShaderTargetHalf, halfDescriptor, FilterMode.Bilinear);
				
				commandBuffer.SetRenderTarget( 
					halfTarget, 
					RenderBufferLoadAction.DontCare,	
					RenderBufferStoreAction.Store,
					RenderBufferLoadAction.DontCare,	
					RenderBufferStoreAction.DontCare);
				if( SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal)
				{
					commandBuffer.ClearRenderTarget( false, true, Color.black, 0);
				}
				commandBuffer.SetGlobalTexture( kShaderPropertyMainTex, shadeTarget);
				pipeline.DrawFill( commandBuffer, materialSSAO, 1);
				
				/**/
				var quarterTarget = new RenderTargetIdentifier( kShaderTargetQuarter);
				commandBuffer.GetTemporaryRT( kShaderTargetQuarter, quarterDescriptor, FilterMode.Bilinear);
				
				commandBuffer.SetRenderTarget( 
					quarterTarget, 
					RenderBufferLoadAction.DontCare,	
					RenderBufferStoreAction.Store,
					RenderBufferLoadAction.DontCare,	
					RenderBufferStoreAction.DontCare);
				if( SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal)
				{
					commandBuffer.ClearRenderTarget( false, true, Color.black, 0);
				}
				commandBuffer.SetGlobalTexture( kShaderPropertyMainTex, halfTarget);
				pipeline.DrawFill( commandBuffer, materialSSAO, 1);
				
				/**/
				commandBuffer.SetRenderTarget( 
					context.target0,
					RenderBufferLoadAction.DontCare,	
					RenderBufferStoreAction.Store,
					RenderBufferLoadAction.DontCare,	
					RenderBufferStoreAction.DontCare);
				commandBuffer.SetGlobalTexture( kShaderPropertyMainTex, context.source0);
				commandBuffer.SetGlobalTexture( kShaderPropertyBlurTex, quarterTarget);
				pipeline.DrawFill( commandBuffer, materialSSAO, 2);
				
				commandBuffer.ReleaseTemporaryRT( kShaderTargetShade);
				commandBuffer.ReleaseTemporaryRT( kShaderTargetHalf);
				commandBuffer.ReleaseTemporaryRT( kShaderTargetQuarter);
			}
			context.duplicated = false;
		}
		
		static readonly int kShaderTargetShade = Shader.PropertyToID( "_SSAO.Target.Shade");
		static readonly int kShaderTargetHalf = Shader.PropertyToID( "_SSAO.Target.Half");
		static readonly int kShaderTargetQuarter = Shader.PropertyToID( "_SSAO.Target.Quarter");
		
		static readonly int kShaderPropertyBlurTex = Shader.PropertyToID( "_BlurTex");
		
		
		[SerializeField]
        Shader shaderSSAO = default;
        [SerializeField]
        SSAOSettings sharedSettings = default;
        [SerializeField]
        SSAOProperties properties = default;
        [System.NonSerialized]
		Material materialSSAO;
		[System.NonSerialized]
		RenderTextureDescriptor shadeDescriptor;
		[System.NonSerialized]
		RenderTextureDescriptor halfDescriptor;
		[System.NonSerialized]
		RenderTextureDescriptor quarterDescriptor;
	}
}
