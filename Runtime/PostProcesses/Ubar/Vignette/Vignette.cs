
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	[DisallowMultipleComponent]
	public sealed partial class Vignette : UbarProperty
	{
		internal override bool Enabled
		{
			get{ return ((sharedSettings != null)? sharedSettings.properties : properties).Enabled; }
		}
		public VignetteProperties Properties
		{
			get{ return (sharedSettings != null && useSharedProperties != false)? sharedSettings.properties : properties; }
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
        VignetteSettings sharedSettings = default;
        [SerializeField]
        VignetteProperties properties = default;
		[SerializeField]
		bool useSharedProperties = true;
	}
}
