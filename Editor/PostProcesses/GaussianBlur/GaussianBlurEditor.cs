
using UnityEditor;
using UnityEngine;

namespace RenderingPipeline.Editor
{
	[CanEditMultipleObjects]
	[CustomEditor( typeof( GaussianBlur))]
	public sealed class GaussianBlurEditor : PostProcessEditor
	{
	}
}
