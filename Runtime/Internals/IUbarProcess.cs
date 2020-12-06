
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	public interface IUbarProcess : IPostProcess
	{
		bool Enabled
		{
			get;
		}
		bool HasIndependent( ref bool rebuild);
		bool Independent();
		bool PreProcess();
		IUbarProperties GetProperties();
	}
}
