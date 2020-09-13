
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace RenderPipeline
{
	[TrackColor( 0.855f, 0.8623f, 0.87f)]
	[TrackClipType( typeof( ScreenBlendColorClip))]
	[TrackBindingType( typeof( RenderPipeline))]
	public sealed class ScreenBlendColorTrack : TrackAsset
	{
		public override Playable CreateTrackMixer( PlayableGraph graph, GameObject go, int inputCount)
	    {
	        return ScriptPlayable<ScreenBlendColorMixerBehaviour>.Create( graph, inputCount);
	    }
	}
}
