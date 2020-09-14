
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace RenderPipeline
{
	[TrackColor( 0.9f, 0.3f, 0.1f)]
	[TrackClipType( typeof( ScreenBlendColorTweenClip))]
	[TrackBindingType( typeof( RenderPipeline))]
	public sealed class ScreenBlendColorTweenTrack : TrackAsset
	{
		public override Playable CreateTrackMixer( PlayableGraph graph, GameObject go, int inputCount)
	    {
	        return ScriptPlayable<ScreenBlendColorTweenMixerBehaviour>.Create( graph, inputCount);
	    }
	}
}
