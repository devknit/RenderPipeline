
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
				
				if( sharedSettings.objectReferenceValue != null)
				{
					var sharedSettingsObject = new SerializedObject( sharedSettings.objectReferenceValue);
					sharedSettingsObject.Update();
					OnPropertiesGUI( sharedSettingsObject.FindProperty( "properties"));
					sharedSettingsObject.ApplyModifiedProperties();
				}
				else
				{
					OnPropertiesGUI( properties);
				}
				serializedObject.ApplyModifiedProperties();
			}
		}
		void OnPropertiesGUI( SerializedProperty properties)
		{
			if( properties != null)
			{
				int depth = properties.depth + 1;
				
				while( properties.NextVisible( true) != false)
				{
					if( properties.depth == depth)
					{
						EditorGUILayout.PropertyField( properties, true);
					}
				}
			}
		}
	}
}
