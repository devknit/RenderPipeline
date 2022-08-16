
using UnityEngine;
using UnityEngine.Playables;

namespace RenderingPipeline
{
	[System.Serializable]
	public sealed class SpeedLineBehaviour : PlayableBehaviour
	{
		[SerializeField]
		internal SpeedLineType type = SpeedLineType.Radial;
		[SerializeField]
		internal Color color = Color.black;
		[SerializeField]
		internal Vector2 center = new Vector2( 0.5f, 0.5f);
		[SerializeField]
		internal Vector2 axisMask = Vector2.one;
		[SerializeField]
		internal float tiling = 200;
		[SerializeField]
		internal float sparse = 3.0f;
		[SerializeField, Range( 0, 1)]
		internal float remap = 0.5f;
		[SerializeField, Range( 0, 10)]
		internal float radialScale = 0.5f;
		[SerializeField]
		internal float smoothWidth = 0.45f;
		[SerializeField]
		internal float smoothBorder = 0.3f;
		[SerializeField]
		internal float animationSpeed = 3.0f;
		
		[SerializeField, HideInInspector]
		internal Transform transform;
		[SerializeField]
		internal Remap smoothBorderDistanceRemap = new Remap();
		
		[System.Serializable]
		internal sealed class Remap
		{
			internal float ToRemap( float value)
			{
				return outputBorderMin + (value - inputDistanceMin) * (outputBorderMax - outputBorderMin) / (inputDistanceMax - inputDistanceMin);
			}
			[SerializeField]
			float inputDistanceMin = 1.0f;
			[SerializeField]
			float inputDistanceMax = 15.0f;
			[SerializeField]
			float outputBorderMin = 0.5f;
			[SerializeField]
			float outputBorderMax = -0.45f;
		}
	}
}
