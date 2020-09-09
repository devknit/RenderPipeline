
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace RenderPipeline
{
	[System.Serializable]
	public sealed class BloomSharedThresholdsBehaviour : PlayableBehaviour
	{
		public AnimationCurve thresholds = new AnimationCurve
	    (
	        new Keyframe( 0.0f, 1.0f, 0.0f, 0.0f),
	        new Keyframe( 0.5f, -5.0f, 0.0f, 0.0f),
	        new Keyframe( 1.0f, 1.0f, 0.0f, 0.0f)
	    );
	}
}
