Shader "Hidden/RenderPipeline/Ubar"
{
	SubShader
	{
		Cull Off
		ZWrite Off
		ZTest Always
		Blend Off
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_local _ _VIGNETTE
			#pragma multi_compile_local _ _FLIP_X
			#pragma multi_compile_local _ _FLIP_Y
			#include "../Ubar/Vignette/Shaders/Vignette.cginc"
			
			UNITY_DECLARE_SCREENSPACE_TEXTURE( _MainTex);
			
			struct VertexInput
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			struct VertexOutput
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			void vert( VertexInput i, out VertexOutput o)
			{
				UNITY_SETUP_INSTANCE_ID( i);
				UNITY_INITIALIZE_OUTPUT(VertexOutput, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o);
				o.pos = UnityObjectToClipPos( i.pos);
				o.uv = UnityStereoTransformScreenSpaceTex( i.uv);
			#if defined(_FLIP_X)
				o.uv.x = 1.0 - o.uv.x;
			#endif
			#if defined(_FLIP_Y)
				o.uv.y = 1.0 - o.uv.y;
			#endif
			}
			fixed4 frag( VertexOutput i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i);
				fixed4 color = UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, i.uv);
			#if defined(_VIGNETTE)
				color = Vignette( color, i.uv);
			#endif
				return color;
			}
			ENDCG
		}
	}
}
