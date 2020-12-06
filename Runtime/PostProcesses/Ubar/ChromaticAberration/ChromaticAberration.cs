
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	[DisallowMultipleComponent]
	public sealed partial class ChromaticAberration : UbarProperty<ChromaticAberrationSettings, ChromaticAberrationProperties>
	{
		public override void Dispose()
		{
			(properties as ChromaticAberrationProperties).Dispose();
		}
		public override PostProcessEvent GetPostProcessEvent()
		{
			return PostProcessEvent.BeforeImageEffects;
		}
	}
}
