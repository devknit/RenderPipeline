
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	[DisallowMultipleComponent]
	public sealed partial class LensDistortion : UbarPropertyEx<LensDistortionSettings, LensDistortionProperties>
	{
		public override PostProcessEvent GetPostProcessEvent()
		{
			return PostProcessEvent.BeforeImageEffects;
		}
	}
}
