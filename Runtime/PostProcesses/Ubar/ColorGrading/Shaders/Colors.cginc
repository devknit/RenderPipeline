
#ifndef __COLORS_CGINC__
#define __COLORS_CGINC__

#include "UnityCG.cginc"

#ifndef USE_VERY_FAST_SRGB
    #if defined(SHADER_API_MOBILE)
        #define USE_VERY_FAST_SRGB 1
    #else
        #define USE_VERY_FAST_SRGB 0
    #endif
#endif

#ifndef USE_FAST_SRGB
    #if defined(SHADER_API_CONSOLE)
        #define USE_FAST_SRGB 1
    #else
        #define USE_FAST_SRGB 0
    #endif
#endif

#define EPSILON         1.0e-4

#define FLT_EPSILON     1.192092896e-07
#define FLT_MIN         1.175494351e-38
#define FLT_MAX         3.402823466e+38

static const float3x3 LIN_2_LMS_MAT = {
    3.90405e-1, 5.49941e-1, 8.92632e-3,
    7.08416e-2, 9.63172e-1, 1.35775e-3,
    2.31082e-2, 1.28021e-1, 9.36245e-1
};

static const float3x3 LMS_2_LIN_MAT = {
    2.85847e+0, -1.62879e+0, -2.48910e-2,
    -2.10182e-1,  1.15820e+0,  3.24281e-4,
    -4.18120e-2, -1.18169e-1,  1.06867e+0
};
float FastSign( float x)
{
    return saturate(x * FLT_MAX + 0.5) * 2.0 - 1.0;
}
float PositivePow( float base, float power)
{
    return pow( max( abs( base), float( FLT_EPSILON)), power);
}
float2 PositivePow( float2 base, float2 power)
{
    return pow( max( abs( base), float2( FLT_EPSILON, FLT_EPSILON)), power);
}
float3 PositivePow( float3 base, float3 power)
{
    return pow( max( abs( base), float3( FLT_EPSILON, FLT_EPSILON, FLT_EPSILON)), power);
}
float4 PositivePow( float4 base, float4 power)
{
    return pow( max( abs( base), float4( FLT_EPSILON, FLT_EPSILON, FLT_EPSILON, FLT_EPSILON)), power);
}


half SRGBToLinear( half c)
{
#if USE_VERY_FAST_SRGB
    return c * c;
#elif USE_FAST_SRGB
    return c * (c * (c * 0.305306011 + 0.682171111) + 0.012522878);
#else
    half linearRGBLo = c / 12.92;
    half linearRGBHi = PositivePow( (c + 0.055) / 1.055, 2.4);
    half linearRGB = (c <= 0.04045) ? linearRGBLo : linearRGBHi;
    return linearRGB;
#endif
}
half3 SRGBToLinear(half3 c)
{
#if USE_VERY_FAST_SRGB
    return c * c;
#elif USE_FAST_SRGB
    return c * (c * (c * 0.305306011 + 0.682171111) + 0.012522878);
#else
    half3 linearRGBLo = c / 12.92;
    half3 linearRGBHi = PositivePow( (c + 0.055) / 1.055, half3(2.4, 2.4, 2.4));
    half3 linearRGB = (c <= 0.04045) ? linearRGBLo : linearRGBHi;
    return linearRGB;
#endif
}
half4 SRGBToLinear(half4 c)
{
    return half4( SRGBToLinear( c.rgb), c.a);
}
half LinearToSRGB( half c)
{
#if USE_VERY_FAST_SRGB
    return sqrt( c);
#elif USE_FAST_SRGB
    return max( 1.055 * PositivePow( c, 0.416666667) - 0.055, 0.0);
#else
    half sRGBLo = c * 12.92;
    half sRGBHi = (PositivePow( c, 1.0 / 2.4) * 1.055) - 0.055;
    half sRGB = (c <= 0.0031308) ? sRGBLo : sRGBHi;
    return sRGB;
#endif
}
half3 LinearToSRGB( half3 c)
{
#if USE_VERY_FAST_SRGB
    return sqrt( c);
#elif USE_FAST_SRGB
    return max( 1.055 * PositivePow( c, 0.416666667) - 0.055, 0.0);
#else
    half3 sRGBLo = c * 12.92;
    half3 sRGBHi = (PositivePow( c, half3( 1.0 / 2.4, 1.0 / 2.4, 1.0 / 2.4)) * 1.055) - 0.055;
    half3 sRGB = (c <= 0.0031308) ? sRGBLo : sRGBHi;
    return sRGB;
#endif
}
half4 LinearToSRGB( half4 c)
{
    return half4( LinearToSRGB( c.rgb), c.a);
}

