
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderingPipeline
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
		bool UpdateProperties( RenderPipeline pipeline, Material material);
		bool UpdateUbarProperties( RenderPipeline pipeline, Material material, bool forcedDisable);
	}
}
