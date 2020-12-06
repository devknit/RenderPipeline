
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace RenderPipeline
{
	[System.Serializable]
	public sealed class ColorFilterClip : PlayableAsset, ITimelineClipAsset
	{
		public ClipCaps clipCaps
		{
			get { return ClipCaps.Blending; }
		}
		public override Playable CreatePlayable( PlayableGraph graph, GameObject owner)
		{
			return ScriptPlayable<ColorFilterBehaviour>.Create( graph, colorFilter);
		}
		
		public ColorFilterBehaviour colorFilter = new ColorFilterBehaviour();
	}
}
