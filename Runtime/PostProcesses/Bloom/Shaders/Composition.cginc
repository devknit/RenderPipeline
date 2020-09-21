#ifndef __BLOOM_COMPOSITION_CGINC__
#define __BLOOM_COMPOSITION_CGINC__

#include "UnityCG.cginc"

sampler2D _MainTex;
sampler2D _BloomTex;
sampler2D _BloomCombinedTex;
float4 _BloomUvTransform0;
float4 _BloomUvTransform1;
float4 _BloomUvTransform2;
float4 _BloomUvTransform3;
float4 _BloomUvTransform4;
float4 _BloomUvTransform5;
float4 _BloomUvTransformCombined;
float _BloomWeight0;
float _BloomWeight1;
float _BloomWeight2;
float _BloomWeight3;
float _BloomWeight4;
float _BloomWeight5;
float _BloomWeightCombined;

struct VertexInput
{
	float4 vertex : POSITION;
};
struct VertexOutput
{
	float4 vertex : SV_POSITION;
	float2 mainUv : TEXCOORD0;
	float2 bloomUv0 : TEXCOORD1;
	float2 bloomUv1 : TEXCOORD2;
	float2 bloomUv2 : TEXCOORD3;
	float2 bloomUv3 : TEXCOORD4;
	float2 bloomUv4 : TEXCOORD5;
	float2 bloomUv5 : TEXCOORD6;
	float2 bloomUvCombined : TEXCOORD7;
};
struct mrt
{
    fixed4 color0 : COLOR0;
    fixed4 color1 : COLOR1;
};
void vert( VertexInput v, out VertexOutput o)
{
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.mainUv = v.vertex;
	o.bloomUv0 = v.vertex * _BloomUvTransform0.xy + _BloomUvTransform0.zw;
	o.bloomUv1 = v.vertex * _BloomUvTransform1.xy + _BloomUvTransform1.zw;
	o.bloomUv2 = v.vertex * _BloomUvTransform2.xy + _BloomUvTransform2.zw;
	o.bloomUv3 = v.vertex * _BloomUvTransform3.xy + _BloomUvTransform3.zw;
	o.bloomUv4 = v.vertex * _BloomUvTransform4.xy + _BloomUvTransform4.zw;
	o.bloomUv5 = v.vertex * _BloomUvTransform5.xy + _BloomUvTransform5.zw;
	o.bloomUvCombined = v.vertex * _BloomUvTransformCombined.xy + _BloomUvTransformCombined.zw;
}
fixed4 composite( VertexOutput i)
{
	fixed4 c = tex2D( _MainTex, i.mainUv);
#ifdef COMPOSITION_SAMPLE4
	c += tex2D( _BloomTex, i.bloomUv0) * _BloomWeight0;
	c += tex2D( _BloomTex, i.bloomUv1) * _BloomWeight1;
	c += tex2D( _BloomTex, i.bloomUv2) * _BloomWeight2;
	c += tex2D( _BloomTex, i.bloomUv3) * _BloomWeight3;
#ifdef COMPOSITION_SAMPLE2
	c += tex2D( _BloomTex, i.bloomUv4) * _BloomWeight4;
	c += tex2D( _BloomTex, i.bloomUv5) * _BloomWeight5;
#ifdef COMPOSITION_SAMPLE1
	return fixed4( 1, 0, 1, 1);
#endif
#elif COMPOSITION_SAMPLE1
	c += tex2D( _BloomTex, i.bloomUv4) * _BloomWeight4;
#endif
#elif COMPOSITION_SAMPLE2
	c += tex2D( _BloomTex, i.bloomUv0) * _BloomWeight0;
	c += tex2D( _BloomTex, i.bloomUv1) * _BloomWeight1;
#ifdef COMPOSITION_SAMPLE1
	c += tex2D( _BloomTex, i.bloomUv2) * _BloomWeight2;
#endif
#elif COMPOSITION_SAMPLE1
	c += tex2D( _BloomTex, i.bloomUv0) * _BloomWeight0;
#endif
#ifdef COMPOSITION_COMBINED
	c += tex2D( _BloomCombinedTex, i.bloomUvCombined) * _BloomWeightCombined;
#endif
	return saturate( c);
}
fixed4 frag( VertexOutput i) : COLOR
{
	return composite( i);
}
void fragMRT( VertexOutput i, out mrt o)
{
	o.color0 = composite( i);
	o.color1 = o.color0;
}
#endif /* __BLOOM_COMPOSITION_CGINC__ */
