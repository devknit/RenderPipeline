
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
		internal enum PostPlaybackState
		{
			Revert,
			LeaveAsIs,
			Overwrite,
		}
		public override Playable CreateTrackMixer( PlayableGraph graph, GameObject go, int inputCount)
		{
			var playable = ScriptPlayable<ColorFilterMixerBehaviour>.Create( graph, inputCount);
			var behaviour = playable.GetBehaviour();
			behaviour.postPlaybackState = postPlaybackState;
			behaviour.overwriteDot = overwriteDot;
			behaviour.overwriteMultiply = overwriteMultiply;
			behaviour.overwriteAdd = overwriteAdd;
			behaviour.overwriteContrast = overwriteContrast;
			return playable;
		}
		
		[SerializeField]
		PostPlaybackState postPlaybackState = PostPlaybackState.Revert;
		[SerializeField, ColorUsage( true, true)]
		Color overwriteDot = ColorFilterProperties.kMonochromeDot;
		[SerializeField, ColorUsage( true, true)]
		Color overwriteMultiply = ColorFilterProperties.kSepiaMultiply;
		[SerializeField, ColorUsage( true, true)]
		Color overwriteAdd = Color.clear;
		[SerializeField, Range( 0, 1)]
		float overwriteContrast = 1;
	}
}
