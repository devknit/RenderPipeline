
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	public abstract class GenericProcess<TSettings, TProperties> : PostProcess
		where TSettings : Settings<TProperties>
		where TProperties : IGenericProperties
	{
		public sealed override bool Enabled
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
		public sealed override bool Valid()
		{
			return Enabled != false && material != null;
		}
		public override void ClearPropertiesCache()
		{
			sharedSettings?.properties.ClearCache();
			properties.ClearCache();
		}
		public sealed override bool UpdateProperties( RenderPipeline pipeline, bool clearCache)
		{
			if( clearCache != false)
			{
				ClearPropertiesCache();
			}
			return OnUpdateProperties( pipeline, material);
		}
		public override PostProcessEvent GetPostProcessEvent()
		{
			return Properties.Phase;
		}
		protected abstract bool OnUpdateProperties( RenderPipeline pipeline, Material material);
		
		[SerializeField]
		TSettings sharedSettings = default;
		[SerializeField]
		TProperties properties = default;
		[SerializeField]
		bool useSharedProperties = true;
		[SerializeField]
		Shader shader = default;
		[System.NonSerialized]
		protected Material material;
	}
}
