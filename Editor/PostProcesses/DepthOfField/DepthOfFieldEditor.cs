
using UnityEditor;
using UnityEngine;

namespace RenderingPipeline.Editor
{
	[CanEditMultipleObjects]
	[CustomEditor( typeof( DepthOfField.DepthOfField))]
	public sealed class DepthOfFieldEditor : PostProcessEditor
	{
	}
}
