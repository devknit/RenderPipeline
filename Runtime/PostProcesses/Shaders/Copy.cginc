#ifndef __RENDER_PIPELINE_COPY_CGINC__
#define __RENDER_PIPELINE_COPY_CGINC__

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
#endif /* __RENDER_PIPELINE_COPY_CGINC__ */
