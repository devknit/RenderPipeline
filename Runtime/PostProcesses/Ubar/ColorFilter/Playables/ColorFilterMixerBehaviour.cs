
using UnityEngine;
using UnityEngine.Playables;

namespace RenderingPipeline
{
	public sealed class ColorFilterMixerBehaviour : PlayableBehaviour
	{
		public override void ProcessFrame( Playable playable, FrameData info, object playerData)
		{
			if( playerData is RenderPipeline renderPipeline)
			{
				if( colorFilterComponent == null)
				{
					colorFilterComponent = renderPipeline.FindPostProcess<ColorFilter>();
					if( colorFilterComponent != null)
					{
						defaultDot = colorFilterComponent.Properties.Dot;
						defaultMultiply = colorFilterComponent.Properties.Multiply;
						defaultAdd = colorFilterComponent.Properties.Add;
						defaultInvert = colorFilterComponent.Properties.Invert;
					}
				}
				if( colorFilterComponent != null)
				{
					int inputCount = playable.GetInputCount();
					Color blendedDot = Color.clear;
					Color blendedMultiply = Color.clear;
					Color blendedAdd = Color.clear;
					float blendedInvert = 0;
					float totalWeight = 0.0f;
					
					for( int i0 = 0; i0 < inputCount; ++i0)
					{
						float inputWeight = playable.GetInputWeight( i0);
						
						if( inputWeight > 0)
						{
							var playableInput = (ScriptPlayable<ColorFilterBehaviour>)playable.GetInput( i0);
							ColorFilterBehaviour input = playableInput.GetBehaviour();
							
							blendedDot += input.dot * inputWeight;
							blendedMultiply += input.multiply * inputWeight;
							blendedAdd += input.add * inputWeight;
							blendedInvert += input.invert * inputWeight;
							totalWeight += inputWeight;
						}
					}
					float defaultWeight = 1.0f - totalWeight;
					if( defaultWeight > 0)
					{
						blendedDot += defaultDot * defaultWeight;
						blendedMultiply += defaultMultiply * defaultWeight;
						blendedAdd += defaultAdd * defaultWeight;
						blendedInvert += defaultInvert * defaultWeight;
					}
					colorFilterComponent.Properties.Dot = blendedDot;
					colorFilterComponent.Properties.Multiply = blendedMultiply;
					colorFilterComponent.Properties.Add = blendedAdd;
					colorFilterComponent.Properties.Invert = blendedInvert;
				}
			}
		}
		public override void OnGraphStop( Playable playable)
		{
			if( colorFilterComponent != null)
			{
				if( leaveAsIs == false)
				{
					colorFilterComponent.Properties.Dot = defaultDot;
					colorFilterComponent.Properties.Multiply = defaultMultiply;
					colorFilterComponent.Properties.Add = defaultAdd;
					colorFilterComponent.Properties.Invert = defaultInvert;
				}
				colorFilterComponent = null;
			}
		}
		
		internal bool leaveAsIs;
		Color defaultDot;
		Color defaultMultiply;
		Color defaultAdd;
		float defaultInvert;
		ColorFilter colorFilterComponent;
	}
}
