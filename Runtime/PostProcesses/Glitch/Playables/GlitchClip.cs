
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace RenderPipeline
{
	[System.Serializable]
	public sealed class GlitchClip : PlayableAsset, ITimelineClipAsset
	{
		public ClipCaps clipCaps
		{
			get { return ClipCaps.Blending; }
		}
		public override Playable CreatePlayable( PlayableGraph graph, GameObject owner)
		{
			return ScriptPlayable<GlitchBehaviour>.Create( graph, glitch);
		}
		
		public GlitchBehaviour glitch = new GlitchBehaviour();
	}
}
