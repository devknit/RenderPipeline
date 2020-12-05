
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	[DisallowMultipleComponent]
	public sealed partial class ChromaticAberration : UbarProperty
	{
		internal override bool Enabled
		{
			get{ return ((sharedSettings != null)? sharedSettings.properties : properties).Enabled; }
		}
		public ChromaticAberrationProperties Properties
		{
			get{ return (sharedSettings != null && useSharedProperties != false)? sharedSettings.properties : properties; }
		}
		public override void Dispose()
		{
			properties.Dispose();
		}
		public override void ClearPropertiesCache()
		{
			base.ClearPropertiesCache();
			sharedSettings?.properties.ClearCache();
			properties.ClearCache();
		}
		public override PostProcessEvent GetPostProcessEvent()
		{
			return PostProcessEvent.BeforeImageEffects;
		}
		internal override IUbarProperties GetProperties()
		{
			return Properties;
		}
		
		[SerializeField]
        ChromaticAberrationSettings sharedSettings = default;
        [SerializeField]
        ChromaticAberrationProperties properties = default;
        [SerializeField]
		bool useSharedProperties = true;
	}
}
