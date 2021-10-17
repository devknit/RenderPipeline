
Shader "Hidden/RenderPipeline/MotionBlur"
{
	// Properties
	// {
	// 	_Samples( "Samples", Range( 4, 32)) = 16
	// 	_ShutterAngle( "Intensity", Range( 0, 360)) = 10.0
	// }
	CGINCLUDE
	#include "UnityCG.cginc"
	#define TWO_PI	6.28318530718
	
	uniform sampler2D _MainTex;
	uniform float4 _MainTex_TexelSize;
	uniform sampler2D _CameraDepthTexture;
	uniform sampler2D _CameraMotionVectorsTexture;
	uniform float4 _CameraMotionVectorsTexture_TexelSize;
	uniform sampler2D _VelocityTex;
	uniform float2 _VelocityTex_TexelSize;
	uniform sampler2D _NeighborTex;
	uniform float2 _NeighborTex_TexelSize;
	
	// Velocity scale factor
	uniform float _VelocityScale;
	
	// Maximum blur radius (in pixels)
	uniform half _MaxBlurRadius;
	uniform float _RcpMaxBlurRadius;
	
	// TileMax filter parameters
	int _TileMaxLoop;
	float2 _TileMaxOffs;
	
	// Filter parameters/coefficients
	half _LoopCount;
	
	struct VertexInput
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};
	struct VertexOutput
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};
	void vert( VertexInput v, out VertexOutput o)
	{
		UNITY_INITIALIZE_OUTPUT( VertexOutput, o);
		o.pos = UnityObjectToClipPos( v.vertex);
		o.uv = v.uv;
	#if UNITY_UV_STARTS_AT_TOP
		// o.uv = o.uv * float2( 1.0, -1.0) + float2( 0.0, 1.0);
	#endif
	}
	half4 fragVelocity( VertexOutput i) : SV_Target
	{
		float2 v = tex2D( _CameraMotionVectorsTexture, i.uv).rg;
		half d = Linear01Depth( tex2D( _CameraDepthTexture, i.uv));
		
		v *= (_VelocityScale * 0.5) * _CameraMotionVectorsTexture_TexelSize.zw;
		v /= max( 1.0, length( v) * _RcpMaxBlurRadius);
		
		return half4((v * _RcpMaxBlurRadius + 1.0) * 0.5, d, 0.0);
	}
	inline half2 maxV( half2 v1, half2 v2)
	{
		return dot( v1, v1) < dot( v2, v2) ? v2 : v1;
	}
	half4 fragTileMax1( VertexOutput i) : SV_Target
	{
		float4 d = _MainTex_TexelSize.xyxy * float4( -0.5, -0.5, 0.5, 0.5);
		half2 v1 = (tex2D( _MainTex, i.uv + d.xy).rg * 2.0 - 1.0) * _MaxBlurRadius;
		half2 v2 = (tex2D( _MainTex, i.uv + d.zy).rg * 2.0 - 1.0) * _MaxBlurRadius;
		half2 v3 = (tex2D( _MainTex, i.uv + d.xw).rg * 2.0 - 1.0) * _MaxBlurRadius;
		half2 v4 = (tex2D( _MainTex, i.uv + d.zw).rg * 2.0 - 1.0) * _MaxBlurRadius;
		return half4( maxV( maxV( maxV( v1, v2), v3), v4), 0.0, 0.0);
	}
	half4 fragTileMax2( VertexOutput i) : SV_Target
	{
		float4 d = _MainTex_TexelSize.xyxy * float4( -0.5, -0.5, 0.5, 0.5);
		half2 v1 = tex2D( _MainTex, i.uv + d.xy).rg;
		half2 v2 = tex2D( _MainTex, i.uv + d.zy).rg;
		half2 v3 = tex2D( _MainTex, i.uv + d.xw).rg;
		half2 v4 = tex2D( _MainTex, i.uv + d.zw).rg;
		return half4( maxV( maxV( maxV( v1, v2), v3), v4), 0.0, 0.0);
	}
	half4 fragTileMaxV( VertexOutput i) : SV_Target
	{
		float2 uv = i.uv + _MainTex_TexelSize.xy * _TileMaxOffs.xy;
		float2 du = float2( _MainTex_TexelSize.x, 0.0);
		float2 dv = float2( 0.0, _MainTex_TexelSize.y);
		half2 vo = 0.0;
		
		UNITY_LOOP
		for( int ix = 0; ix < _TileMaxLoop; ++ix)
		{
			UNITY_LOOP
			for( int iy = 0; iy < _TileMaxLoop; ++iy)
			{
				vo = maxV( vo, tex2D( _MainTex, uv + du * ix + dv * iy).rg);
			}
		}
		return half4( vo, 0.0, 0.0);
	}
	half4 fragNeighborMax( VertexOutput i) : SV_Target
	{
		const half cw = 1.01; // Center weight tweak
		
		float4 d = _MainTex_TexelSize.xyxy * float4( 1.0, 1.0, -1.0, 0.0);
		
		half2 v1 = tex2D( _MainTex, i.uv - d.xy).rg;
		half2 v2 = tex2D( _MainTex, i.uv - d.wy).rg;
		half2 v3 = tex2D( _MainTex, i.uv - d.zy).rg;
		
		half2 v4 = tex2D( _MainTex, i.uv - d.xw).rg;
		half2 v5 = tex2D( _MainTex, i.uv).rg * cw;
		half2 v6 = tex2D( _MainTex, i.uv + d.xw).rg;
		
		half2 v7 = tex2D( _MainTex, i.uv + d.zy).rg;
		half2 v8 = tex2D( _MainTex, i.uv + d.wy).rg;
		half2 v9 = tex2D( _MainTex, i.uv + d.xy).rg;
		
		half2 va = maxV( v1, maxV( v2, v3));
		half2 vb = maxV( v4, maxV( v5, v6));
		half2 vc = maxV( v7, maxV( v8, v9));
		
		return half4( maxV( va, maxV( vb, vc)) * (1.0 / cw), 0.0, 0.0);
	}
	half3 sampleVelocity( float2 uv)
	{
		half3 v = tex2Dlod( _VelocityTex, float4( uv, 0, 0)).xyz;
		return half3((v.xy * 2.0 - 1.0) * _MaxBlurRadius, v.z);
	}
	float gradientNoise(float2 uv)
	{
		uv = floor( uv * _ScreenParams.xy);
		float f = dot( float2( 0.06711056, 0.00583715), uv);
		return frac( 52.9829189 * frac( f));
	}
	float2 jitterTile( float2 uv)
	{
		float rx, ry;
		sincos( gradientNoise(uv + float2(2.0, 0.0)) * TWO_PI, ry, rx);
		return float2( rx, ry) * _NeighborTex_TexelSize.xy * 0.25;
	}
	inline bool interval( half phase, half interval)
	{
		return frac( phase / interval) > 0.499;
	}
	fixed4 frag( VertexOutput i) : SV_Target
	{
		// Color sample at the center point
		const half4 c_p = tex2D( _MainTex, i.uv);
		
		// Velocity/Depth sample at the center point
		const half3 vd_p = sampleVelocity( i.uv);
		const half l_v_p = max( length( vd_p.xy), 0.5);
		const half rcp_d_p = 1.0 / vd_p.z;
		
		// NeighborMax vector sample at the center point
		const half2 v_max = tex2D( _NeighborTex, i.uv + jitterTile( i.uv)).xy;
		const half l_v_max = length( v_max);
		const half rcp_l_v_max = 1.0 / l_v_max;
		
		// Escape early if the NeighborMax vector is small enough.
		if( l_v_max < 2.0)
		{
			return c_p;
		}
		
		// Use V_p as a secondary sampling direction except when it's too small
		// compared to V_max. This vector is rescaled to be the length of V_max.
		const half2 v_alt = (l_v_p * 2.0 > l_v_max) ? vd_p.xy * (l_v_max / l_v_p) : v_max;
		
		// Determine the sample count.
		const half sc = floor( min( _LoopCount, l_v_max * 0.5));
		
		// Loop variables (starts from the outermost sample)
		const half dt = 1.0 / sc;
		const half t_offs = (gradientNoise(i.uv) - 0.5) * dt;
		half t = 1.0 - dt * 0.5;
		half count = 0.0;
		
		// Background velocity
		// This is used for tracking the maximum velocity in the background layer.
		half l_v_bg = max( l_v_p, 1.0);
		
		// Color accumlation
		half4 acc = 0.0;
		
		UNITY_LOOP
		while( t > dt * 0.25)
		{
			// Sampling direction (switched per every two samples)
			const half2 v_s = interval( count, 4.0) ? v_alt : v_max;
			
			// Sample position (inverted per every sample)
			const half t_s = (interval( count, 2.0) ? -t : t) + t_offs;
			
			// Distance to the sample position
			const half l_t = l_v_max * abs( t_s);
			
			// UVs for the sample position
			const float2 uv0 = i.uv + v_s * t_s * _MainTex_TexelSize.xy;
			const float2 uv1 = i.uv + v_s * t_s * _VelocityTex_TexelSize.xy;
			
			// Color sample
			const half3 c = tex2Dlod( _MainTex, float4( uv0, 0, 0)).rgb;
			
			// Velocity/Depth sample
			const half3 vd = sampleVelocity( uv1);
			
			// Background/Foreground separation
			const half fg = saturate( (vd_p.z - vd.z) * 20.0 * rcp_d_p);
			
			// Length of the velocity vector
			const half l_v = lerp( l_v_bg, length( vd.xy), fg);
			
			// Sample weight
			// (Distance test) * (Spreading out by motion) * (Triangular window)
			const half w = saturate( l_v - l_t) / l_v * (1.2 - t);
			
			// Color accumulation
			acc += half4( c, 1.0) * w;
			
			// Update the background velocity.
			l_v_bg = max( l_v_bg, l_v);
			
			// Advance to the next sample.
			t = interval( count, 2.0) ? t - dt : t;
			count += 1.0;
		}
		// Add the center sample.
		acc += half4( c_p.rgb, 1.0) * (1.2 / (l_v_bg * sc * 2.0));
		
		return half4( acc.rgb / acc.a, c_p.a);
	}
	ENDCG
	
	SubShader
	{
		Cull Off
		ZWrite Off
		ZTest Always
			
		Pass
		{
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragVelocity
			ENDCG
		}
		Pass
		{
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragTileMax1
			ENDCG
		}
		Pass
		{
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragTileMax2
			ENDCG
		}
		Pass
		{
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragTileMaxV
			ENDCG
		}
		Pass
		{
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragNeighborMax
			ENDCG
		}
		Pass
		{
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	} 
	FallBack Off
}
