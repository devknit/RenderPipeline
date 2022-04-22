#ifndef __COMBINE_CGINC__
#define __COMBINE_CGINC__

#include "UnityCG.cginc"

sampler2D _MainTex;
float4 _UvTransform0;
float4 _UvTransform1;
float4 _UvTransform2;
float4 _UvTransform3;
float4 _UvTransform4;
float4 _UvTransform5;
float4 _UvTransform6;
float _Weight0;
float _Weight1;
float _Weight2;
float _Weight3;
float _Weight4;
float _Weight5;
float _Weight6;

struct VertexInput
{
	float4 vertex : POSITION;
};
struct VertexOutput
{
	float4 vertex : SV_POSITION;
	float2 uv0 : TEXCOORD0;
	float2 uv1 : TEXCOORD1;
	float2 uv2 : TEXCOORD2;
	float2 uv3 : TEXCOORD3;
	float2 uv4 : TEXCOORD4;
	float2 uv5 : TEXCOORD5;
	float2 uv6 : TEXCOORD6;
};
void vert( VertexInput v, out VertexOutput o)
{
	o.vertex = UnityObjectToClipPos( v.vertex);
	o.uv0 = v.vertex * _UvTransform0.xy + _UvTransform0.zw;
	o.uv1 = v.vertex * _UvTransform1.xy + _UvTransform1.zw;
	o.uv2 = v.vertex * _UvTransform2.xy + _UvTransform2.zw;
	o.uv3 = v.vertex * _UvTransform3.xy + _UvTransform3.zw;
	o.uv4 = v.vertex * _UvTransform4.xy + _UvTransform4.zw;
	o.uv5 = v.vertex * _UvTransform5.xy + _UvTransform5.zw;
	o.uv6 = v.vertex * _UvTransform6.xy + _UvTransform6.zw;
}
fixed4 frag( VertexOutput i) : SV_Target
{
	fixed4 c = tex2D( _MainTex, i.uv0) * _Weight0;
#ifdef COMBINE_SAMPLE4
	c += tex2D( _MainTex, i.uv1) * _Weight1;
	c += tex2D( _MainTex, i.uv2) * _Weight2;
	c += tex2D( _MainTex, i.uv3) * _Weight3;
#ifdef COMBINE_SAMPLE2
	c += tex2D( _MainTex, i.uv4) * _Weight4;
	c += tex2D( _MainTex, i.uv5) * _Weight5;
#ifdef COMBINE_SAMPLE1
	c += tex2D( _MainTex, i.uv6) * _Weight6;
#endif
#elif COMBINE_SAMPLE1
	c += tex2D( _MainTex, i.uv4) * _Weight4;
#endif
#elif COMBINE_SAMPLE2
	c += tex2D( _MainTex, i.uv1) * _Weight1;
#ifdef COMBINE_SAMPLE1
	c += tex2D( _MainTex, i.uv2) * _Weight2;
#endif
#elif SAMPLE1
#else
	return fixed4( 1, 0, 1, 1);
#endif
	return c;
}
#endif /* __COMBINE_CGINC__ */
