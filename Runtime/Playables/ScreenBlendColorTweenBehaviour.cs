
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace RenderPipeline
{
	[System.Serializable]
	public sealed class ScreenBlendColorTweenBehaviour : PlayableBehaviour
	{
		public Color startColor = Color.clear;
		public Color endColor = Color.clear;
		public AnimationCurve transition = new AnimationCurve
	    (
	        new Keyframe( 0.0f, 0.0f, 0.0f, 0.0f),
	        new Keyframe( 1.0f, 1.0f, 0.0f, 0.0f)
	    );
	}
}
