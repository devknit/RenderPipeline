
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	public static class ShaderProperty
	{
		public static readonly int MainTex = Shader.PropertyToID( "_MainTex");
		public static readonly int Color = Shader.PropertyToID( "_Color");
		public static readonly int OverrideDepthTexture = Shader.PropertyToID( "_OverrideDepthTexture");
		public static readonly int CameraDepthTexture = Shader.PropertyToID( "_CameraDepthTexture");
	}
}
