
Shader "Hidden/RenderPipeline/Antialiasing/FXAA"
{
	Properties
	{
		_Threshold( "Threshold", float) = 0.25
		_Sharpness( "Sharpness",float) = 4.0
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

			UNITY_DECLARE_SCREENSPACE_TEXTURE( _MainTex)
			uniform float4 _MainTex_TexelSize;
			uniform float _Threshold;
			uniform float _Sharpness;

		#ifdef UNITY_COLORSPACE_GAMMA
			#define lum float3( 0.22, 0.707, 0.071)
		#else 
			#define lum float3( 0.0396819152, 0.45802179, 0.00609653955)
		#endif
			#define SAMPLE( uv) UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, UnityStereoTransformScreenSpaceTex( uv))
			
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
				float4 uv1 : TEXCOORD1;
				float4 uv2 : TEXCOORD2;
				UNITY_VERTEX_OUTPUT_STEREO
			};
			void vert( VertexInput v, out VertexOutput o)
			{
				UNITY_SETUP_INSTANCE_ID( v);
				UNITY_INITIALIZE_OUTPUT( VertexOutput, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o);
				
				o.pos = UnityObjectToClipPos( v.vertex);
				o.uv = v.uv;
				float2 offset = _MainTex_TexelSize.xy * 0.5;
				o.uv1 = float4( v.uv - offset, v.uv + offset);
				o.uv2 = float4( offset, offset * 4.0);
			}
			fixed4 frag( VertexOutput i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i);
				
				float3 col = SAMPLE( i.uv).rgb;
				float gr = dot( col, lum);
				float gtl = dot( SAMPLE( i.uv1.xy).rgb, lum);
				float gbl = dot( SAMPLE( i.uv1.xw).rgb, lum);
				float gtr = dot( SAMPLE( i.uv1.zy).rgb, lum) + 0.0026041667;
				float gbr = dot( SAMPLE( i.uv1.zw).rgb, lum);

				float gmax = max( max( gtr, gbr), max( gtl, gbl));
				float gmin = min( min( gtr, gbr), min( gtl, gbl));

				if( max( gmax, gr) - min( gmin, gr) < max( 0.0, gmax * _Threshold))
				{
					return fixed4( col, 1.0);
				}

				float diff1 = gbl - gtr;
				float diff2 = gbr - gtl;

				float2 mltp = normalize( float2( diff1 + diff2, diff1 - diff2));
				float dvd = min( abs( mltp.x), abs( mltp.y)) * _Sharpness;

				float3 tmp1 = saturate( SAMPLE( i.uv - mltp * i.uv2.xy).rgb);
				float3 tmp2 = saturate( SAMPLE( i.uv + mltp * i.uv2.xy).rgb);

				mltp = clamp( mltp.xy / dvd, -2.0, 2.0);

				float3 tmp3 = saturate( SAMPLE( i.uv - mltp * i.uv2.zw).rgb);
				float3 tmp4 = saturate( SAMPLE( i.uv + mltp * i.uv2.zw).rgb);

				float3 col1 = tmp1 + tmp2;
				float3 col2 = ((tmp3 + tmp4) * 0.25) + (col1 * 0.25);
				
				return fixed4( saturate( (dot( col1, lum) < gmin || dot( col2, lum) > gmax)? col1 * 0.5 : col2), 1.0);
			}
			ENDCG
		}
	} 
	FallBack Off
}
