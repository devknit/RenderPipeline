Shader "Hidden/RenderPipeline/Antialiasing/NFAA"
{
	CGINCLUDE
	
	#include "UnityCG.cginc"
	
	uniform sampler2D _MainTex;
	uniform float4 _MainTex_TexelSize;
	uniform half4 _MainTex_ST;
	uniform float _OffsetScale;
	uniform float _BlurRadius;
	
	struct v2f
	{
		float4 pos : SV_POSITION;
		float4 uv01 : TEXCOORD0;
		float4 uv23 : TEXCOORD1;
		float4 uv45 : TEXCOORD2;
		float4 uv67 : TEXCOORD3;
	};
	void vert( appdata_img v, out v2f o)
	{
		o.pos = UnityObjectToClipPos (v.vertex);
		
		float2 uv = v.texcoord.xy;
		float2 up = float2( 0.0, _MainTex_TexelSize.y) * _OffsetScale;
		float2 right = float2( _MainTex_TexelSize.x, 0.0) * _OffsetScale;	
			
		o.uv01.xy = uv + up;
		o.uv01.zw = uv - up;
		o.uv23.xy = uv + right;
		o.uv23.zw = uv - right;
		o.uv45.xy = uv - right + up;
		o.uv45.zw = uv - right -up;
		o.uv67.xy = uv + right + up;
		o.uv67.zw = uv + right -up;
	}
	half4 frag( v2f i) : SV_Target
	{	
		// get luminance values
		//	maybe: experiment with different luminance calculations
		float topL = Luminance( tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( i.uv01.xy, _MainTex_ST)).rgb );
		float bottomL = Luminance( tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( i.uv01.zw, _MainTex_ST)).rgb );
		float rightL = Luminance( tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( i.uv23.xy, _MainTex_ST)).rgb );
		float leftL = Luminance( tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( i.uv23.zw, _MainTex_ST)).rgb );
		float leftTopL = Luminance( tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( i.uv45.xy, _MainTex_ST)).rgb );
		float leftBottomL = Luminance( tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( i.uv45.zw, _MainTex_ST)).rgb );
		float rightBottomL = Luminance( tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( i.uv67.xy, _MainTex_ST)).rgb );
		float rightTopL = Luminance( tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( i.uv67.zw, _MainTex_ST)).rgb );
		
		// 2 triangle subtractions
		float sum0 = dot( float3( 1, 1, 1), float3( rightTopL, bottomL, leftTopL));
		float sum1 = dot( float3( 1, 1, 1), float3( leftBottomL, topL, rightBottomL));
		float sum2 = dot( float3( 1, 1, 1), float3( leftTopL, rightL, leftBottomL));
		float sum3 = dot( float3( 1, 1, 1), float3( rightBottomL, leftL, rightTopL));

		// figure out "normal"
		float2 blurDir = half2( (sum0 - sum1), (sum3 - sum2));
		blurDir *= _MainTex_TexelSize.xy * _BlurRadius;

		// reconstruct normal uv
		float2 uv_ = (i.uv01.xy + i.uv01.zw) * 0.5;
		 
		float4 returnColor = tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( uv_, _MainTex_ST));
		returnColor += tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( uv_+ blurDir.xy, _MainTex_ST));
		returnColor += tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( uv_ - blurDir.xy, _MainTex_ST));
		returnColor += tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( uv_ + float2( blurDir.x, -blurDir.y), _MainTex_ST));
		returnColor += tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( uv_ - float2( blurDir.x, -blurDir.y), _MainTex_ST));

		return returnColor * 0.2;
	}
	half4 fragDebug( v2f i) : SV_Target
	{	
		// get luminance values
		//	maybe: experiment with different luminance calculations
		float topL = Luminance( tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( i.uv01.xy, _MainTex_ST)).rgb );
		float bottomL = Luminance( tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( i.uv01.zw, _MainTex_ST)).rgb );
		float rightL = Luminance( tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( i.uv23.xy, _MainTex_ST)).rgb );
		float leftL = Luminance( tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( i.uv23.zw, _MainTex_ST)).rgb );
		float leftTopL = Luminance( tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( i.uv45.xy, _MainTex_ST)).rgb );
		float leftBottomL = Luminance( tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( i.uv45.zw, _MainTex_ST)).rgb );
		float rightBottomL = Luminance( tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( i.uv67.xy, _MainTex_ST)).rgb );
		float rightTopL = Luminance( tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( i.uv67.zw, _MainTex_ST)).rgb );
		
		// 2 triangle subtractions
		float sum0 = dot( float3( 1, 1, 1), float3( rightTopL, bottomL, leftTopL));
		float sum1 = dot( float3( 1, 1, 1), float3( leftBottomL, topL, rightBottomL));
		float sum2 = dot( float3( 1, 1, 1), float3( leftTopL, rightL, leftBottomL));
		float sum3 = dot( float3( 1, 1, 1), float3( rightBottomL, leftL, rightTopL));

		// figure out "normal"
		float2 blurDir = half2( (sum0 - sum1), (sum3 - sum2));
		blurDir *= _MainTex_TexelSize.xy * _BlurRadius;

		// reconstruct normal uv
		float2 uv_ = (i.uv01.xy + i.uv01.zw) * 0.5;
		 
		float4 returnColor = tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( uv_, _MainTex_ST));
		returnColor += tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( uv_+ blurDir.xy, _MainTex_ST));
		returnColor += tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( uv_ - blurDir.xy, _MainTex_ST));
		returnColor += tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( uv_ + float2( blurDir.x, -blurDir.y), _MainTex_ST));
		returnColor += tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( uv_ - float2( blurDir.x, -blurDir.y), _MainTex_ST));

		blurDir = half2( (sum0 - sum1), (sum3 - sum2)) * _BlurRadius;
		return half4( normalize( half3( blurDir,1) * 0.5 + 0.5), 1);
	//	return returnColor * 0.2;
	}	
	ENDCG

	SubShader
	{
		Pass
		{
			ZTest Always Cull Off ZWrite Off
		
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma exclude_renderers d3d11_9x
			ENDCG
		}
		Pass
		{
			ZTest Always Cull Off ZWrite Off
		
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragDebug
			#pragma exclude_renderers d3d11_9x
			ENDCG
		}
	}
	Fallback Off
}
