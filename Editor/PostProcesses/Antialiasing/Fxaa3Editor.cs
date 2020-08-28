﻿
using UnityEditor;
using UnityEngine;

namespace RenderPipeline.Editor
{
	[CanEditMultipleObjects]
	[CustomEditor( typeof( Fxaa3))]
	public sealed class Fxaa3Editor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			SerializedProperty sharedSettings = serializedObject.FindProperty( "sharedSettings");
			SerializedProperty properties = serializedObject.FindProperty( "properties");
			
			if( sharedSettings == null || properties == null)
			{
				base.OnInspectorGUI();
			}
			else
			{
				serializedObject.Update();
				EditorGUILayout.PropertyField( sharedSettings, true);
				
				if( sharedSettings.objectReferenceValue == null)
				{
					EditorGUILayout.PropertyField( properties, true);
				}
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}
