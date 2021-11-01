
Shader "Hidden/RenderPipeline/SSAO"
{
	SubShader
	{
		CGINCLUDE
		#include "UnityCG.cginc"

		UNITY_DECLARE_SCREENSPACE_TEXTURE( _BlurTex);
		UNITY_DECLARE_SCREENSPACE_TEXTURE( _MainTex);
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture);
		
		#define SAMPLE_DEPTH( uv) LinearEyeDepth( SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, uv));
		
		half4 _CameraDepthTexture_TexelSize;
		half4 _MainTex_TexelSize;
		half _Intensity;
		half _BlurAmount;
		half _Radius;
		half _Area;
		half _IgnoreDistance;
		
		struct VertexInput
		{
			float4 pos : POSITION;
			float2 uv : TEXCOORD0;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};
		
		struct VertexOutputShade
		{
			float4 pos  : SV_POSITION;
			float4  uv  : TEXCOORD0;
			float4  uv1  : TEXCOORD1;
			float3  uv2  : TEXCOORD2;
			UNITY_VERTEX_INPUT_INSTANCE_ID
			UNITY_VERTEX_OUTPUT_STEREO
		};
		void vertShade( VertexInput i, out VertexOutputShade o)
		{
			UNITY_SETUP_INSTANCE_ID( i);
			UNITY_INITIALIZE_OUTPUT( VertexOutputShade, o);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o);
			o.pos = UnityObjectToClipPos( i.pos);
			o.uv = float4( i.uv, i.uv + _CameraDepthTexture_TexelSize.xy);
			float a = 52.98292f * dot( float2( 0.0671h, 0.00584h), floor( 0.5h * i.uv * _ScreenParams.xy));
			float uv2 = frac( a + 0.11231h) * 2.0h - 1.0h;
			float uv3 = frac( a + 0.14844h) * 2.0h - 1.0h;
			float uv4 = frac( a + 0.13672h) * 2.0h - 1.0h;
			a *= 6.2832h;
			o.uv1 = _Radius * _Radius * float4( float2( -0.40825h, 0.40825h) * sqrt( 1.0h - uv2 * uv2), float2( 0.5h, -0.5h) * sqrt( 1.0h - uv3 * uv3));
			o.uv2.xy = _Radius * _Radius * float2( -0.57735h, -0.57735h) * sqrt( 1.0h - uv4 * uv4);
			o.uv2.z = a;
		}
		float4 fragShade( VertexOutputShade i) : SV_Target
		{
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i);
			
			const float3 offset = float3( _CameraDepthTexture_TexelSize.xy, 0.0h);

			float depth = SAMPLE_DEPTH( i.uv.xy);
			float depthu = SAMPLE_DEPTH( i.uv.xw);
			float depthr = SAMPLE_DEPTH( i.uv.zy);

			float3 pu = float3( offset.zy, depthu - depth);
			float3 pr = float3( offset.xz, depthr - depth);
			float3 normal = normalize( cross( pu, pr));
			float s = sin( i.uv2.z);
			float c = cos( i.uv2.z);

			float2 ray = float2( s * -0.9863h - c * 0.1649h, s * 0.1649h - c * 0.9863h) * i.uv1.zw / depth;
			float difference = depth - SAMPLE_DEPTH( saturate( i.uv.xy + sign( dot( ray, normal.xy)) * ray));
			difference = step( 0, difference - _IgnoreDistance) * difference;
			float occlusion = step( 0.02h, difference) * (1.0h - smoothstep( 0.02h, _Area, difference));
		#if !defined(FASTMODE)
			ray = float2( s * -0.38267h - c * 0.92388h, s * 0.92388h - c * 0.38267h) * i.uv1.xy / depth;
			difference = depth - SAMPLE_DEPTH( saturate( i.uv.xy + sign( dot( ray, normal.xy)) * ray));
			difference = step( 0, difference - _IgnoreDistance) * difference;
			occlusion += step( 0.02h, difference) * (1.0h - smoothstep( 0.02h, _Area, difference));

			ray = float2( s * 0.94953h - c * 0.3137h, s * 0.3137h + c * 0.94953h) * i.uv2.xy / depth;
			difference = depth - SAMPLE_DEPTH( saturate( i.uv.xy + sign( dot( ray, normal.xy)) * ray));
			difference = step( 0, difference - _IgnoreDistance) * difference;
			occlusion += step( 0.02h, difference) * (1.0h - smoothstep( 0.02h, _Area, difference));

			return 1.0h - _Intensity * occlusion * 0.3333h;
		#endif
			return 1.0h - _Intensity * occlusion;
		}
		
		struct VertexOutputBlur
		{
			float4 pos : SV_POSITION;
			float2  uv : TEXCOORD0;
			float4  uv1 : TEXCOORD1;
			UNITY_VERTEX_INPUT_INSTANCE_ID
			UNITY_VERTEX_OUTPUT_STEREO
		};
		void vertBlur( VertexInput i, out VertexOutputBlur o)
		{
			UNITY_SETUP_INSTANCE_ID( i);
			UNITY_INITIALIZE_OUTPUT( VertexOutputBlur, o);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o);
			o.pos = UnityObjectToClipPos( i.pos);
			float2 offset = _MainTex_TexelSize.xy * _BlurAmount;
			o.uv = i.uv;
			o.uv1 = float4( i.uv - offset, i.uv + offset);
		}
		fixed4 fragBlur( VertexOutputBlur i) : SV_Target
		{
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i);
			fixed4 col = UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, i.uv);
			col.r *= 0.5h;
			col.r += UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, i.uv1.xy).r * 0.125h;
			col.r += UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, i.uv1.xw).r * 0.125h;
			col.r += UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, i.uv1.zy).r * 0.125h;
			col.r += UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, i.uv1.zw).r * 0.125h;
			return col;
		}
		
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
			fixed4 col = UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, i.uv);
			fixed4 ao = UNITY_SAMPLE_SCREENSPACE_TEXTURE( _BlurTex, i.uv);
			col.rgb *= ao.r;
			return col;
		}
		ENDCG
		
		Pass
		{
			Cull Off
			ZTest Always
			ZWrite Off
			Fog{ Mode off }
		
			CGPROGRAM
			#pragma vertex vertShade
			#pragma fragment fragShade
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile_local _ FASTMODE
			ENDCG
		}
		Pass
		{
			Cull Off
			ZTest Always
			ZWrite Off
			Fog{ Mode off }
			
			CGPROGRAM
			#pragma vertex vertBlur
			#pragma fragment fragBlur
			#pragma fragmentoption ARB_precision_hint_fastest
			ENDCG
		}
		Pass
		{
			Cull Off
			ZTest Always
			ZWrite Off
			Fog{ Mode off }
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			ENDCG
		}
	} 
	FallBack Off
}
