Shader "Hidden/RenderPipeline/Bloom/BrightnessExtraction"
{
	SubShader
	{
		Cull Off
		ZWrite Off
		ZTest Always
		BlendOp Add
		Blend One Zero
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_local _ LDR
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			float2 _ColorTransform;
			float _Thresholds;
			
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};
			inline fixed3 remap( fixed3 value, fixed3 srcMin, fixed3 srcMax, fixed3 dstMin, fixed3 dstMax)
			{
				fixed3 volume = srcMax - srcMin;
				return dstMin + ((volume != 0.0)? (value - srcMin) * (dstMax - dstMin) / volume : 0.0);
			}
			inline float remap( fixed3 value, fixed4 param)
			{
				return remap( value, param.x, param.y, param.z, param.w);
			}
			v2f vert( appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos( v.vertex);
				o.uv = v.uv;
				return o;
			}
			fixed4 frag( v2f i) : SV_Target
			{
				fixed4 col = tex2D( _MainTex, i.uv);
			#if defined(LDR)
				col.rgb = saturate( col.rgb * _ColorTransform.x + _ColorTransform.y);
			#else
				col.rgb = max( 0, col.rgb - _Thresholds);
			#endif
				return col;
			}
			ENDCG
		}
	}
}
