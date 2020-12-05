
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	[DisallowMultipleComponent]
	public sealed partial class SSAO : PostProcess
	{
		public SSAOProperties Properties
		{
			get{ return (sharedSettings != null && useSharedProperties != false)? sharedSettings.properties : properties; }
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
			return ((sharedSettings != null)? sharedSettings.properties : properties).Enabled != false && material != null;
		}
		public override void ClearPropertiesCache()
		{
			sharedSettings?.properties.ClearCache();
			properties.ClearCache();
		}
		public override bool UpdateProperties( RenderPipeline pipeline, bool clearCache)
		{
			if( clearCache != false)
			{
				ClearPropertiesCache();
			}
			return Properties.CheckParameterChange( pipeline, material, (width, height) => 
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
		public override PostProcessEvent GetPostProcessEvent()
		{
			return Properties.Phase;
		}
		public override DepthTextureMode GetDepthTextureMode()
		{
			return DepthTextureMode.Depth;
		}
		public override bool IsRequiredHighDynamicRange()
		{
			return false;
		}
		public override void BuildCommandBuffer( RenderPipeline pipeline,
			CommandBuffer commandBuffer, TargetContext context, IPostProcess nextProcess)
		{
			if( context.CompareSource0ToTarget0() != false)
			{
				int temporary = pipeline.GetTemporaryRT();
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
				commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, context.source0);
				pipeline.SetViewport( commandBuffer, nextProcess);
				pipeline.DrawFill( commandBuffer, material, 0);
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
				commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, context.source0);
				pipeline.DrawFill( commandBuffer, material, 0);
				
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
				commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, shadeTarget);
				pipeline.DrawFill( commandBuffer, material, 1);
				
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
				commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, halfTarget);
				pipeline.DrawFill( commandBuffer, material, 1);
				
				/**/
				commandBuffer.SetRenderTarget( 
					context.target0,
					RenderBufferLoadAction.DontCare,	
					RenderBufferStoreAction.Store,
					RenderBufferLoadAction.DontCare,	
					RenderBufferStoreAction.DontCare);
				commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, context.source0);
				commandBuffer.SetGlobalTexture( kShaderPropertyBlurTex, quarterTarget);
				pipeline.DrawFill( commandBuffer, material, 2);
				
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
        SSAOSettings sharedSettings = default;
        [SerializeField]
        SSAOProperties properties = default;
        [SerializeField]
		bool useSharedProperties = true;
        [SerializeField]
        Shader shader = default;
        [System.NonSerialized]
		Material material;
		[System.NonSerialized]
		RenderTextureDescriptor shadeDescriptor;
		[System.NonSerialized]
		RenderTextureDescriptor halfDescriptor;
		[System.NonSerialized]
		RenderTextureDescriptor quarterDescriptor;
	}
}
