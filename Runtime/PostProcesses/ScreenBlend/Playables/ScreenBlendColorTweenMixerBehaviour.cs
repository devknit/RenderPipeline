
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace RenderingPipeline
{
	public sealed class ScreenBlendColorTweenMixerBehaviour : PlayableBehaviour
	{
		public override void ProcessFrame( Playable playable, FrameData info, object playerData)
		{
			if( playerData is RenderPipeline renderPipeline && renderPipeline.ScreenBlend != null)
			{
				if( cachedScreenBlend == null)
				{
					cachedScreenBlend = renderPipeline.ScreenBlend;
					defaultScreenBlendColor = cachedScreenBlend.Properties.Color;
				}
				int inputCount = playable.GetInputCount();
				Color defaultColor = cachedScreenBlend.Properties.Color;
				Color blendedColor = Color.clear;
				float totalWeight = 0.0f;
				
				for( int i0 = 0; i0 < inputCount; ++i0)
				{
					var playableInput = (ScriptPlayable<ScreenBlendColorTweenBehaviour>)playable.GetInput( i0);
					ScreenBlendColorTweenBehaviour input = playableInput.GetBehaviour();
					float inputWeight = playable.GetInputWeight( i0);
					
					float normalisedTime = (float)(playableInput.GetTime() / playableInput.GetDuration());
					blendedColor += Color.Lerp( input.startColor, input.endColor, input.transition.Evaluate( normalisedTime)) * inputWeight;
					totalWeight += inputWeight;
				}
				blendedColor += defaultColor * (1.0f - totalWeight);
				cachedScreenBlend.Properties.Color = blendedColor;
			}
		}
		public override void OnGraphStop( Playable playable)
		{
			if( cachedScreenBlend != null)
			{
				cachedScreenBlend.Properties.Color = defaultScreenBlendColor;
			}
		}
		
		ScreenBlend cachedScreenBlend;
		Color defaultScreenBlendColor;
	}
}
