
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
			return Properties.CheckParameterChange( material, (width, height) => 
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
		public override CameraEvent GetCameraEvent()
		{
			return CameraEvent.BeforeImageEffectsOpaque;
		}
		public override DepthTextureMode GetDepthTextureMode()
		{
			return DepthTextureMode.Depth;
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
			if( Properties.DebugMode != false)
			{
				commandBuffer.SetRenderTarget( 
					context.target0,
					RenderBufferLoadAction.DontCare,	
					RenderBufferStoreAction.Store,
					RenderBufferLoadAction.DontCare,	
					RenderBufferStoreAction.DontCare);
				commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, context.source0);
				Pipeline.DrawFill( commandBuffer, material, 0);
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
				Pipeline.DrawFill( commandBuffer, material, 0);
				
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
				Pipeline.DrawFill( commandBuffer, material, 1);
				
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
				Pipeline.DrawFill( commandBuffer, material, 1);
				
				/**/
				commandBuffer.SetRenderTarget( 
					context.target0,
					RenderBufferLoadAction.DontCare,	
					RenderBufferStoreAction.Store,
					RenderBufferLoadAction.DontCare,	
					RenderBufferStoreAction.DontCare);
				commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, context.source0);
				commandBuffer.SetGlobalTexture( kShaderPropertyBlurTex, quarterTarget);
				Pipeline.DrawFill( commandBuffer, material, 2);
				
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
