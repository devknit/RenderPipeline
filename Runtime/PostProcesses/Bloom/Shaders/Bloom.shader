Shader "Hidden/RenderPipeline/Bloom"
{
	Properties
	{
		// Blending Status
		[Enum( UnityEngine.Rendering.BlendOp)]
		_ColorBlendOp( "Color Blend Op", float) = 0 // Add
		[Enum( UnityEngine.Rendering.BlendMode)]
		_ColorSrcFactor( "Color Src Factor", float) = 1 // One
		[Enum( UnityEngine.Rendering.BlendMode)]
		_ColorDstFactor( "Color Dst Factor", float) = 0 // Zero
		[Enum( UnityEngine.Rendering.BlendOp)]
		_AlphaBlendOp( "Alpha Blend Op", float) = 0 // Add
		[Enum( UnityEngine.Rendering.BlendMode)]
		_AlphaSrcFactor( "Alpha Src Factor", float) = 1 // One
		[Enum( UnityEngine.Rendering.BlendMode)]
		_AlphaDstFactor( "Alpha Dst Factor", float) = 0 // Zero
	}
	SubShader
	{
		Cull Off
		ZWrite Off
		ZTest Always
			
		Pass
		{
			BlendOp Add, Add
			Blend One Zero, One Zero
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_local _ LDR
			#include "BrightnessExtraction.cginc"
			ENDCG
		}
		Pass
		{
			BlendOp Add, Add
			Blend One Zero, One Zero
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "../../GaussianBlur/Shaders/GaussianBlur.cginc"
			ENDCG
		}
		Pass
		{
			BlendOp Add, Add
			Blend One Zero, One Zero
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_local _ COMBINE_SAMPLE1
			#pragma multi_compile_local _ COMBINE_SAMPLE2
			#pragma multi_compile_local _ COMBINE_SAMPLE4
			#include "../../GaussianBlur/Shaders/Combine.cginc"
			ENDCG
		}
		Pass
		{
			BlendOp [_ColorBlendOp], [_AlphaBlendOp]
			Blend [_ColorSrcFactor] [_ColorDstFactor], [_AlphaSrcFactor] [_AlphaDstFactor]
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_local _ COMPOSITION_SAMPLE1
			#pragma multi_compile_local _ COMPOSITION_SAMPLE2
			#pragma multi_compile_local _ COMPOSITION_SAMPLE4
			#pragma multi_compile_local _ COMPOSITION_COMBINED
			#include "../../GaussianBlur/Shaders/Composition.cginc"
			ENDCG
		}
	}
}
