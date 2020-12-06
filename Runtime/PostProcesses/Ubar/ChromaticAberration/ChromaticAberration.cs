
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	[DisallowMultipleComponent]
	public sealed partial class ChromaticAberration : UbarProcess<ChromaticAberrationSettings, ChromaticAberrationProperties>
	{
		public override void Dispose()
		{
			(properties as ChromaticAberrationProperties).Dispose();
		}
	}
}
