
using UnityEngine;
using UnityEngine.Playables;

namespace RenderingPipeline
{
	[System.Serializable]
	public sealed class SpeedLineBehaviour : PlayableBehaviour
	{
		[SerializeField]
		internal Color color = Color.black;
		[SerializeField]
		internal Vector2 center = new Vector2( 0.5f, 0.5f);
		[SerializeField]
		internal Vector2 axisVolume = Vector2.one;
		[SerializeField]
		internal float tiling = 200;
		[SerializeField, Range( 0, 10)]
		internal float radialScale = 0.1f;
		[SerializeField]
		internal float smoothWidth = 0.3f;
		[SerializeField]
		internal float smoothBorder = 0.3f;
		[SerializeField]
		internal float animationSpeed = 3;
		[Space]
		[SerializeField, HideInInspector]
		internal Transform transform;
		[SerializeField]
		internal Remap smoothBorderDistanceRemap = new Remap();
		
		[System.Serializable]
		internal sealed class Remap
		{
			internal float ToRemap( float value)
			{
				return outputMin + (value - inputMin) * (outputMax - outputMin) / (inputMax - inputMin);
			}
			[SerializeField]
			float inputMin = 2.0f;
			[SerializeField]
			float inputMax = 8.0f;
			[SerializeField, Range( 0, 1)]
			float outputMin = 0.5f;
			[SerializeField, Range( 0, 1)]
			float outputMax = 0.8f;
		}
	}
}
