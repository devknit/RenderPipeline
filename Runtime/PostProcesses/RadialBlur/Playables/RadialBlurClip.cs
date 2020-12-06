
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace RenderPipeline
{
	[System.Serializable]
	public sealed class RadialBlurClip : PlayableAsset, ITimelineClipAsset
	{
		public ClipCaps clipCaps
		{
			get { return ClipCaps.Blending; }
		}
		public override Playable CreatePlayable( PlayableGraph graph, GameObject owner)
		{
			return ScriptPlayable<RadialBlurBehaviour>.Create( graph, radialBlur);
		}
		
		public RadialBlurBehaviour radialBlur = new RadialBlurBehaviour();
	}
}
