
using UnityEngine;
using UnityEngine.Playables;

namespace RenderingPipeline
{
	[System.Serializable]
	public sealed class VignetteBehaviour : PlayableBehaviour
	{
		[SerializeField]
		public Color color = Color.black;
		[SerializeField]
		public Vector2 center = new Vector2( 0.5f, 0.5f);
		[SerializeField, Range( 0, 1)]
		public float intensity = 0.2f; 
		[SerializeField, Range( 0, 1)]
		public float smoothness = 1.0f;
		[SerializeField, Range( 0, 1)]
		public float roundness = 1.0f;
	}
}
