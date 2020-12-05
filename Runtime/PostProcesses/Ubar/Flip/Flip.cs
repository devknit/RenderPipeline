
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	[DisallowMultipleComponent]
	public sealed partial class Flip : UbarProperty
	{
		internal override bool Enabled
		{
			get{ return ((sharedSettings != null)? sharedSettings.properties : properties).Enabled; }
		}
		public FlipProperties Properties
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
        FlipSettings sharedSettings = default;
        [SerializeField]
        FlipProperties properties = default;
		[SerializeField]
		bool useSharedProperties = true;
	}
}
