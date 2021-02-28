
Shader "Hidden/RenderPipeline/Glitch"
{
	Properties
	{
		_Intensity( "Intensity", float) = 0.025
		_TimeScale( "Time scale", float) = 1.0
		_GlitchParam( "Glitch parameters", Vector) = (1, 1000, 10, 1)
		_ChromaticAberration( "Chromatic aberration", Vector) = (5, 3, 1, 0)
	}
	SubShader
	{
		Pass
		{
			Cull Off
			ZTest Always
			ZWrite Off
			Fog
			{
				Mode off
			}
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			#include "Glitch.cginc"
			
			sampler2D _MainTex;
			float _Intensity;
			float _TimeScale;
			float4 _GlitchParam;
			float3 _ChromaticAberration;
			
			struct VertexInput
			{
				half4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			struct VertexOutput
			{
				half4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};
			void vert( VertexInput v, out VertexOutput o)
			{
				UNITY_SETUP_INSTANCE_ID( v);
				UNITY_INITIALIZE_OUTPUT( VertexOutput, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o);
				o.pos = UnityObjectToClipPos( v.vertex);
				o.uv = v.uv;
			}
			fixed4 frag( VertexOutput i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i);
				if( _Intensity > 0.0)
				{
					return glitch( _MainTex, i.uv, _Intensity, _TimeScale,
						_GlitchParam.xy, _GlitchParam.zw, _ChromaticAberration);
				}
				return tex2D( _MainTex, i.uv);
			}
			ENDCG
		}
	} 
	FallBack Off
}
