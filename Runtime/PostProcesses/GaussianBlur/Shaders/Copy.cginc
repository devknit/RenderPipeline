#ifndef __COPY_CGINC__
#define __COPY_CGINC__

#include "UnityCG.cginc"
	
sampler2D _MainTex;

struct VertexInput
{
	float4 vertex : POSITION;
	fixed4 color : COLOR;
	float2 texcoord : TEXCOORD0;
};
struct VertexOutput
{
	float4 pos : SV_POSITION;
	fixed4 color : COLOR;
	float2 uv : TEXCOORD0;
};
void vert( VertexInput v, out VertexOutput o)
{
	o.pos = UnityObjectToClipPos( v.vertex);
	o.color = v.color;
	o.uv = v.texcoord.xy;
}
float4 frag( VertexOutput i) : SV_Target 
{
	return saturate( tex2D( _MainTex, i.uv) * i.color);
}
#endif /* __COPY_CGINC__ */
