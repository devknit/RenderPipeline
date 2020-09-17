
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	public abstract class PostProcess : MonoBehaviour
	{
		internal void Initialize( RenderPipeline pipeline)
		{
			this.pipeline = pipeline;
		}
		internal abstract void Create();
		internal abstract void Dispose();
		internal abstract bool RestoreResources();
		internal abstract bool Valid();
		internal abstract DepthTextureMode GetDepthTextureMode();
		internal abstract bool IsHighDynamicRange();
		internal abstract bool CheckParameterChange( bool clearCache);
		internal bool DuplicateMRT()
		{
			if( SystemInfo.supportedRenderTargetCount > 1)
			{
				return OnDuplicate();
			}
			return false;
		}
		protected abstract bool OnDuplicate();
		internal abstract void BuildCommandBuffer( 
			CommandBuffer commandBuffer, TargetContext context, 
			System.Func<int, int, int, FilterMode, RenderTextureFormat, int> GetTemporaryRT);
		
		protected static readonly int kShaderPropertyMainTex = Shader.PropertyToID( "_MainTex");
		protected static readonly int kShaderPropertyColor = Shader.PropertyToID( "_Color");
		
		protected RenderPipeline pipeline;
		internal PostProcess nextProcess;
	}
}
