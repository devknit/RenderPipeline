
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace RenderingPipeline
{
	public sealed class SpeedLineMixerBehaviour : PlayableBehaviour
	{
		public override void ProcessFrame( Playable playable, FrameData info, object playerData)
		{
			if( playerData is RenderPipeline renderPipeline)
			{
				if( baindedComponent == null)
				{
					baindedComponent = renderPipeline.FindPostProcess<SpeedLine>();
					if( baindedComponent != null)
					{
						defaultType = baindedComponent.Properties.Type;
						defaultColor = baindedComponent.Properties.Color;
						defaultCenter = baindedComponent.Properties.Center;
						defaultAxisMask = baindedComponent.Properties.AxisMask;
						defaultTiling = baindedComponent.Properties.Tiling;
						defaultSparse = baindedComponent.Properties.Sparse;
						defaultRemap = baindedComponent.Properties.Remap;
						defaultRadialScale = baindedComponent.Properties.RadialScale;
						defaultSmoothWidth = baindedComponent.Properties.SmoothWidth;
						defaultSmoothBorder = baindedComponent.Properties.SmoothBorder;
						defaultAnimationSpeed = baindedComponent.Properties.AnimationSpeed;
					}
				}
				if( baindedComponent != null)
				{
					int inputCount = playable.GetInputCount();
					Color blendedColor = Color.clear;
					Vector2 blendedCenter = Vector2.zero;
					Vector2 blendedAxisMask = Vector2.zero;
					float blendedTiling = 0;
					float blendedSparse = 0;
					float blendedRemap = 0;
					float blendedRadialScale = 0;
					float blendedSmoothWidth = 0;
					float blendedSmoothBorder = 0;
					float blendedAnimationSpeed = 0;
					SpeedLineBehaviour positioning = null;
					SpeedLineType? lastType = null;
					float totalWeight = 0.0f;
					
					for( int i0 = 0; i0 < inputCount; ++i0)
					{
						float inputWeight = playable.GetInputWeight( i0);
						
						if( inputWeight > 0)
						{
							var playableInput = (ScriptPlayable<SpeedLineBehaviour>)playable.GetInput( i0);
							SpeedLineBehaviour input = playableInput.GetBehaviour();
							
							blendedColor += input.color * inputWeight;
							blendedCenter += input.center * inputWeight;
							blendedAxisMask += input.axisMask * inputWeight;
							blendedTiling += input.tiling * inputWeight;
							blendedSparse += input.sparse * inputWeight;
							blendedRemap += input.remap * inputWeight;
							blendedRadialScale += input.radialScale * inputWeight;
							blendedSmoothWidth += input.smoothWidth * inputWeight;
							blendedSmoothBorder += input.smoothBorder * inputWeight;
							blendedAnimationSpeed += input.animationSpeed * inputWeight;
							totalWeight += inputWeight;
							
							if( input.transform != null)
							{
								positioning = input;
							}
							lastType = input.type;
						}
					}
					float defaultWeight = 1.0f - totalWeight;
					if( defaultWeight > 0)
					{
						blendedColor += defaultColor * defaultWeight;
						blendedCenter += defaultCenter * defaultWeight;
						blendedAxisMask += defaultAxisMask * defaultWeight;
						blendedTiling += defaultTiling * defaultWeight;
						blendedSparse += defaultSparse * defaultWeight;
						blendedRemap += defaultRemap * defaultWeight;
						blendedRadialScale += defaultRadialScale * defaultWeight;
						blendedSmoothWidth += defaultSmoothWidth * defaultWeight;
						blendedSmoothBorder += defaultSmoothBorder * defaultWeight;
						blendedAnimationSpeed += defaultAnimationSpeed * defaultWeight;
					}
					if( lastType.HasValue != false)
					{
						baindedComponent.Properties.Type = lastType.Value;
					}
					baindedComponent.Properties.Color = blendedColor;
					baindedComponent.Properties.AxisMask = blendedAxisMask;
					baindedComponent.Properties.Tiling = blendedTiling;
					baindedComponent.Properties.Sparse = blendedSparse;
					baindedComponent.Properties.Remap = blendedRemap;
					baindedComponent.Properties.RadialScale = blendedRadialScale;
					
					if( positioning != null && renderPipeline.CacheCamera != null)
					{
						Vector3 targetPosition = positioning.transform.position;
						Vector3 cameraPosition = renderPipeline.CacheCamera.transform.position;
						
						baindedComponent.Properties.Center = renderPipeline.CacheCamera.WorldToViewportPoint( targetPosition);
						float distance = Vector3.Distance( targetPosition, cameraPosition);
						baindedComponent.Properties.SmoothBorder = positioning.smoothBorderDistanceRemap.ToRemap( distance);
					}
					else
					{
						baindedComponent.Properties.Center = blendedCenter;
						baindedComponent.Properties.SmoothBorder = blendedSmoothBorder;
					}
					baindedComponent.Properties.SmoothWidth = blendedSmoothWidth;
					baindedComponent.Properties.AnimationSpeed = blendedAnimationSpeed;
				}
			}
		}
		public override void OnGraphStop( Playable playable)
		{
			if( baindedComponent != null)
			{
				if( leaveAsIs == false)
				{
					baindedComponent.Properties.Type = defaultType;
					baindedComponent.Properties.Color = defaultColor;
					baindedComponent.Properties.Center = defaultCenter;
					baindedComponent.Properties.AxisMask = defaultAxisMask;
					baindedComponent.Properties.Tiling = defaultTiling;
					baindedComponent.Properties.Sparse = defaultSparse;
					baindedComponent.Properties.Remap = defaultRemap;
					baindedComponent.Properties.RadialScale = defaultRadialScale;
					baindedComponent.Properties.SmoothWidth = defaultSmoothWidth;
					baindedComponent.Properties.SmoothBorder = defaultSmoothBorder;
					baindedComponent.Properties.AnimationSpeed = defaultAnimationSpeed;
				}
				baindedComponent = null;
			}
		}
		
		internal bool leaveAsIs;
		SpeedLine baindedComponent;
		SpeedLineType defaultType;
		Color defaultColor;
		Vector2 defaultCenter;
		Vector2 defaultAxisMask;
		float defaultTiling;
		float defaultSparse;
		float defaultRemap;
		float defaultRadialScale;
		float defaultSmoothWidth;
		float defaultSmoothBorder;
		float defaultAnimationSpeed;
	}
}
