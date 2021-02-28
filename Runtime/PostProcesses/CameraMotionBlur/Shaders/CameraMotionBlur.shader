
Shader "Hidden/RenderPipeline/CameraMotionBlur"
{
	Properties
	{
		_Distance( "Distance", Range( 0, 1)) = 1.0
	}
	CGINCLUDE
	#include "UnityCG.cginc"
#if defined(QUALITY_HIGH) 
	#define MULTIPLAYER 0.1
#elif defined(QUALITY_MIDDLE)
	#define MULTIPLAYER 0.125
#else
	#define MULTIPLAYER 0.16666667
#endif
	UNITY_DECLARE_SCREENSPACE_TEXTURE( _MainTex);
	UNITY_DECLARE_SCREENSPACE_TEXTURE( _BlurTex);
	float4x4 _CurrentToPreviousViewProjectionMatrix;
	float _Distance;
	
	struct VertexInput
	{
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};
	struct VertexOutputBlur
	{
		float4 pos : POSITION;
		float4 uv : TEXCOORD0;
		float4 uv1 : TEXCOORD1;
		float4 uv2 : TEXCOORD2;
	#if defined(QUALITY_MIDDLE) || defined(QUALITY_HIGH)
		float4 uv3 : TEXCOORD3;
	#endif
	#if defined(QUALITY_HIGH)
		float4 uv4 : TEXCOORD4;
	#endif
		UNITY_VERTEX_OUTPUT_STEREO
	};
	void vertBlur( VertexInput i, out VertexOutputBlur o)
	{
		UNITY_SETUP_INSTANCE_ID( i);
		UNITY_INITIALIZE_OUTPUT( VertexOutputBlur, o);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o);
		
		o.pos = UnityObjectToClipPos( i.pos);
		float4 projPos = float4( i.uv * 2.0 - 1.0, _Distance, 1.0);
		float4 previous = mul( _CurrentToPreviousViewProjectionMatrix, projPos);
		previous /= previous.w;
		float2 vel = (previous.xy - projPos.xy) * MULTIPLAYER * 0.5;
		o.uv.xy = i.uv;
		o.uv.zw = vel;
		o.uv1.xy = vel * 2.0;
		o.uv1.zw = vel * 3.0;
		o.uv2.xy = vel * 4.0;
		o.uv2.zw = vel * 5.0;
#if defined(QUALITY_MIDDLE) || defined(QUALITY_HIGH)
		o.uv3.xy = vel * 6.0;
		o.uv3.zw = vel * 7.0;
#endif
#if defined(QUALITY_HIGH)
		o.uv4.xy = vel * 8.0;
		o.uv4.zw = vel * 9.0;
#endif
	}
	fixed4 fragBlur( VertexOutputBlur i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i);
		float2 uv = UnityStereoTransformScreenSpaceTex( i.uv);
		float4 col = UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, uv);
		float col1A = 1.0;//UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, uv + i.uv.zw).a;
		float col2A = 1.0;//UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, uv + i.uv1.xy).a;
		float col3A = 1.0;//UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, uv + i.uv1.zw).a;
		float col4A = 1.0;//UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, uv + i.uv2.xy).a;
		float col5A = 1.0;//UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, uv + i.uv2.zw).a;
		col += UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, uv + i.uv.zw * col1A);
		col += UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, uv + i.uv1.xy * col2A);
		col += UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, uv + i.uv1.zw * col3A);
		col += UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, uv + i.uv2.xy * col4A);
		col += UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, uv + i.uv2.zw * col5A);

#if defined(QUALITY_MIDDLE) || defined(QUALITY_HIGH)
		float col6A = UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, uv + i.uv3.xy).a;
		float col7A = UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, uv + i.uv3.zw).a;
		col += UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, uv + i.uv3.zw * col6A);
		col += UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, uv + i.uv3.xy * col7A);
#endif
#if defined(QUALITY_HIGH)
		float col8A = UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, uv + i.uv4.xy).a;
		float col9A = UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, uv + i.uv4.zw).a;
		col += UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, uv + i.uv4.zw * col8A);
		col += UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, uv + i.uv4.xy * col9A);
#endif
		return col * MULTIPLAYER;
	}
	struct VertexOutput
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		UNITY_VERTEX_OUTPUT_STEREO
	};
	void vert( VertexInput i, out VertexOutput o)
	{
		UNITY_SETUP_INSTANCE_ID( i);
		UNITY_INITIALIZE_OUTPUT( VertexOutput, o);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o);
		o.pos = UnityObjectToClipPos( i.pos);
		o.uv = i.uv;
	}
	fixed4 frag( VertexOutput i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i);
		fixed4 col = UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, UnityStereoTransformScreenSpaceTex( i.uv));
		fixed4 blur = UNITY_SAMPLE_SCREENSPACE_TEXTURE( _BlurTex, UnityStereoTransformScreenSpaceTex( i.uv));
		return lerp( col, blur, 1.0);
	}
	ENDCG
	
	SubShader
	{
		Cull Off
		ZTest Always
		ZWrite Off
		Fog{ Mode off }
			
		Pass
		{
			CGPROGRAM
			#pragma vertex vertBlur
			#pragma fragment fragBlur
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile_local QUALITY_LOW QUALITY_MIDDLE QUALITY_HIGH
			ENDCG
		}
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			ENDCG
		}
	} 
	FallBack Off
}
