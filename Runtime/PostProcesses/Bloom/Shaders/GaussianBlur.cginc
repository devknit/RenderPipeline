#ifndef __BLOOM_GAUSSIAN_BLUR_CGINC__
#define __BLOOM_GAUSSIAN_BLUR_CGINC__

#include "UnityCG.cginc"

sampler2D _MainTex;
float _InvertOffsetScale01;

struct VertexInput
{
	float4 vertex : POSITION;
	float3 sample0 : TEXCOORD0; //x:u y:v z:weight
	float3 sample1 : TEXCOORD1;
	float3 sample2 : TEXCOORD2;
	float3 sample3 : TEXCOORD3;
};
struct VertexOutput
{
	float4 vertex : SV_POSITION;
	float3 sample0 : TEXCOORD0; //x:u y:v z:weight
	float3 sample1 : TEXCOORD1;
	float3 sample2 : TEXCOORD2;
	float3 sample3 : TEXCOORD3;
	float3 sample4 : TEXCOORD4;
	float3 sample5 : TEXCOORD5;
	float3 sample6 : TEXCOORD6;
	float3 sample7 : TEXCOORD7;
};
void vert( VertexInput v, out VertexOutput o)
{
	o.vertex = UnityObjectToClipPos( v.vertex);
	o.sample0 = v.sample0;
	o.sample1 = v.sample1;
	o.sample2 = v.sample2;
	o.sample3 = v.sample3;
	// 4は0の反対側。5は1の反対側。6は2の反対側。7は3の反対側。
	// 4の位置は0に、1から0へのベクトルを、スカラ倍して加えた所になる。このスカラは_InvertOffsetScale01として与える。
	o.sample4.xy = v.sample0.xy + ((v.sample0.xy - v.sample1.xy) * _InvertOffsetScale01);
	o.sample5.xy = o.sample4.xy + (v.sample0.xy - v.sample1.xy);
	o.sample6.xy = o.sample5.xy + (v.sample1.xy - v.sample2.xy);
	o.sample7.xy = o.sample6.xy + (v.sample2.xy - v.sample3.xy);
	o.sample4.z = v.sample0.z;
	o.sample5.z = v.sample1.z;
	o.sample6.z = v.sample2.z;
	o.sample7.z = v.sample3.z;
}
fixed4 frag( VertexOutput i) : SV_Target
{
	fixed4 c = tex2D( _MainTex, i.sample0.xy) * i.sample0.z;
	c += tex2D( _MainTex, i.sample1.xy) * i.sample1.z;
	c += tex2D( _MainTex, i.sample2.xy) * i.sample2.z;
	c += tex2D( _MainTex, i.sample3.xy) * i.sample3.z;
	c += tex2D( _MainTex, i.sample4.xy) * i.sample4.z;
	c += tex2D( _MainTex, i.sample5.xy) * i.sample5.z;
	c += tex2D( _MainTex, i.sample6.xy) * i.sample6.z;
	c += tex2D( _MainTex, i.sample7.xy) * i.sample7.z;
	return c;
}
#endif /* __BLOOM_GAUSSIAN_BLUR_CGINC__ */
