
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace RenderPipeline
{
	[System.Serializable]
	public sealed class ScreenBlendColorClip : PlayableAsset, ITimelineClipAsset
	{
		public ScreenBlendColorBehaviour screenBlend = new ScreenBlendColorBehaviour();
		
		public ClipCaps clipCaps
	    {
	        get { return ClipCaps.Blending; }
	    }
	    public override Playable CreatePlayable( PlayableGraph graph, GameObject owner)
	    {
	        return ScriptPlayable<ScreenBlendColorBehaviour>.Create( graph, screenBlend);
	    }
	}
}
