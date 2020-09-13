
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace RenderPipeline
{
	public sealed class ScreenBlendColorMixerBehaviour : PlayableBehaviour
	{
		public override void ProcessFrame( Playable playable, FrameData info, object playerData)
	    {
			if( playerData is RenderPipeline renderPipeline)
			{
				if( cachedRenderPipeline == null)
				{
					defaultScreenBlendColor = renderPipeline.ScreenBlendColor;
					cachedRenderPipeline = renderPipeline;
				}
				int inputCount = playable.GetInputCount();
				Color defaultColor = renderPipeline.ScreenBlendColor;
				Color blendedColor = Color.clear;
				float totalWeight = 0.0f;
				
				for( int i0 = 0; i0 < inputCount; ++i0)
		        {
					var playableInput = (ScriptPlayable<ScreenBlendColorBehaviour>)playable.GetInput( i0);
		            ScreenBlendColorBehaviour input = playableInput.GetBehaviour();
		            float inputWeight = playable.GetInputWeight( i0);

					float normalisedTime = (float)(playableInput.GetTime() / playableInput.GetDuration());
					blendedColor += Color.Lerp( input.startColor, input.endColor, normalisedTime) * inputWeight;
		            totalWeight += inputWeight;
		        }
		        blendedColor += defaultColor * (1.0f - totalWeight);
		        renderPipeline.ScreenBlendColor = blendedColor;
			}
		}
		public override void OnGraphStop( Playable playable)
	    {
			if( cachedRenderPipeline != null)
			{
				cachedRenderPipeline.ScreenBlendColor = defaultScreenBlendColor;
			}
	    }
	    
	    RenderPipeline cachedRenderPipeline;
	    Color defaultScreenBlendColor;
	}
}
