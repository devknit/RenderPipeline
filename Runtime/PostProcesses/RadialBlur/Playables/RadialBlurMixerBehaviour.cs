
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace RenderingPipeline
{
	public sealed class RadialBlurMixerBehaviour : PlayableBehaviour
	{
		public override void ProcessFrame( Playable playable, FrameData info, object playerData)
		{
			if( playerData is RenderPipeline renderPipeline)
			{
				if( radialBlurComponent == null)
				{
					radialBlurComponent = renderPipeline.FindPostProcess<RadialBlur>();
					if( radialBlurComponent != null)
					{
						defaultSamples = radialBlurComponent.Properties.Samples;
						defaultIntensity = radialBlurComponent.Properties.Intensity;
						defaultRadius = radialBlurComponent.Properties.Radius;
						defaultCenter = radialBlurComponent.Properties.Center;
					}
				}
				if( radialBlurComponent != null)
				{
					int inputCount = playable.GetInputCount();
					float blendedSamples = 0.0f;
					float blendedIntensity = 0.0f;
					float blendedRadius = 0.0f;
					Vector2 blendedCenter = Vector2.zero;
					float totalWeight = 0.0f;
					
					for( int i0 = 0; i0 < inputCount; ++i0)
					{
						float inputWeight = playable.GetInputWeight( i0);
						
						if( inputWeight > 0)
						{
							var playableInput = (ScriptPlayable<RadialBlurBehaviour>)playable.GetInput( i0);
							RadialBlurBehaviour input = playableInput.GetBehaviour();
							
							blendedSamples += (float)input.samples * inputWeight;
							blendedIntensity += input.intensity * inputWeight;
							blendedRadius += input.radius * inputWeight;
							blendedCenter += input.center * inputWeight;
							totalWeight += inputWeight;
						}
					}
					float defaultWeight = 1.0f - totalWeight;
					if( defaultWeight > 0)
					{
						blendedSamples += (float)defaultSamples * defaultWeight;
						blendedIntensity += defaultIntensity * defaultWeight;
						blendedRadius += defaultRadius * defaultWeight;
						blendedCenter += defaultCenter * defaultWeight;
					}
					radialBlurComponent.Properties.Samples = (int)blendedSamples;
					radialBlurComponent.Properties.Intensity = blendedIntensity;
					radialBlurComponent.Properties.Radius = blendedRadius;
					radialBlurComponent.Properties.Center = blendedCenter;
				}
			}
		}
		public override void OnGraphStop( Playable playable)
		{
			if( radialBlurComponent != null)
			{
				if( leaveAsIs == false)
				{
					radialBlurComponent.Properties.Samples = defaultSamples;
					radialBlurComponent.Properties.Intensity = defaultIntensity;
					radialBlurComponent.Properties.Radius = defaultRadius;
					radialBlurComponent.Properties.Center = defaultCenter;
				}
				radialBlurComponent = null;
			}
		}
		
		internal bool leaveAsIs;
		int defaultSamples;
		float defaultIntensity;
		float defaultRadius;
		Vector2 defaultCenter;
		RadialBlur radialBlurComponent;
	}
}
