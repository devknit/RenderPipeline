
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderingPipeline
{
	public abstract class PostProcess : MonoBehaviour, IPostProcess
	{
		public abstract bool Enabled
		{
			get;
			set;
		}
		public abstract void Create();
		public abstract void Dispose();
		public abstract bool RestoreMaterials();
		public abstract bool Valid();
		public abstract void ClearPropertiesCache();
		public abstract bool UpdateProperties( RenderPipeline pipeline, bool clearCache);
		public abstract PostProcessEvent GetPostProcessEvent();
		public abstract DepthTextureMode GetDepthTextureMode();
		public abstract bool IsRequiredHighDynamicRange();
		public abstract bool BuildCommandBuffer( RenderPipeline pipeline,
			CommandBuffer commandBuffer, TargetContext context, IPostProcess nextProcess);
		
		public virtual long GetDepthStencilHashCode()
		{
			return DepthStencil.kDefaultHash;
		}
		internal bool DuplicateMRT()
		{
			if( SystemInfo.supportedRenderTargetCount > 1)
			{
				return false; // OnDuplicateMRT();
			}
			return false;
		}
		
	#if UNITY_EDITOR
		internal bool ChangePostProcessEvent()
		{
			PostProcessEvent postProcessEvent = GetPostProcessEvent();
			if( cachePostProcessEvent != postProcessEvent)
			{
				cachePostProcessEvent = postProcessEvent;
				return true;
			}
			return false;
		}
		PostProcessEvent? cachePostProcessEvent;
	#endif
	}
}
