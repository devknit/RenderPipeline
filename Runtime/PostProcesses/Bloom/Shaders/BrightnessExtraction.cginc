#ifndef __BLOOM_BRIGHTNESS_EXTRACTION_CGINC__
#define __BLOOM_BRIGHTNESS_EXTRACTION_CGINC__

#include "UnityCG.cginc"
			
sampler2D _MainTex;
#if defined(LDR)
	float2 _ColorTransform;
#else
	float _Thresholds;
#endif

struct VertexInput
{
	float4 vertex : POSITION;
	fixed4 color : COLOR;
	float2 uv : TEXCOORD0;
};
struct VertexOutput
{
	float2 uv : TEXCOORD0;
	fixed4 color : COLOR;
	float4 vertex : SV_POSITION;
};
void vert( VertexInput v, out VertexOutput o)
{
	o.vertex = UnityObjectToClipPos( v.vertex);
	o.color = v.color;
	o.uv = v.uv;
}
fixed4 frag( VertexOutput i) : SV_Target
{
	fixed4 col = tex2D( _MainTex, i.uv);
#if defined(LDR)
	col.rgb = saturate( col.rgb * _ColorTransform.x + _ColorTransform.y);
#else
	col.rgb = max( 0, col.rgb - _Thresholds);
#endif
	return col * i.color;
}
#endif /* __BLOOM_BRIGHTNESS_EXTRACTION_CGINC__ */
