
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	public interface IProperties
	{
		bool Enabled
		{
			get;
			set;
		}
		PostProcessEvent Phase
		{
			get;
		}
		void ClearCache();
	}
	public interface IGenericProperties : IProperties
	{
	}
	public interface IUbarProperties : IProperties
	{
		bool UpdateProperties( Material material, bool forcedDisable);
		bool UpdateUbarProperties( Material material, bool forcedDisable);
	}
}
