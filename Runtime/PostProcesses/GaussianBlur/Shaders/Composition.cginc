#ifndef __COMPOSITION_CGINC__
#define __COMPOSITION_CGINC__

#include "UnityCG.cginc"

sampler2D _MainTex;
sampler2D _BlurTex;
sampler2D _BlurCombinedTex;
float4 _BlurUvTransform0;
float4 _BlurUvTransform1;
float4 _BlurUvTransform2;
float4 _BlurUvTransform3;
float4 _BlurUvTransform4;
float4 _BlurUvTransform5;
float4 _BlurUvTransformCombined;
float _BlurWeight0;
float _BlurWeight1;
float _BlurWeight2;
float _BlurWeight3;
float _BlurWeight4;
float _BlurWeight5;
float _BlurWeightCombined;

struct VertexInput
{
	float4 vertex : POSITION;
};
struct VertexOutput
{
	float4 vertex : SV_POSITION;
	float2 mainUv : TEXCOORD0;
	float2 blurUv0 : TEXCOORD1;
	float2 blurUv1 : TEXCOORD2;
	float2 blurUv2 : TEXCOORD3;
	float2 blurUv3 : TEXCOORD4;
	float2 blurUv4 : TEXCOORD5;
	float2 blurUv5 : TEXCOORD6;
	float2 blurUvCombined : TEXCOORD7;
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
	o.blurUv0 = v.vertex * _BlurUvTransform0.xy + _BlurUvTransform0.zw;
	o.blurUv1 = v.vertex * _BlurUvTransform1.xy + _BlurUvTransform1.zw;
	o.blurUv2 = v.vertex * _BlurUvTransform2.xy + _BlurUvTransform2.zw;
	o.blurUv3 = v.vertex * _BlurUvTransform3.xy + _BlurUvTransform3.zw;
	o.blurUv4 = v.vertex * _BlurUvTransform4.xy + _BlurUvTransform4.zw;
	o.blurUv5 = v.vertex * _BlurUvTransform5.xy + _BlurUvTransform5.zw;
	o.blurUvCombined = v.vertex * _BlurUvTransformCombined.xy + _BlurUvTransformCombined.zw;
}
fixed4 composite( VertexOutput i)
{
	fixed4 c = fixed4( 0, 0, 0, 1);//tex2D( _MainTex, i.mainUv);
#ifdef COMPOSITION_SAMPLE4
	c += tex2D( _BlurTex, i.blurUv0) * _BlurWeight0;
	c += tex2D( _BlurTex, i.blurUv1) * _BlurWeight1;
	c += tex2D( _BlurTex, i.blurUv2) * _BlurWeight2;
	c += tex2D( _BlurTex, i.blurUv3) * _BlurWeight3;
#ifdef COMPOSITION_SAMPLE2
	c += tex2D( _BlurTex, i.blurUv4) * _BlurWeight4;
	c += tex2D( _BlurTex, i.blurUv5) * _BlurWeight5;
#ifdef COMPOSITION_SAMPLE1
	return fixed4( 1, 0, 1, 1);
#endif
#elif COMPOSITION_SAMPLE1
	c += tex2D( _BlurTex, i.blurUv4) * _BlurWeight4;
#endif
#elif COMPOSITION_SAMPLE2
	c += tex2D( _BlurTex, i.blurUv0) * _BlurWeight0;
	c += tex2D( _BlurTex, i.blurUv1) * _BlurWeight1;
#ifdef COMPOSITION_SAMPLE1
	c += tex2D( _BlurTex, i.blurUv2) * _BlurWeight2;
#endif
#elif COMPOSITION_SAMPLE1
	c += tex2D( _BlurTex, i.blurUv0) * _BlurWeight0;
#endif
#ifdef COMPOSITION_COMBINED
	c += tex2D( _BlurCombinedTex, i.blurUvCombined) * _BlurWeightCombined;
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
#endif /* __COMPOSITION_CGINC__ */
