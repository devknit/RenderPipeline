Shader "Hidden/RenderPipeline/ColorFilter"
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
			#include "ColorFilter.cginc"
			
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
			}
			fixed4 frag( VertexOutput i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i);
				return ColorFilter( UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, i.uv));
			}
			ENDCG
		}
	}
}
