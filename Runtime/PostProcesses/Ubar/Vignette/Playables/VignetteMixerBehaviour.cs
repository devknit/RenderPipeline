
using UnityEngine;
using UnityEngine.Playables;

namespace RenderingPipeline
{
	public sealed class VignetteMixerBehaviour : PlayableBehaviour
	{
		public override void ProcessFrame( Playable playable, FrameData info, object playerData)
		{
			if( playerData is RenderPipeline renderPipeline)
			{
				if( renderPipelineComponent == null)
				{
					vignetteComponent = renderPipeline.FindPostProcess<Vignette>();
					if( vignetteComponent != null)
					{
						defaultColor = vignetteComponent.Properties.Color;
						defaultCenter = vignetteComponent.Properties.Center;
						defaultIntensity = vignetteComponent.Properties.Intensity;
						defaultSmoothness = vignetteComponent.Properties.Smoothness;
						defaultRoundness = vignetteComponent.Properties.Roundness;
					}
					renderPipelineComponent = renderPipeline;
				}
				if( vignetteComponent != null)
				{
					int inputCount = playable.GetInputCount();
					Color blendedColor = Color.clear;
					Vector2 blendedCenter = Vector2.zero;
					float blendedIntensity = 0.0f;
					float blendedSmoothness = 0.0f;
					float blendedRoundness = 0.0f;
					float totalWeight = 0.0f;
					
					for( int i0 = 0; i0 < inputCount; ++i0)
					{
						float inputWeight = playable.GetInputWeight( i0);
						
						if( inputWeight > 0)
						{
							var playableInput = (ScriptPlayable<VignetteBehaviour>)playable.GetInput( i0);
							VignetteBehaviour input = playableInput.GetBehaviour();
							
							blendedColor += input.color * inputWeight;
							blendedCenter += input.center * inputWeight;
							blendedIntensity += input.intensity * inputWeight;
							blendedSmoothness += input.smoothness * inputWeight;
							blendedRoundness += input.roundness * inputWeight;
							totalWeight += inputWeight;
						}
					}
					float defaultWeight = 1.0f - totalWeight;
					if( defaultWeight > 0)
					{
						blendedColor += defaultColor * defaultWeight;
						blendedCenter += defaultCenter * defaultWeight;
						blendedIntensity += defaultIntensity * defaultWeight;
						blendedSmoothness += defaultSmoothness * defaultWeight;
						blendedRoundness += defaultRoundness * defaultWeight;
					}
					vignetteComponent.Properties.Color = blendedColor;
					vignetteComponent.Properties.Center = blendedCenter;
					vignetteComponent.Properties.Intensity = blendedIntensity;
					vignetteComponent.Properties.Smoothness = blendedSmoothness;
					vignetteComponent.Properties.Roundness = blendedRoundness;
				}
			}
		}
		public override void OnGraphStop( Playable playable)
		{
			if( vignetteComponent != null)
			{
				vignetteComponent.Properties.Color = defaultColor;
				vignetteComponent.Properties.Center = defaultCenter;
				vignetteComponent.Properties.Intensity = defaultIntensity;
				vignetteComponent.Properties.Smoothness = defaultSmoothness;
				vignetteComponent.Properties.Roundness = defaultRoundness;
				vignetteComponent = null;
			}
			renderPipelineComponent = null;
		}
		
		Color defaultColor;
		Vector2 defaultCenter;
		float defaultIntensity;
		float defaultSmoothness;
		float defaultRoundness;
		Vignette vignetteComponent;
		RenderPipeline renderPipelineComponent;
	}
}
