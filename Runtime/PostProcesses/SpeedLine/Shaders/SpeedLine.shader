
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
			#include "UnityCG.cginc"
			#include "SimplexNoise.cginc"
			
			sampler2D _MainTex;
			float4 _MainTex_ST;
		#if UNITY_UV_STARTS_AT_TOP
			float4  _MainTex_TexelSize;
		#endif
			float2 _Center; 
			float2 _AxisVolume; 
			float4 _Color;
			float _Tiling;
			float _RadialScale;
			float _ToneVolume;
			float _ToneBorder;
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
			#define PI2I	0.15915494309189 /* 1 / 2π */
			
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
			fixed4 frag( VertexOutput i) : SV_Target
			{
				float4 color = tex2D( _MainTex, i.uv * _MainTex_ST.xy + _MainTex_ST.zw);
				float2 center = lerp( _Center, i.uv - _Center, _AxisVolume);
				float2 v = float2( 
					(length( center) * _RadialScale * 2.0) + (-_AnimationSpeed * _Time.y), 
					(atan2( center.x, center.y) * PI2I * _Tiling));
				float p = _SmoothBorder - _SmoothWidth;
				float q = _SmoothBorder + _SmoothWidth;
				float n = (snoise( v) * 0.5 + 0.5) * smoothstep( p, q, length( center));
				float alpha = n * _Color.a;
				return lerp( color , float4( n * _Color.rgb, 0.0), 
					lerp( alpha, step( _ToneBorder, alpha), _ToneVolume));
			}
			ENDCG
		}
	} 
	FallBack Off
}
