
#ifndef __LENSDISTORTION_CGINC__
#define __LENSDISTORTION_CGINC__

#include "UnityCG.cginc"

float4 _LensDistortionAmount;
float4 _LensDistortionCenterScale;

inline float2 LensDistortion( float2 uv)
{
#if defined(_LENSDISTORTION)
	uv = (uv - 0.5) * _LensDistortionAmount.z + 0.5;
	float2 ruv = _LensDistortionCenterScale.zw * (uv - 0.5 - _LensDistortionCenterScale.xy);
	float ru = length(float2(ruv));
	
	UNITY_BRANCH
	if( _LensDistortionAmount.w > 0.0)
	{
		float wu = ru * _LensDistortionAmount.x;
		ru = tan(wu) * (1.0 / (ru * _LensDistortionAmount.y));
		uv = uv + ruv * (ru - 1.0);
	}
	else
	{
		ru = (1.0 / ru) * _LensDistortionAmount.x * atan(ru * _LensDistortionAmount.y);
		uv = uv + ruv * (ru - 1.0);
	}
#endif
	return uv;
}
#endif /* __LENSDISTORTION_CGINC__ */
