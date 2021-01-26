
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace RenderingPipeline
{
	[TrackColor( 0.9f, 0.3f, 0.1f)]
	[TrackClipType( typeof( BloomThresholdsClip))]
	[TrackBindingType( typeof( RenderPipeline))]
	public sealed class BloomThresholdsTrack : TrackAsset
	{
		public override Playable CreateTrackMixer( PlayableGraph graph, GameObject go, int inputCount)
		{
			return ScriptPlayable<BloomThresholdsMixerBehaviour>.Create( graph, inputCount);
		}
	}
}
