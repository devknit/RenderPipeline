
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	public interface IUbarProperty : IPostProcess
	{
		bool Enabled
		{
			get;
		}
		bool HasIndependent( ref bool rebuild);
		bool Independent();
		IUbarProperties GetProperties();
	}
	public abstract class UbarProperty<TSettings, TProperties> :PostProcess, IUbarProperty
		where TSettings : Settings<TProperties>
		where TProperties : IUbarProperties
	{
		public override bool Enabled
		{
			get{ return ((sharedSettings != null)? sharedSettings.properties : properties).Enabled; }
		}
		public TProperties Properties
		{
			get{ return (sharedSettings != null && useSharedProperties != false)? sharedSettings.properties : properties; }
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
			return false;
		}
		public override void ClearPropertiesCache()
		{
			cacheIndependent = null;
			sharedSettings?.properties.ClearCache();
			properties.ClearCache();
		}
		public override bool UpdateProperties( RenderPipeline pipeline, bool clearCache)
		{
			return false;
		}
		public override DepthTextureMode GetDepthTextureMode()
		{
			return DepthTextureMode.None;
		}
		public override bool IsRequiredHighDynamicRange()
		{
			return false;
		}
		public override void BuildCommandBuffer( RenderPipeline pipeline,
			CommandBuffer commandBuffer, TargetContext context, IPostProcess nextProcess)
		{
		}
		public IUbarProperties GetProperties()
		{
			return Properties;
		}
		public bool HasIndependent( ref bool rebuild)
		{
			bool independent = Independent();
			
			if( cacheIndependent != independent)
			{
				cacheIndependent = independent;
				ClearPropertiesCache();
				rebuild = true;
			}
			return independent;
		}
		public virtual bool Independent()
		{
			return DepthStencil.HasIndependent( GetDepthStencilHashCode());
		}
		
		[SerializeField]
        protected TSettings sharedSettings = default;
        [SerializeField]
        protected TProperties properties = default;
        [SerializeField]
		protected bool useSharedProperties = true;
		[System.NonSerialized]
		bool? cacheIndependent;
	}
	public abstract class UbarPropertyRx<TSettings, TProperties> : UbarProperty<TSettings, TProperties>
		where TSettings : Settings<TProperties>
		where TProperties : IUbarProperties
	{
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
			return Enabled != false && material != null;
		}
		
		[SerializeField]
		Shader shader = default;
		[System.NonSerialized]
		protected Material material;
	}
}
