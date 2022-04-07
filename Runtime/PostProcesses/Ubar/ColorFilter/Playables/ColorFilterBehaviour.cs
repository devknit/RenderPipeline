
using UnityEngine;
using UnityEngine.Playables;

namespace RenderingPipeline
{
	[System.Serializable]
	public sealed class ColorFilterBehaviour : PlayableBehaviour
	{
		[SerializeField, ColorUsage( true, true)]
		public Color dot = ColorFilterProperties.kMonochromeDot;
		[SerializeField, ColorUsage( true, true)]
		public Color multiply = ColorFilterProperties.kSepiaMultiply;
		[SerializeField, ColorUsage( true, true)]
		public Color add = Color.clear;
		[SerializeField, Range( -5, 5)]
		public float contrast = 1;
	}
}
