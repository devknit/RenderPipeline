
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace RenderingPipeline
{
	public sealed class GlitchMixerBehaviour : PlayableBehaviour
	{
		public override void ProcessFrame( Playable playable, FrameData info, object playerData)
		{
			if( playerData is RenderPipeline renderPipeline)
			{
				if( glitchComponent == null)
				{
					glitchComponent = renderPipeline.FindPostProcess<Glitch>();
					if( glitchComponent != null)
					{
						defaultIntensity = glitchComponent.Properties.Intensity;
						defaultTimeScale = glitchComponent.Properties.TimeScale;
						defaultSlice = glitchComponent.Properties.Slice;
						defaultVolume = glitchComponent.Properties.Volume;
						defaultChromaticAberration = glitchComponent.Properties.ChromaticAberration;
					}
				}
				if( glitchComponent != null)
				{
					int inputCount = playable.GetInputCount();
					float blendedIntensity = 0.0f;
					float blendedTimeScale = 0.0f;
					Vector2 blendedSlice = Vector2.zero;
					Vector2 blendedVolume = Vector2.zero;
					Vector3 blendedChromaticAberration = Vector3.zero;
					float totalWeight = 0.0f;
					
					for( int i0 = 0; i0 < inputCount; ++i0)
					{
						float inputWeight = playable.GetInputWeight( i0);
						
						if( inputWeight > 0)
						{
							var playableInput = (ScriptPlayable<GlitchBehaviour>)playable.GetInput( i0);
							GlitchBehaviour input = playableInput.GetBehaviour();
							
							blendedIntensity += input.intensity * inputWeight;
							blendedTimeScale += input.timeScale * inputWeight;
							blendedSlice += input.slice * inputWeight;
							blendedVolume += input.volume * inputWeight;
							blendedChromaticAberration += input.chromaticAberration * inputWeight;
							totalWeight += inputWeight;
						}
					}
					float defaultWeight = 1.0f - totalWeight;
					if( defaultWeight > 0)
					{
						blendedIntensity += defaultIntensity * defaultWeight;
						blendedTimeScale += defaultTimeScale * defaultWeight;
						blendedSlice += defaultSlice * defaultWeight;
						blendedVolume += defaultVolume * defaultWeight;
						blendedChromaticAberration += defaultChromaticAberration * defaultWeight;
					}
					glitchComponent.Properties.Intensity = blendedIntensity;
					glitchComponent.Properties.TimeScale = blendedTimeScale;
					glitchComponent.Properties.Slice = blendedSlice;
					glitchComponent.Properties.Volume = blendedVolume;
					glitchComponent.Properties.ChromaticAberration = blendedChromaticAberration;
				}
			}
		}
		public override void OnGraphStop( Playable playable)
		{
			if( glitchComponent != null)
			{
				if( leaveAsIs == false)
				{
					glitchComponent.Properties.Intensity = defaultIntensity;
					glitchComponent.Properties.TimeScale = defaultTimeScale;
					glitchComponent.Properties.Slice = defaultSlice;
					glitchComponent.Properties.Volume = defaultVolume;
					glitchComponent.Properties.ChromaticAberration = defaultChromaticAberration;
				}
				glitchComponent = null;
			}
		}
		
		internal bool leaveAsIs;
		float defaultIntensity;
		float defaultTimeScale;
		Vector2 defaultSlice;
		Vector2 defaultVolume;
		Vector3 defaultChromaticAberration;
		Glitch glitchComponent;
	}
}
