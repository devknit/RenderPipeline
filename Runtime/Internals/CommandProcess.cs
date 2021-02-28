
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderingPipeline
{
	public abstract class CommandProcess<TProperties> : PostProcess
		where TProperties : IGenericProperties
	{
		public override bool Enabled
		{
			get{ return properties.Enabled; }
			set{ properties.Enabled = value; }
		}
		public TProperties Properties
		{
			get{ return properties; }
		}
		public override void Create()
		{
		}
		public override void Dispose()
		{
		}
		public override bool RestoreMaterials()
		{
			return false;
		}
		public override bool Valid()
		{
			return Enabled != false;
		}
		public override void ClearPropertiesCache()
		{
			properties.ClearCache();
		}
		public sealed override bool UpdateProperties( RenderPipeline pipeline, bool clearCache)
		{
			if( clearCache != false)
			{
				ClearPropertiesCache();
			}
			return OnUpdateProperties( pipeline);
		}
		public override PostProcessEvent GetPostProcessEvent()
		{
			return Properties.Phase;
		}
		public override DepthTextureMode GetDepthTextureMode()
		{
			return DepthTextureMode.None;
		}
		public override bool IsRequiredHighDynamicRange()
		{
			return false;
		}
		protected abstract bool OnUpdateProperties( RenderPipeline pipeline);
		
		[SerializeField]
		TProperties properties = default;
	}
}
