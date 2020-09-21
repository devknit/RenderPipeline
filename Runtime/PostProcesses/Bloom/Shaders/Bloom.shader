Shader "Hidden/RenderPipeline/Bloom"
{
	SubShader
	{
		Cull Off
		ZWrite Off
		ZTest Always
			
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_local _ LDR
			#include "BrightnessExtraction.cginc"
			ENDCG
		}
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "GaussianBlur.cginc"
			ENDCG
		}
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_local _ COMBINE_SAMPLE1
			#pragma multi_compile_local _ COMBINE_SAMPLE2
			#pragma multi_compile_local _ COMBINE_SAMPLE4
			#include "Combine.cginc"
			ENDCG
		}
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_local _ COMPOSITION_SAMPLE1
			#pragma multi_compile_local _ COMPOSITION_SAMPLE2
			#pragma multi_compile_local _ COMPOSITION_SAMPLE4
			#pragma multi_compile_local _ COMPOSITION_COMBINED
			#include "Composition.cginc"
			ENDCG
		}
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragMRT
			#pragma multi_compile_local _ COMPOSITION_SAMPLE1
			#pragma multi_compile_local _ COMPOSITION_SAMPLE2
			#pragma multi_compile_local _ COMPOSITION_SAMPLE4
			#pragma multi_compile_local _ COMPOSITION_COMBINED
			#include "Composition.cginc"
			ENDCG
		}
	}
}
