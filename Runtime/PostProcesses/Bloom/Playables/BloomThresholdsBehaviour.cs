
using UnityEngine;
using UnityEngine.Playables;

namespace RenderingPipeline
{
	[System.Serializable]
	public sealed class BloomThresholdsBehaviour : PlayableBehaviour
	{
		public AnimationCurve thresholds = new AnimationCurve
		(
			new Keyframe( 0.0f, 1.0f, 0.0f, 0.0f),
			new Keyframe( 0.5f, -5.0f, 0.0f, 0.0f),
			new Keyframe( 1.0f, 1.0f, 0.0f, 0.0f)
		);
	}
}
