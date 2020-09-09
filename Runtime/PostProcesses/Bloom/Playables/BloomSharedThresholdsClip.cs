
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace RenderPipeline
{
	[System.Serializable]
	public sealed class BloomSharedThresholdsClip : PlayableAsset, ITimelineClipAsset
	{
		public BloomSharedThresholdsBehaviour bloom = new BloomSharedThresholdsBehaviour();
		
		public ClipCaps clipCaps
	    {
	        get { return ClipCaps.None; }
	    }
	    public override Playable CreatePlayable( PlayableGraph graph, GameObject owner)
	    {
	        return ScriptPlayable<BloomSharedThresholdsBehaviour>.Create( graph, bloom);
	    }
	}
}
