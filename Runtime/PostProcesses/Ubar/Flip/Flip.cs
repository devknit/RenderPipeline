
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	[DisallowMultipleComponent]
	public sealed partial class Flip : UbarPropertyEx<FlipSettings, FlipProperties>
	{
		public override PostProcessEvent GetPostProcessEvent()
		{
			return PostProcessEvent.BeforeImageEffects;
		}
	}
}
