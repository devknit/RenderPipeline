
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
			var scriptPlayable = ScriptPlayable<ScreenBlendColorMixerBehaviour>.Create( graph, inputCount);
			var mixerBehaviour = scriptPlayable.GetBehaviour();
			mixerBehaviour.restoreSeconds = restoreSeconds;
			return scriptPlayable;
		}
		
		[SerializeField, Range( 0, 1)]
		float restoreSeconds = 0.5f;
	}
}
