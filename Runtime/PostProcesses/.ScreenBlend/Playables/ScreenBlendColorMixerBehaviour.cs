﻿
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace RenderingPipeline
{
	public sealed class ScreenBlendColorMixerBehaviour : PlayableBehaviour
	{
		public override void ProcessFrame( Playable playable, FrameData info, object playerData)
		{
			if( playerData is RenderPipeline renderPipeline && renderPipeline.ScreenBlend != null)
			{
				if( cachedScreenBlend == null)
				{
					cachedScreenBlend = renderPipeline.ScreenBlend;
				}
				int inputCount = playable.GetInputCount();
				Color blendedColor = Color.clear;
				float totalWeight = 0.0f;
				
				for( int i0 = 0; i0 < inputCount; ++i0)
				{
					var playableInput = (ScriptPlayable<ScreenBlendColorBehaviour>)playable.GetInput( i0);
					ScreenBlendColorBehaviour input = playableInput.GetBehaviour();
					float inputWeight = playable.GetInputWeight( i0);

					blendedColor += input.color * inputWeight;
					totalWeight += inputWeight;
				}
				cachedScreenBlend.Properties.ApplyBlendColor( blendedColor, totalWeight);
			}
		}
		public override void OnGraphStop( Playable playable)
		{
		#if UNITY_EDITOR
			if( UnityEditor.EditorApplication.isPlaying == false)
			{
				return;
			}
		#endif
			if( cachedScreenBlend != null)
			{
				cachedScreenBlend.Properties.RestoreBlendColor( restoreSeconds);
				cachedScreenBlend = null;
			}
		}
	#if UNITY_EDITOR
		public override void OnPlayableDestroy( Playable playable)
		{
			if( UnityEditor.EditorApplication.isPlaying == false && cachedScreenBlend != null)
			{
				cachedScreenBlend.Properties.RestoreBlendColor( 0.0f);
				cachedScreenBlend = null;
			}
		}
	#endif
		
		ScreenBlend cachedScreenBlend;
		internal float restoreSeconds;
	}
}
