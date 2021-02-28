
using UnityEditor;
using UnityEngine;

namespace RenderingPipeline.Editor
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
					var resolutionScale = serializedObject.FindProperty( "resolutionScale");
					var overrideCameraDepthTexture = serializedObject.FindProperty( "overrideCameraDepthTexture");
					
					if( resolutionScale != null || overrideCameraDepthTexture != null)
					{
						EditorGUI.indentLevel++;
						
						if( resolutionScale != null)
						{
							EditorGUILayout.PropertyField( resolutionScale, true);
						}
						if( overrideCameraDepthTexture != null)
						{
							EditorGUILayout.PropertyField( overrideCameraDepthTexture, true);
						}
						EditorGUI.indentLevel--;
					}
				}
				EditorGUILayout.PropertyField( postProcessesTarget, true);
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}
