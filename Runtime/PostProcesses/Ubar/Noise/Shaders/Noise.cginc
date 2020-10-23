
#ifndef __NOISE_CGINC__
#define __NOISE_CGINC__

#include "UnityCG.cginc"

fixed4 _NoiseColor;
float4 _NoiseParam;

inline float Random( float2 st)
{
	return frac( sin( dot( st.xy, float2( 12.9898, 78.233))) * 43758.5453123);
}
fixed4 Noise( fixed4 color, float2 uv)
{
	float vfactor = frac( uv.y / (1.0 / _ScreenParams.y * _NoiseParam.x));
	uv.y += _Time.y * _NoiseParam.w;
	vfactor = smoothstep( _NoiseParam.y, _NoiseParam.z, max( 0, abs( vfactor - 0.5) * 2.0)) * Random( uv);
//	return fixed4( lerp( color.rgb, _NoiseColor.rgb, _NoiseColor.a * vfactor), 1);
	return fixed4( lerp( color.rgb, color.rgb + _NoiseColor.rgb, _NoiseColor.a * vfactor), 1);
}
#endif /* __NOISE_CGINC__ */
