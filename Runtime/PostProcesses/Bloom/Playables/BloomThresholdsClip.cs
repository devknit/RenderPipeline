
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace RenderingPipeline
{
	[System.Serializable]
	public sealed class BloomThresholdsClip : PlayableAsset, ITimelineClipAsset
	{
		public ClipCaps clipCaps
		{
			get { return ClipCaps.None; }
		}
		public override Playable CreatePlayable( PlayableGraph graph, GameObject owner)
		{
			return ScriptPlayable<BloomThresholdsBehaviour>.Create( graph, bloom);
		}
		
		public BloomThresholdsBehaviour bloom = new BloomThresholdsBehaviour();
	}
}
