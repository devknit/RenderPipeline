
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	public abstract class Settings : ScriptableObject
	{
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
	internal interface IUbarProperties
	{
		bool Enabled
		{
			get;
			set;
		}
		void ClearCache();
		bool UpdateProperties( Material material, bool forcedDisable);
	}
}
