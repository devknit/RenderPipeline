#ifndef __GLITCH_CGINC__
#define __GLITCH_CGINC__

#include "UnityCG.cginc"

inline float rand( float2 seed)
{
	return frac( sin( dot( seed.xy, float2( 12.9898, 78.233))) * 41758.5453);
}
inline float trunc( float x, float volume)
{
	return floor( x * volume) / volume;
}
inline float trunc( float2 x, float volume)
{
	return floor( x * volume) / volume;
}
fixed4 glitch( sampler2D tex, float2 uv, float intensity, float timeScale, 
	float2 threshold, float2 offsetVolume, float3 chromaticAberration)
{
	intensity = saturate( intensity);

	float time = fmod( _Time.x * timeScale * 0.001, 0.3);
	
	float rnd0 = rand( trunc( time.xx, 6));
	float r0 = saturate( (1.0 - intensity) * 0.7 + rnd0);
	
	float rnd1 = rand( float2( trunc( uv.x, threshold.x * r0), time));
	float r11 = 0.5 - 0.5 * intensity + rnd1;
#if 0
	float r1 = 1.0 - max( 0.0, r11 < 1.0 ? r11 : r11);
#else
	float r1 = 1.0 - max( 0.0, r11);
#endif
	float r2 = saturate( rand( float2( trunc( uv.y, threshold.y * r1), time)));
	
#if UNITY_SINGLE_PASS_STEREO
	float vrFactor = 0.5;
#else
	float vrFactor = 1;
#endif
	float offset = 0.05 * intensity * vrFactor;
	offset = offset * r2 * (rnd0 > 0.5 ? 1.0 : -1.0) + (offset * 0.5);
	
	float t = 1.0 - step( intensity, 0);
	chromaticAberration *= t;
	offsetVolume *= t;
	
	return fixed4(
		tex2D( tex, uv + (offset * chromaticAberration.r) * offsetVolume.xy).r,
		tex2D( tex, uv + (offset * chromaticAberration.g) * offsetVolume.xy).g,
		tex2D( tex, uv + (offset * chromaticAberration.b) * offsetVolume.xy).b,
		1);
}
#endif /* __GLITCH_CGINC__ */
