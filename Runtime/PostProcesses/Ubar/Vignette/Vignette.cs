
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	[DisallowMultipleComponent]
	public sealed partial class Vignette : UbarPropertyEx<VignetteSettings, VignetteProperties>
	{
		public override PostProcessEvent GetPostProcessEvent()
		{
			return PostProcessEvent.BeforeImageEffects;
		}
	}
}
