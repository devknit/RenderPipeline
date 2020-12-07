
using UnityEngine;
using UnityEngine.Playables;

namespace RenderPipeline
{
	[System.Serializable]
	public sealed class NoiseBehaviour : PlayableBehaviour
	{
		[SerializeField]
		public Color color = new Color( 0.5f, 0.7f, 0.8f);
		[SerializeField]
		public float speed = 1.0f;
		[SerializeField]
		public float interval = 5.0f; 
		[SerializeField, Range( 0, 1)]
		public float edge0 = 0.0f;
		[SerializeField, Range( 0, 1)]
		public float edge1 = 1.0f;
	}
}
