
using UnityEngine;
using UnityEngine.Playables;

namespace RenderingPipeline
{
	public sealed class NoiseMixerBehaviour : PlayableBehaviour
	{
		public override void ProcessFrame( Playable playable, FrameData info, object playerData)
		{
			if( playerData is RenderPipeline renderPipeline)
			{
				if( noiseComponent == null)
				{
					noiseComponent = renderPipeline.FindPostProcess<Noise>();
					if( noiseComponent != null)
					{
						defaultColor = noiseComponent.Properties.Color;
						defaultSpeed = noiseComponent.Properties.Speed;
						defaultInterval = noiseComponent.Properties.Interval;
						defaultEdge0 = noiseComponent.Properties.Edge0;
						defaultEdge1 = noiseComponent.Properties.Edge1;
					}
				}
				if( noiseComponent != null)
				{
					int inputCount = playable.GetInputCount();
					Color blendedColor = Color.clear;
					float blendedSpeed = 0.0f;
					float blendedInterval = 0.0f;
					float blendedEdge0 = 0.0f;
					float blendedEdge1 = 0.0f;
					float totalWeight = 0.0f;
					
					for( int i0 = 0; i0 < inputCount; ++i0)
					{
						float inputWeight = playable.GetInputWeight( i0);
						
						if( inputWeight > 0)
						{
							var playableInput = (ScriptPlayable<NoiseBehaviour>)playable.GetInput( i0);
							NoiseBehaviour input = playableInput.GetBehaviour();
							
							blendedColor += input.color * inputWeight;
							blendedSpeed += input.speed * inputWeight;
							blendedInterval += input.interval * inputWeight;
							blendedEdge0 += input.edge0 * inputWeight;
							blendedEdge1 += input.edge1 * inputWeight;
							totalWeight += inputWeight;
						}
					}
					float defaultWeight = 1.0f - totalWeight;
					if( defaultWeight > 0)
					{
						blendedColor += defaultColor * defaultWeight;
						blendedSpeed += defaultSpeed * defaultWeight;
						blendedInterval += defaultInterval * defaultWeight;
						blendedEdge0 += defaultEdge0 * defaultWeight;
						blendedEdge1 += defaultEdge1 * defaultWeight;
					}
					noiseComponent.Properties.Color = blendedColor;
					noiseComponent.Properties.Speed = blendedSpeed;
					noiseComponent.Properties.Interval = blendedInterval;
					noiseComponent.Properties.Edge0 = blendedEdge0;
					noiseComponent.Properties.Edge1 = blendedEdge1;
				}
			}
		}
		public override void OnGraphStop( Playable playable)
		{
			if( noiseComponent != null)
			{
				if( leaveAsIs == false)
				{
					noiseComponent.Properties.Color = defaultColor;
					noiseComponent.Properties.Speed = defaultSpeed;
					noiseComponent.Properties.Interval = defaultInterval;
					noiseComponent.Properties.Edge0 = defaultEdge0;
					noiseComponent.Properties.Edge1 = defaultEdge1;
				}
				noiseComponent = null;
			}
		}
		
		internal bool leaveAsIs;
		Color defaultColor;
		float defaultSpeed;
		float defaultInterval;
		float defaultEdge0;
		float defaultEdge1;
		Noise noiseComponent;
	}
}
