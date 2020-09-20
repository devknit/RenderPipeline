
using UnityEditor;
using UnityEngine;

namespace RenderPipeline.Editor
{
	[CanEditMultipleObjects]
	[CustomEditor( typeof( RenderPipeline))]
	public sealed class RenderPipelineEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			var defaultDepthTextureMode = serializedObject.FindProperty( "defaultDepthTextureMode");
			var overrideTargetBuffers = serializedObject.FindProperty( "overrideTargetBuffers");
			var postProcessesTarget = serializedObject.FindProperty( "postProcessesTarget");
			
			if( defaultDepthTextureMode == null
			||	overrideTargetBuffers == null
			||	postProcessesTarget == null)
			{
				base.OnInspectorGUI();
			}
			else
			{
				serializedObject.Update();
				
				EditorGUILayout.PropertyField( defaultDepthTextureMode, true);
				EditorGUILayout.PropertyField( overrideTargetBuffers, true);
				
				if( overrideTargetBuffers.boolValue != false)
				{
					var overrideCameraDepthTexture = serializedObject.FindProperty( "overrideCameraDepthTexture");
					
					if( overrideCameraDepthTexture != null)
					{
						EditorGUI.indentLevel++;
						EditorGUILayout.PropertyField( overrideCameraDepthTexture, true);
						EditorGUI.indentLevel--;
					}
				}
				EditorGUILayout.PropertyField( postProcessesTarget, true);
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}
