
using UnityEditor;
using UnityEngine;

namespace RenderPipeline.Editor
{
	[CanEditMultipleObjects]
	[CustomEditor( typeof( DepthOfField.DepthOfField))]
	public sealed class DepthOfFieldEditor : PostProcessEditor
	{
	}
}
