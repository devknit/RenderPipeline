
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	[System.Serializable]
	public sealed partial class ChromaticAberration : UbarProperty
	{
		public ChromaticAberrationProperties Properties
		{
			get{ return (sharedSettings != null)? sharedSettings.properties : properties; }
		}
		public override void Dispose()
		{
			properties.Dispose();
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
	}
}
