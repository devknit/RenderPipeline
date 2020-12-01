
using UnityEditor;
using UnityEngine;

namespace RenderPipeline.Editor
{
	[CanEditMultipleObjects]
	[CustomEditor( typeof( Bloom))]
	public sealed class BloomEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			SerializedProperty sharedSettings = serializedObject.FindProperty( "sharedSettings");
			SerializedProperty properties = serializedObject.FindProperty( "properties");
			SerializedProperty useSharedProperties = serializedObject.FindProperty( "useSharedProperties");
			
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
					EditorGUILayout.PropertyField( useSharedProperties, true);
					
					var sharedSettingsObject = new SerializedObject( sharedSettings.objectReferenceValue);
					sharedSettingsObject.Update();
					
					Color defaultBackgroundColor = GUI.backgroundColor;
					GUI.backgroundColor = new Color32( 194, 230, 237, 255);
					{
						OnPropertiesGUI( sharedSettingsObject.FindProperty( "properties"), (propertyName) =>
						{
							return propertyName.Equals( "enabled") != false;
						});
						if( useSharedProperties.boolValue != false)
						{
							OnPropertiesGUI( sharedSettingsObject.FindProperty( "properties"), (propertyName) =>
							{
								return propertyName.Equals( "enabled") == false;
							});
						}
					}
					GUI.backgroundColor = defaultBackgroundColor;
					sharedSettingsObject.ApplyModifiedProperties();
					
					if( useSharedProperties.boolValue == false)
					{
						OnPropertiesGUI( properties, (propertyName) =>
						{
							return propertyName.Equals( "enabled") == false;
						});
					}
				}
				else
				{
					OnPropertiesGUI( properties);
				}
				serializedObject.ApplyModifiedProperties();
			}
		}
		void OnPropertiesGUI( SerializedProperty properties, System.Func<string, bool> onVerify=null)
		{
			if( properties != null)
			{
				int depth = properties.depth + 1;
				
				while( properties.NextVisible( true) != false)
				{
					if( (onVerify?.Invoke( properties.name) ?? true) != false)
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
}