half3 ApplyLut2D( sampler2D tex, float3 uvw, float3 scaleOffset)
{
    // Strip format where `height = sqrt(width)`
    uvw.z *= scaleOffset.z;
    float shift = floor( uvw.z);
    uvw.xy = uvw.xy * scaleOffset.z * scaleOffset.xy + scaleOffset.xy * 0.5;
    uvw.x += shift * scaleOffset.y;
    uvw.xyz = lerp(
        tex2D( tex, uvw.xy).rgb,
        tex2D( tex, uvw.xy + float2( scaleOffset.y, 0.0)).rgb,
        uvw.z - shift
    );
    return uvw;
}

float3 GetLutStripValue( float2 uv, float4 params)
{
    uv -= params.yz;
    float3 color;
    color.r = frac( uv.x * params.x);
    color.b = uv.x - color.r / params.x;
    color.g = uv.y;
    return color * params.w;
}
float3 WhiteBalance(float3 c, float3 balance)
{
    float3 lms = mul( LIN_2_LMS_MAT, c);
    lms *= balance;
    return mul( LMS_2_LIN_MAT, lms);
}
float3 RgbToHsv( float3 c)
{
    float4 K = float4( 0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    float4 p = lerp( float4( c.bg, K.wz), float4( c.gb, K.xy), step( c.b, c.g));
    float4 q = lerp( float4( p.xyw, c.r), float4( c.r, p.yzx), step( p.x, c.r));
    float d = q.x - min( q.w, q.y);
    float e = EPSILON;
    return float3( abs( q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}
float3 HsvToRgb(float3 c)
{
    float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs( frac( c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * lerp( K.xxx, saturate( p - K.xxx), c.y);
}
float RotateHue( float value, float low, float hi)
{
    return (value < low)
            ? value + hi
            : (value > hi)
                ? value - hi
                : value;
}
float3 Saturation( float3 c, float sat)
{
    float luma = Luminance( c);
    return luma.xxx + sat.xxx * (c - luma.xxx);
}
float3 Contrast( float3 c, float midpoint, float contrast)
{
    return (c - midpoint) * contrast + midpoint;
}
float3 LiftGammaGainHDR(float3 c, float3 lift, float3 invgamma, float3 gain)
{
    c = c * gain + lift;

    return FastSign( c) * pow( abs( c), invgamma);
}
float3 YrgbCurve( float3 c, sampler2D curveTex)
{
    const float kHalfPixel = (1.0 / 128.0) / 2.0;

    // Y (master)
    c += kHalfPixel.xxx;
    float mr = tex2D( curveTex, float2( c.r, 0.75)).a;
    float mg = tex2D( curveTex, float2( c.g, 0.75)).a;
    float mb = tex2D( curveTex, float2( c.b, 0.75)).a;
    c = saturate( float3( mr, mg, mb));

    // RGB
    c += kHalfPixel.xxx;
    float r = tex2D( curveTex, float2( c.r, 0.75)).r;
    float g = tex2D( curveTex, float2( c.g, 0.75)).g;
    float b = tex2D( curveTex, float2( c.b, 0.75)).b;
    return saturate( float3( r, g, b));
}
float3 ChannelMixer( float3 c, float3 red, float3 green, float3 blue)
{
    return float3(
        dot( c, red),
        dot( c, green),
        dot( c, blue)
    );
}

#endif /* __COLORS_CGINC__ */
