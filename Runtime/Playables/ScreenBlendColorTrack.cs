
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace RenderPipeline
{
	[TrackColor( 0.9f, 0.3f, 0.1f)]
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
