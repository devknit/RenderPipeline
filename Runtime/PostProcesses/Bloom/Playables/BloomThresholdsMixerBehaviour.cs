﻿
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace RenderPipeline
{
	public sealed class BloomThresholdsMixerBehaviour : PlayableBehaviour
	{
		public override void ProcessFrame( Playable playable, FrameData info, object playerData)
	    {
			if( playerData is RenderPipeline renderPipeline)
			{
				if( renderPipeline.Bloom != null)
				{
					if( cachedRenderPipeline == null)
					{
						defaultThresholds = renderPipeline.Bloom.Properties.Thresholds;
						cachedRenderPipeline = renderPipeline;
					}
					int inputCount = playable.GetInputCount();
					float thresholds = 0.0f;
					float totalWeight = 0.0f;
					
					for( int i0 = 0; i0 < inputCount; ++i0)
			        {
						var playableInput = (ScriptPlayable<BloomThresholdsBehaviour>)playable.GetInput( i0);
			            BloomThresholdsBehaviour input = playableInput.GetBehaviour();
			            float inputWeight = playable.GetInputWeight( i0);

						float normalisedTime = (float)(playableInput.GetTime() / playableInput.GetDuration());
			            thresholds += inputWeight * (input.thresholds?.Evaluate( normalisedTime) ?? 1.0f);
			            totalWeight += inputWeight;
			        }
			        thresholds += defaultThresholds * (1.0f - totalWeight);
			        renderPipeline.Bloom.Properties.Thresholds = thresholds;
			    }
		    }
		}
		public override void OnGraphStop( Playable playable)
	    {
			if( cachedRenderPipeline != null)
			{
				cachedRenderPipeline.Bloom.Properties.Thresholds = defaultThresholds;
			}
	    }
	    
	    RenderPipeline cachedRenderPipeline;
	    float defaultThresholds = 1.0f;
	}
}