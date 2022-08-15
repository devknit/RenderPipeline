
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace RenderingPipeline
{
	[System.Serializable]
	public sealed class SpeedLineClip : PlayableAsset, ITimelineClipAsset
	{
		public ClipCaps clipCaps
		{
			get { return ClipCaps.Blending; }
		}
		public override Playable CreatePlayable( PlayableGraph graph, GameObject owner)
		{
			var playable = ScriptPlayable<SpeedLineBehaviour>.Create( graph, radialBlur);
			SpeedLineBehaviour behaviour = playable.GetBehaviour();
			behaviour.transform = transform.Resolve( graph.GetResolver());
			return playable;
		}
		
		[SerializeField]
		internal SpeedLineBehaviour radialBlur = new SpeedLineBehaviour();
		[SerializeField]
		internal ExposedReference<Transform> transform;
	}
}
