
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace RenderPipeline
{
	[System.Serializable]
	public sealed class BloomThresholdsClip : PlayableAsset, ITimelineClipAsset
	{
		public BloomThresholdsBehaviour bloom = new BloomThresholdsBehaviour();
		
		public ClipCaps clipCaps
	    {
	        get { return ClipCaps.None; }
	    }
	    public override Playable CreatePlayable( PlayableGraph graph, GameObject owner)
	    {
	        return ScriptPlayable<BloomThresholdsBehaviour>.Create( graph, bloom);
	    }
	}
}
