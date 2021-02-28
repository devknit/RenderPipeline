Shader "Hidden/RenderPipeline/ScreenBlend"
{
	CGINCLUDE
	#include "UnityCG.cginc"
	
	sampler2D _MainTex;
	fixed4    _Color;
	
	struct v2f
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};
	void vert( appdata_img v, out v2f o)
	{
		o.pos = UnityObjectToClipPos( v.vertex);
	#if defined(FLIPHORIZONTAL)
		o.uv.x = 1.0 - v.texcoord.x;
		o.uv.y = v.texcoord.y;
	#else
		o.uv = v.texcoord.xy;
	#endif
	}
	float4 frag( v2f i) : SV_Target 
	{
		fixed4 color = tex2D( _MainTex, i.uv);
        return fixed4( color.rgb * (1.0f - _Color.a) + _Color.rgb * _Color.a, color.a);
	}
	ENDCG
	
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
			#pragma multi_compile_local _ FLIPHORIZONTAL
			ENDCG
		}
	}
}
