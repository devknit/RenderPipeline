
Shader "Hidden/RenderPipeline/Glitch"
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
				
				float3 col = SAMPLE( i.uv).rgb;
				
				fixed3 _SliceColor = fixed3( 0.5, 0.7, 0.8);
				float _SliceDot = 5.0;
				float _SliceStep = 0.8;
				
				float sliceFrc = i.uv.y / (_MainTex_TexelSize.y * _SliceDot);
				float sliceStp = step( _SliceStep, frac( sliceFrc));
				
				col = lerp( col, _SliceColor, sliceStp);
				
				
				fixed3 _VignetteColor = fixed3( 0, 0, 0);
				float2 _VignetteCenter = 0.5;
				float _VignetteIntensity = 0.5;
				float _VignetteSmoothness = 1.0;
				float _VignetteRoundness = 0.8;
				float _VignetteRounded = 0.0;
				
				_VignetteIntensity *= 3.0;
				_VignetteSmoothness *= 5.0;
				_VignetteRoundness = lerp( 6, 1, _VignetteRoundness);
				
				half2 d = abs( i.uv - _VignetteCenter) * _VignetteIntensity;
				d.x *= lerp( 1.0, _ScreenParams.x / _ScreenParams.y, _VignetteRounded);
				d = pow( saturate( d), _VignetteRoundness);
				half vfactor = pow( saturate( 1.0 - dot( d, d)), _VignetteSmoothness);
				col *= lerp( _VignetteColor, (1.0).xxx, vfactor);
				
				
				return fixed4( col, 1.0);
			}
			ENDCG
		}
	} 
	FallBack Off
}
