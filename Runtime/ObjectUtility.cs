
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	public static class ObjectUtility
	{
		public static void Release<T>( T obj) where T : Object
		{
		#if UNITY_EDITOR
			if( UnityEditor.EditorApplication.isPlaying == false)
			{
				Object.DestroyImmediate( obj);
			}
			else
		#endif
			{
				Object.Destroy( obj);
			}
		}
		public static bool IsMissing<T>( T obj) where T : class
		{
			if( obj == null)
			{
				return object.ReferenceEquals( obj, null);
			}
			return false;
		}
	}
}
