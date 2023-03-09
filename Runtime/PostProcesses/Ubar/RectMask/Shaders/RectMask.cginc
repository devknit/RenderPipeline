﻿
#ifndef __RECTMASK_CGINC__
#define __RECTMASK_CGINC__

#include "UnityCG.cginc"

fixed4 _RectMaskColor;
float4 _RectMaskRect;
float _RectMaskSmoothness;

fixed4 RectMask( fixed4 color, float2 uv)
{
	float o = _RectMaskSmoothness;
	float2 lb = _RectMaskRect.xy;
	float2 rt = 1.0 - (lb + _RectMaskRect.zw);
    float2 uvs = smoothstep( lb - o, lb + o, uv);
	uvs *= smoothstep( rt - o, rt + o, 1.0 - uv);
	half vfactor = uvs.x * uvs.y;
    return fixed4( color.rgb * lerp( _RectMaskColor.rgb, fixed3( 1, 1, 1), vfactor), saturate( color.a + (1.0 - vfactor)));
}
#endif /* __RECTMASK_CGINC__ */
