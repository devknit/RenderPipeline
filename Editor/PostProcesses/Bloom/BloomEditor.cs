
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
			serializedObject.Update();
			
			SerializedProperty enabled = serializedObject.FindProperty( "m_Enabled");
			if( enabled != null)
			{
				EditorGUILayout.PropertyField( enabled, false);
			}
			SerializedProperty iterator = serializedObject.GetIterator();
			int currentDepth = 0;
			
			while( iterator.NextVisible( true) != false)
			{
				if( iterator.editable == false
				||	currentDepth < iterator.depth)
				{
					continue;
				}
				if( iterator.propertyType == SerializedPropertyType.ObjectReference)
				{
					if( iterator.name == "m_Script" && iterator.type == "PPtr<MonoScript>")
					{
						continue;
					}
					else if( iterator.objectReferenceValue is Shader)
					{
						continue;
					}
				}
				
				EditorGUI.indentLevel = iterator.depth;
				EditorGUILayout.PropertyField( iterator, false);
				
				if( iterator.isExpanded != false)
	            {
	                currentDepth = iterator.depth + 1;
	            }
	            else
	            {
	                currentDepth = iterator.depth;
	            }
			}
			serializedObject.ApplyModifiedProperties();
		}
	}
}
