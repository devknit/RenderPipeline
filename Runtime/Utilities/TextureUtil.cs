
using UnityEngine;

namespace RenderPipeline
{
	public static class TextureUtil
	{
		public static RenderTextureFormat DefaultHDR
        {
            get
            {
		#if UNITY_ANDROID || UNITY_IPHONE || UNITY_TVOS || UNITY_SWITCH || UNITY_EDITOR
                RenderTextureFormat format = RenderTextureFormat.RGB111110Float;
			#if UNITY_EDITOR
                var target = UnityEditor.EditorUserBuildSettings.activeBuildTarget;
                if( target != UnityEditor.BuildTarget.Android
                &&	target != UnityEditor.BuildTarget.iOS
                &&	target != UnityEditor.BuildTarget.tvOS
                &&	target != UnityEditor.BuildTarget.Switch)
                {
                    return RenderTextureFormat.DefaultHDR;
                }
			#endif
                if( SystemInfo.SupportsRenderTextureFormat( format) != false)
                {
                    return format;
                }
		#endif
                return RenderTextureFormat.DefaultHDR;
            }
        }
		public static int ToPow2RoundUp( int x)
		{
			if( x == 0)
			{
				return 0;
			}
			return MakeMSB( x - 1) + 1;
		}
		public static int MakeMSB( int x)
		{
			x |= x >> 1;
			x |= x >> 2;
			x |= x >> 4;
			x |= x >> 8;
			x |= x >> 16;
			return x;
		}
	}
}
