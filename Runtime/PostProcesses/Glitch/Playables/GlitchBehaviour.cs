
using UnityEngine;
using UnityEngine.Playables;

namespace RenderingPipeline
{
	[System.Serializable]
	public sealed class GlitchBehaviour : PlayableBehaviour
	{
		[SerializeField, Range( 0.0f, 0.9999f)]
		public float intensity = 0.025f;
		[SerializeField]
		public float timeScale = 1.0f;
		[SerializeField]
		public Vector2 slice = new Vector2( 1, 1000);
		[SerializeField]
		public Vector2 volume = new Vector2( 10, 1);
		[SerializeField]
		public Vector3 chromaticAberration = new Vector3( 5, 3, 1);
	}
}
