
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderingPipeline
{
	public static class ShaderProperty
	{
		public static readonly int MainTex = Shader.PropertyToID( "_MainTex");
		public static readonly int Color = Shader.PropertyToID( "_Color");
		public static readonly int OverrideDepthTexture = Shader.PropertyToID( "_OverrideDepthTexture");
		public static readonly int CameraDepthTexture = Shader.PropertyToID( "_CameraDepthTexture");
		public static readonly int StencilReference = Shader.PropertyToID( "_StencilRef");
		public static readonly int StencilReadMask = Shader.PropertyToID( "_StencilReadMask");
		public static readonly int StencilCompare = Shader.PropertyToID( "_StencilComp");
	}
}
