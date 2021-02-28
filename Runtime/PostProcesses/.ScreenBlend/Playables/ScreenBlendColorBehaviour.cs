
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace RenderingPipeline
{
	[System.Serializable]
	public sealed class ScreenBlendColorBehaviour : PlayableBehaviour
	{
		public Color color = Color.clear;
	}
}
