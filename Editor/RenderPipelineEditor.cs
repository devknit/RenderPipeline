
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
			var defaultColorTargetFormat = serializedObject.FindProperty( "defaultColorTargetFormat");
			var defaultDepthTextureMode = serializedObject.FindProperty( "defaultDepthTextureMode");
			var overrideTargetBuffers = serializedObject.FindProperty( "overrideTargetBuffers");
			var postProcessesTarget = serializedObject.FindProperty( "postProcessesTarget");
			
			if( defaultDepthTextureMode == null
			||	overrideTargetBuffers == null
			||	postProcessesTarget == null
			||	defaultColorTargetFormat == null)
			{
				base.OnInspectorGUI();
			}
			else
			{
				serializedObject.Update();
				
				EditorGUILayout.PropertyField( defaultColorTargetFormat, true);
				EditorGUILayout.PropertyField( defaultDepthTextureMode, true);
				EditorGUILayout.PropertyField( overrideTargetBuffers, true);
				
				if( overrideTargetBuffers.boolValue != false)
				{
					var overrideTargetFormat = serializedObject.FindProperty( "overrideTargetFormat");
					var resolutionScale = serializedObject.FindProperty( "resolutionScale");
					var overrideCameraDepthTexture = serializedObject.FindProperty( "overrideCameraDepthTexture");
					var overrideTargetEvent = serializedObject.FindProperty( "overrideTargetEvent");
					
					if( overrideTargetFormat != null
					||	resolutionScale != null
					||	overrideCameraDepthTexture != null
					||	overrideTargetEvent != null)
					{
						EditorGUI.indentLevel++;
						
						if( overrideTargetFormat != null)
						{
							EditorGUILayout.PropertyField( overrideTargetFormat, true);
						}
						if( resolutionScale != null)
						{
							EditorGUILayout.PropertyField( resolutionScale, true);
						}
						if( overrideCameraDepthTexture != null)
						{
							EditorGUILayout.PropertyField( overrideCameraDepthTexture, true);
						}
						if( overrideTargetEvent != null)
						{
							EditorGUILayout.PropertyField( overrideTargetEvent, true);
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
