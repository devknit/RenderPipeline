
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace RenderingPipeline
{
	[System.Serializable]
	public sealed class VignetteClip : PlayableAsset, ITimelineClipAsset
	{
		public ClipCaps clipCaps
		{
			get { return ClipCaps.Blending; }
		}
		public override Playable CreatePlayable( PlayableGraph graph, GameObject owner)
		{
			return ScriptPlayable<VignetteBehaviour>.Create( graph, vignette);
		}
		
		public VignetteBehaviour vignette = new VignetteBehaviour();
	}
}
