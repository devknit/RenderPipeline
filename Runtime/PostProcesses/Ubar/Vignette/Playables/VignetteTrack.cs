
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace RenderingPipeline
{
	[TrackColor( 0.9f, 0.3f, 0.1f)]
	[TrackClipType( typeof( VignetteClip))]
	[TrackBindingType( typeof( RenderPipeline))]
	public sealed class VignetteTrack : TrackAsset
	{
		public override Playable CreateTrackMixer( PlayableGraph graph, GameObject go, int inputCount)
		{
			var playable = ScriptPlayable<VignetteMixerBehaviour>.Create( graph, inputCount);
			playable.GetBehaviour().leaveAsIs = leaveAsIs;
			return playable;
		}
		
		[SerializeField]
		bool leaveAsIs = false;
	}
}
