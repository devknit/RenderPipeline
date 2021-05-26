
#ifndef __VIGNETTE_CGINC__
#define __VIGNETTE_CGINC__

#include "UnityCG.cginc"

fixed4 _VignetteColor;
float2 _VignetteCenter;
float4 _VignetteParam;

fixed4 Vignette( fixed4 color, float2 uv)
{
	half2 d = abs( uv - _VignetteCenter) * _VignetteParam.x;
	d.x *= lerp( 1.0, _ScreenParams.x / _ScreenParams.y, _VignetteParam.w);
	d = pow( saturate( d), _VignetteParam.z);
	half vfactor = lerp( 1.0, pow( saturate( 1.0 - dot( d, d)), _VignetteParam.y), _VignetteColor.a);
	return fixed4( color.rgb * lerp( _VignetteColor.rgb, fixed3( 1, 1, 1), vfactor), saturate( color.a + (1.0 - vfactor)));
}
#endif /* __VIGNETTE_CGINC__ */
