﻿
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderingPipeline
{
	public abstract class UbarProcess<TSettings, TProperties> :PostProcess, IUbarProcess
		where TSettings : Settings<TProperties>
		where TProperties : IUbarProperties
	{
		public override bool Enabled
		{
			get{ return ((sharedSettings != null)? sharedSettings.properties : properties).Enabled; }
			set{ ((sharedSettings != null)? sharedSettings.properties : properties).Enabled = value; }
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
		public override bool BuildCommandBuffer( RenderPipeline pipeline,
			CommandBuffer commandBuffer, TargetContext context, IPostProcess nextProcess)
		{
			return false;
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
				ClearPropertiesCache();
				cacheIndependent = independent;
				rebuild = true;
			}
			return independent;
		}
		public virtual bool Independent()
		{
			return DepthStencil.HasIndependent( GetDepthStencilHashCode());
		}
		public virtual bool PreProcess()
		{
			return false;
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
}
