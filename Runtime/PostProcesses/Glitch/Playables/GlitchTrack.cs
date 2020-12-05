
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace RenderPipeline
{
	[TrackColor( 0.9f, 0.3f, 0.1f)]
	[TrackClipType( typeof( GlitchClip))]
	[TrackBindingType( typeof( RenderPipeline))]
	public sealed class GlitchTrack : TrackAsset
	{
		public override Playable CreateTrackMixer( PlayableGraph graph, GameObject go, int inputCount)
		{
			return ScriptPlayable<GlitchMixerBehaviour>.Create( graph, inputCount);
		}
	}
}
