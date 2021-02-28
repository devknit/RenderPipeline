
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderingPipeline
{
	public abstract class Settings<T> : ScriptableObject where T : IProperties
	{
		[SerializeField]
		public T properties = default;
	}
}
