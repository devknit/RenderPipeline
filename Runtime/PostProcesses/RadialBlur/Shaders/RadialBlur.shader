
Shader "Hidden/RenderPipeline/RadialBlur"
{
	Properties
	{
		_Samples( "Samples", Range( 4, 32)) = 16
        _Intensity( "Intensity", float) = 1.0
        _Center( "Center", Vector) = (0.5, 0.5, 0, 0)
        _Radius( "Radius", float) = 0.1
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
			
			sampler2D _MainTex;
			float _Samples;
			float _Intensity;
			float2 _Center;
			float _Radius;
			
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
				
				float2 uv = i.uv - _Center;
				float distance = saturate( length( uv) / _Radius);
				float factar = _Intensity / _Samples * distance;
				float3 color = 0;
				
				for( int i0 = 0; i0 < _Samples; ++i0)
				{
					color += tex2D( _MainTex, uv * (1.0 - factar * i0) + _Center).rgb;
				}
				return fixed4( color / _Samples, 1);
			}
			ENDCG
		}
	} 
	FallBack Off
}
