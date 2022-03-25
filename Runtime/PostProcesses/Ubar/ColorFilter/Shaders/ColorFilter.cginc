
#ifndef __COLORFILTER_CGINC__
#define __COLORFILTER_CGINC__

#include "UnityCG.cginc"

half4 _ColorFilterDot;
half4 _ColorFilterMultiply;
half4 _ColorFilterAdd;
half _ColorFilterInvert;

fixed4 ColorFilter( half4 color)
{
	color.rgb = lerp( color.rgb, dot( color.rgb, _ColorFilterDot.rgb), _ColorFilterDot.a);
	color.rgb = lerp( color.rgb, color.rgb * _ColorFilterMultiply.rgb, _ColorFilterMultiply.a);
	color.rgb = lerp( color.rgb, color.rgb + _ColorFilterAdd.rgb, _ColorFilterAdd.a);
	color = saturate( color);
	color.rgb = lerp( color.rgb, 1 - color.rgb, _ColorFilterInvert);
	return color;
}
#endif /* __COLORFILTER_CGINC__ */
