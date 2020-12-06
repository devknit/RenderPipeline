﻿
using UnityEngine;
using UnityEngine.Playables;

namespace RenderPipeline
{
	[System.Serializable]
	public sealed class RadialBlurBehaviour : PlayableBehaviour
	{
		[SerializeField, Range( 4, 32)]
		public int samples = 16;
		[SerializeField, Range( 0, 1)]
		public float intensity = 1.0f;
		[SerializeField, Range( 0, 50)]
		public float radius = 0.1f;
		[SerializeField]
		public Vector2 center = new Vector2( 0.5f, 0.5f);
	}
}
