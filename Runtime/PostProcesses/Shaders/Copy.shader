Shader "Hidden/RenderPipeline/Copy"
{
	CGINCLUDE
	#include "UnityCG.cginc"
	
	sampler2D _MainTex;
	
	struct VertexOutput
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};
	void vert( appdata_img v, out VertexOutput o)
	{
		o.pos = UnityObjectToClipPos( v.vertex);
		o.uv = v.texcoord.xy;
	}
	float4 frag( VertexOutput i) : SV_Target 
	{
        return tex2D( _MainTex, i.uv);
	}
	ENDCG
	
	SubShader
	{
		Cull Off ZWrite Off ZTest Always
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	}
}
