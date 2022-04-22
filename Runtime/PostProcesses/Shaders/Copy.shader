Shader "Hidden/RenderPipeline/Copy"
{
	SubShader
	{
		Cull Off ZWrite Off ZTest Always
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "Copy.cginc"
			ENDCG
		}
	}
}
