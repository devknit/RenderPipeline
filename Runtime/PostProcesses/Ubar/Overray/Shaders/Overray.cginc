
#ifndef __OVERRAY_CGINC__
#define __OVERRAY_CGINC__

#include "UnityCG.cginc"

sampler2D _OverrayTex;
fixed4 _OverrayColor;

inline fixed4 Overray( fixed4 color, float2 uv)
{
	fixed4 overrayColor = tex2D( _OverrayTex, uv) * _OverrayColor;
	return fixed4( lerp( color.rgb, overrayColor.rgb, overrayColor.a), 1);
}
#endif /* __OVERRAY_CGINC__ */
