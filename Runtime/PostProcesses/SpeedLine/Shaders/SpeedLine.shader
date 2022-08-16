
Shader "Hidden/RenderPipeline/SpeedLine"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_Tiling("Speed Lines Tiling", Float) = 200
		_RadialScale("Speed Lines Radial Scale", Range( 0 , 10)) = 0.1
		_Animation("Speed Lines Animation", Float) = 3
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
			#pragma multi_compile_local _ _PATTERN_HORIZONTAL _PATTERN_VERTICAL //_PATTERN_RING
			#include "UnityCG.cginc"
			#include "SimplexNoise.cginc"
			
			sampler2D _MainTex;
			float4 _MainTex_ST;
		#if UNITY_UV_STARTS_AT_TOP
			float4  _MainTex_TexelSize;
		#endif
			float2 _Center; 
			float2 _AxisMask; 
			float4 _Color;
			float _Tiling;
			float _RadialScale;
			float _Sparse;
			float _Remap;
			float _SmoothWidth;
			float _SmoothBorder;
			float _AnimationSpeed;
			
			struct VertexInput
			{
				half4 vertex : POSITION;
				float2 uv  : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			struct VertexOutput
			{
				half4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};
			void vert( VertexInput v, out VertexOutput o)
			{
				UNITY_SETUP_INSTANCE_ID( v);
				UNITY_INITIALIZE_OUTPUT( VertexOutput, o);
				o.pos = UnityObjectToClipPos( v.vertex);
				o.uv = v.uv.xy;
			#if UNITY_UV_STARTS_AT_TOP
				o.uv.y = (_MainTex_TexelSize.y < 0.0)? 1.0 - o.uv.y : o.uv.y;
			#endif	
			}
			inline float remap01n1( float v, float min)
			{
				return saturate( (v - min) / (1.0 - min));
			}
			fixed4 frag( VertexOutput i) : SV_Target
			{
				float4 color = tex2D( _MainTex, i.uv);
				float2 center = i.uv - _Center;
				float t = _AnimationSpeed * -_Time.y;
				float l = length( center);
			#if _PATTERN_HORIZONTAL
				float r = center.y;
			#elif _PATTERN_VERTICAL
				float r = center.x;
//			#elif _PATTERN_RING
//				float r = l;
			#else
				float r = atan2( center.x, center.y);
			#endif
				float2 v = float2( 
					(l * _RadialScale * 2.0) + t, 
					(r * 0.15915494309189 * _Tiling)); // (1 / 2π)
				float n = (snoise( v) * 0.5 + 0.5);
				float b = _SmoothBorder * 0.5;
				float m = smoothstep( b - _SmoothWidth, b + _SmoothWidth, length( center * _AxisMask));
				return lerp( color, _Color, remap01n1( pow( n * m, _Sparse), _Remap) * _Color.a);
			}
			ENDCG
		}
	}
	FallBack Off
}
