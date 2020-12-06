
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	public abstract class Settings<T> : ScriptableObject where T : Properties
	{
		[SerializeField]
		public T properties = default;
	}
	public interface Properties
	{
		bool Enabled
		{
			get;
			set;
		}
		void ClearCache();
	}
	public interface IGenericProperties : Properties
	{
		PostProcessEvent Phase
		{
			get;
		}
	}
	public interface IUbarProperties : Properties
	{
		bool UpdateProperties( Material material, bool forcedDisable);
	}
}
