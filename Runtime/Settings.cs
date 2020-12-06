
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	public abstract class Settings<T> : ScriptableObject where T : IProperties
	{
		[SerializeField]
		public T properties = default;
	}
	public interface IProperties
	{
		bool Enabled
		{
			get;
			set;
		}
		void ClearCache();
	}
	public interface IGenericProperties : IProperties
	{
		PostProcessEvent Phase
		{
			get;
		}
	}
	public interface IUbarProperties : IProperties
	{
		bool UpdateProperties( Material material, bool forcedDisable);
	}
}
