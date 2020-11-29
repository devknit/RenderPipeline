
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace RenderPipeline
{
	[System.Serializable]
	public sealed class ScreenBlendColorTweenClip : PlayableAsset, ITimelineClipAsset
	{
		public ClipCaps clipCaps
		{
			get { return ClipCaps.None; }
		}
		public override Playable CreatePlayable( PlayableGraph graph, GameObject owner)
		{
			return ScriptPlayable<ScreenBlendColorTweenBehaviour>.Create( graph, screenBlend);
		}
		
		public ScreenBlendColorTweenBehaviour screenBlend = new ScreenBlendColorTweenBehaviour();
	}
}
