
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace RenderingPipeline
{
	[TrackColor( 0.9f, 0.3f, 0.1f)]
	[TrackClipType( typeof( RadialBlurClip))]
	[TrackBindingType( typeof( RenderPipeline))]
	public sealed class RadialBlurTrack : TrackAsset
	{
		public override Playable CreateTrackMixer( PlayableGraph graph, GameObject go, int inputCount)
		{
			return ScriptPlayable<RadialBlurMixerBehaviour>.Create( graph, inputCount);
		}
	}
}
