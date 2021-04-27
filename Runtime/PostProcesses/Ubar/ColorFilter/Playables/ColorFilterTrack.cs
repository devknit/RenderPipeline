
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace RenderingPipeline
{
	[TrackColor( 0.9f, 0.3f, 0.1f)]
	[TrackClipType( typeof( ColorFilterClip))]
	[TrackBindingType( typeof( RenderPipeline))]
	public sealed class ColorFilterTrack : TrackAsset
	{
		public override Playable CreateTrackMixer( PlayableGraph graph, GameObject go, int inputCount)
		{
			var playable = ScriptPlayable<ColorFilterMixerBehaviour>.Create( graph, inputCount);
			playable.GetBehaviour().leaveAsIs = leaveAsIs;
			return playable;
		}
		
		[SerializeField]
		bool leaveAsIs = false;
	}
}
