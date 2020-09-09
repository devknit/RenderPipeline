
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace RenderPipeline
{
	[TrackColor( 0.855f, 0.8623f, 0.87f)]
	[TrackClipType( typeof( BloomSharedThresholdsClip))]
	public sealed class BloomSharedThresholdsTrack : TrackAsset
	{
		public override Playable CreateTrackMixer( PlayableGraph graph, GameObject go, int inputCount)
	    {
	        return ScriptPlayable<BloomSharedThresholdsMixerBehaviour>.Create( graph, inputCount);
	    }
	}
}
