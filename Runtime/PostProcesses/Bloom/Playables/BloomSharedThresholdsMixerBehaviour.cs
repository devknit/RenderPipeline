
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace RenderPipeline
{
	public sealed class BloomSharedThresholdsMixerBehaviour : PlayableBehaviour
	{
		public override void ProcessFrame( Playable playable, FrameData info, object playerData)
	    {
			int inputCount = playable.GetInputCount();
			float thresholds = 0.0f;
			float totalWeight = 0.0f;
			
			for( int i0 = 0; i0 < inputCount; ++i0)
	        {
				var playableInput = (ScriptPlayable<BloomSharedThresholdsBehaviour>)playable.GetInput( i0);
	            BloomSharedThresholdsBehaviour input = playableInput.GetBehaviour();
	            float inputWeight = playable.GetInputWeight( i0);

				float normalisedTime = (float)(playableInput.GetTime() / playableInput.GetDuration());
	            thresholds += inputWeight * (input.thresholds?.Evaluate( normalisedTime) ?? 1.0f);
	            totalWeight += inputWeight;
	        }
	        thresholds += defaultThresholds * (1.0f - totalWeight);
	        BloomSettings.Instance.properties.Thresholds = thresholds;
		}
		public override void OnGraphStart( Playable playable)
		{
			defaultThresholds = BloomSettings.Instance.properties.Thresholds;
		}
		public override void OnGraphStop( Playable playable)
	    {
			BloomSettings.Instance.properties.Thresholds = defaultThresholds;
	    }
	    float defaultThresholds = 1.0f;
	}
}
