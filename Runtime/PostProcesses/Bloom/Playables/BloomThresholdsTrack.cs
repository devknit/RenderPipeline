
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace RenderPipeline
{
	[TrackColor( 0.855f, 0.8623f, 0.87f)]
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
