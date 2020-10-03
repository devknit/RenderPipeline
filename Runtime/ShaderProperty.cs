
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	public static class ShaderProperty
	{
		public static readonly int MainTex = Shader.PropertyToID( "_MainTex");
		public static readonly int Color = Shader.PropertyToID( "_Color");
	}
}
