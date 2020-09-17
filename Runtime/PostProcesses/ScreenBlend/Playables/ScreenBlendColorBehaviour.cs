
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace RenderPipeline
{
	[System.Serializable]
	public sealed class ScreenBlendColorBehaviour : PlayableBehaviour
	{
		public Color color = Color.clear;
	}
}
