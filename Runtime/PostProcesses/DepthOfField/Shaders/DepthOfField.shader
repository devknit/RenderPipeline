Shader "Hidden/RenderPipeline/DepthOfFieldHdr"
{
//	Properties
//	{
//		_MainTex( "-", 2D) = "black" {}
//	}

	CGINCLUDE
	#include "UnityCG.cginc"
	#define SCATTER_OVERLAP_SMOOTH (-0.265)
	
	struct v2f
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		float2 uv1 : TEXCOORD1;
	};
	
	uniform sampler2D _MainTex;
	uniform sampler2D _CameraDepthTexture;
	uniform sampler2D _LowRez;
	uniform float4 _CurveParams;
	uniform float4 _MainTex_TexelSize;
	half4 _MainTex_ST;
	uniform float4 _Offsets;

	half4 _CameraDepthTexture_ST;
	half4 _LowRez_ST;
	
	static const int SmallDiscKernelSamples = 12;
	static const float2 SmallDiscKernel[ SmallDiscKernelSamples] =
	{
		float2( -0.326212, -0.40581),
		float2( -0.840144, -0.07358),
		float2( -0.695914, 0.457137),
		float2( -0.203345, 0.620716),
		float2( 0.96234, -0.194983),
		float2( 0.473434, -0.480026),
		float2( 0.519456, 0.767022),
		float2( 0.185461, -0.893124),
		float2( 0.507431, 0.064425),
		float2( 0.89642, 0.412458),
		float2( -0.32194, -0.932615),
		float2( -0.791559, -0.59771)
	};
	static const int NumDiscSamples = 28;
	static const float3 DiscKernel[ NumDiscSamples] = 
	{
		float3( 0.62463, 0.54337, 0.82790),
		float3( -0.13414, -0.94488, 0.95435),
		float3( 0.38772, -0.43475, 0.58253),
		float3( 0.12126, -0.19282, 0.22778),
		float3( -0.20388, 0.11133, 0.23230),
		float3( 0.83114, -0.29218, 0.88100),
		float3( 0.10759, -0.57839, 0.58831),
		float3( 0.28285, 0.79036, 0.83945),
		float3( -0.36622, 0.39516, 0.53876),
		float3( 0.75591, 0.21916, 0.78704),
		float3( -0.52610, 0.02386, 0.52664),
		float3( -0.88216, -0.24471, 0.91547),
		float3( -0.48888, -0.29330, 0.57011),
		float3( 0.44014, -0.08558, 0.44838),
		float3( 0.21179, 0.51373, 0.55567),
		float3( 0.05483, 0.95701, 0.95858),
		float3( -0.59001, -0.70509, 0.91938),
		float3( -0.80065, 0.24631, 0.83768),
		float3( -0.19424, -0.18402, 0.26757),
		float3( -0.43667, 0.76751, 0.88304),
		float3( 0.21666, 0.11602, 0.24577),
		float3( 0.15696, -0.85600, 0.87027),
		float3( -0.75821, 0.58363, 0.95682),
		float3( 0.99284, -0.02904, 0.99327),
		float3( -0.22234, -0.57907, 0.62029),
		float3( 0.55052, -0.66984, 0.86704),
		float3( 0.46431, 0.28115, 0.54280),
		float3( -0.07214, 0.60554, 0.60982),
	};
	inline float BokehWeightDisc( float4 theSample, float sampleDistance, float4 centerSample)
	{
		return smoothstep( SCATTER_OVERLAP_SMOOTH, 0.0, theSample.a - centerSample.a * sampleDistance); 
	}
	inline float2 BokehWeightDisc2( float4 sampleA, float4 sampleB, float2 sampleDistance2, float4 centerSample)
	{
		return smoothstep( float2( SCATTER_OVERLAP_SMOOTH, SCATTER_OVERLAP_SMOOTH), float2( 0.0, 0.0), float2( sampleA.a, sampleB.a) - centerSample.aa * sampleDistance2);
	}
	inline half3 BlendLowWithHighMQ( half coc, half3 low, half3 high)
	{
		float blend = smoothstep( 0.4, 0.6, coc);
		return lerp( low, high, blend);
	}
	inline half3 BlendLowWithHighHQ( half coc, half3 low, half3 high)
	{
		float blend = smoothstep( 0.65, 0.85, coc);
		return lerp( low, high, blend);
	}
	void vert( appdata_img v, out v2f o)
	{
		o.pos = UnityObjectToClipPos( v.vertex);
		o.uv1.xy = v.texcoord.xy;
		o.uv.xy = v.texcoord.xy;
	#if UNITY_UV_STARTS_AT_TOP
		o.uv.y = (_MainTex_TexelSize.y < 0)? 1.0 - o.uv.y : o.uv.y;
	#endif			
	}
	fixed4 fragCoC( v2f i) : SV_Target 
	{	
		fixed4 color = tex2D( _MainTex, i.uv1.xy);
		half d = Linear01Depth( SAMPLE_DEPTH_TEXTURE( 
			_CameraDepthTexture, UnityStereoScreenSpaceUVAdjust( i.uv1.xy, _CameraDepthTexture_ST)));
		color.a = _CurveParams.z * abs( d - _CurveParams.w) / (d + 1e-5f); 
		color.a = clamp( max( 0.0, color.a - _CurveParams.y), 0.0, _CurveParams.x);
		return color;
	}
	float4 fragPrefilter( v2f i) : SV_Target 
	{
		float4 tap  =  tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( i.uv.xy, _MainTex_ST));
	#if 0
		float4 tapA =  tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( i.uv.xy + 0.75 * _MainTex_TexelSize.xy, _MainTex_ST));
		float4 tapB =  tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( i.uv.xy - 0.75 * _MainTex_TexelSize.xy, _MainTex_ST));
		float4 tapC =  tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( i.uv.xy + 0.75 * _MainTex_TexelSize.xy * float2( 1, -1), _MainTex_ST));
		float4 tapD =  tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( i.uv.xy - 0.75 * _MainTex_TexelSize.xy * float2( 1, -1), _MainTex_ST));
		
		float4 weights = saturate( 10.0 * float4( tapA.a, tapB.a, tapC.a, tapD.a));
		float sumWeights = dot( weights, 1);

		float4 color = tapA * weights.x + tapB * weights.y + tapC * weights.z + tapD * weights.w;

		tap.rgb = (tap.a * sumWeights * 8.0 > 1e-5f)? color.rgb / sumWeights : tap.rgb;
	#endif
		return tap;
	}
	half4 fragBlurInsaneMQ( v2f i) : SV_Target 
	{
		half4 centerTap = tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( i.uv1.xy, _MainTex_ST));
		half4 poissonScale = _MainTex_TexelSize.xyxy * centerTap.a * _Offsets.y;
		half sampleCount = max( centerTap.a * 0.25, _Offsets.x);
		half3 sum = centerTap.rgb * sampleCount;
		
		for( int i0 = 0; i0 < NumDiscSamples; ++i0)
		{
			float3 discKernel = DiscKernel[ i0];
			float2 sampleUV = i.uv1.xy + discKernel.xy * poissonScale.xy;
			half4 sample0 = tex2D(_MainTex, UnityStereoScreenSpaceUVAdjust( sampleUV.xy, _MainTex_ST));
			half weights = (sample0.a > 0.0)? BokehWeightDisc( sample0, discKernel.z, centerTap) : 0.0;
			sum = sample0.rgb * weights + sum;
			sampleCount += weights;
		}
		return half4( sum / sampleCount, centerTap.a);
	}
	half4 fragBlurInsaneHQ( v2f i) : SV_Target 
	{
		half4 centerTap = tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( i.uv1.xy, _MainTex_ST));
		half4 poissonScale = _MainTex_TexelSize.xyxy * centerTap.a * _Offsets.y;
		half sampleCount = max( centerTap.a * 0.25, _Offsets.x);
		half3 sum = centerTap.rgb * sampleCount;
		
		for( int i0 = 0; i0 < NumDiscSamples; ++i0)
		{
			float3 discKernel = DiscKernel[ i0];
			float4 sampleUV = i.uv1.xyxy + discKernel.xyxy * poissonScale.xyxy / half4( 1.2, 1.2, discKernel.zz);
			half4 sample0 = tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( sampleUV.xy, _MainTex_ST));
			half4 sample1 = tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( sampleUV.zw, _MainTex_ST));
			half2 weights = (sample0.a + sample1.a > 0.0)? BokehWeightDisc2( sample0, sample1, half2( discKernel.z / 1.2, 1.0), centerTap) : half2( 0.0, 0.0);
			sum = sample0.rgb * weights.x + sample1.rgb * weights.y + sum; 
			sampleCount += dot( weights, 1); 
		}
		return half4( sum / sampleCount, centerTap.a);
	}
	half4 fragCombine( v2f i) : SV_Target 
	{
		half4 dof = tex2D( _LowRez, UnityStereoScreenSpaceUVAdjust( i.uv1.xy, _LowRez_ST));
		half4 color = tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( i.uv1.xy, _MainTex_ST));
		return fixed4( lerp( color.rgb, dof.rgb, (color.a + dof.a) * 0.5), 1);
	}
	float4 fragBlurUpsampleCombineMQ( v2f i) : SV_Target 
	{
		half4 centerTap = tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( i.uv1.xy, _MainTex_ST));
		half4 bigBlur = tex2D( _LowRez, UnityStereoScreenSpaceUVAdjust( i.uv1.xy, _LowRez_ST));
		half4 poissonScale = _MainTex_TexelSize.xyxy * centerTap.a * _Offsets.z;
		half sampleCount = max( centerTap.a * 0.25, 0.1);
		half3 smallBlur = centerTap.rgb * sampleCount;
		
		for( int i0 = 0; i0 < SmallDiscKernelSamples; ++i0)
		{
			float2 discKernel = SmallDiscKernel[ i0];
			float2 sampleUV = i.uv1.xy + discKernel.xy * poissonScale.xy * 1.1;
			half4 sample0 = tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( sampleUV, _MainTex_ST));
			half weight0 = BokehWeightDisc( sample0, length( discKernel.xy * 1.1), centerTap);
			smallBlur = sample0.rgb * weight0 + smallBlur;
			sampleCount += weight0;
		}

		smallBlur /= (sampleCount + 1e-5);
		smallBlur = BlendLowWithHighMQ( centerTap.a, smallBlur, bigBlur.rgb);
		
		return centerTap.a < 1e-2 ? centerTap : float4( smallBlur.rgb, centerTap.a);
	}
	float4 fragBlurUpsampleCombineHQ( v2f i) : SV_Target 
	{	
		half4 centerTap = tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( i.uv1.xy, _MainTex_ST));
		half4 bigBlur = tex2D( _LowRez, UnityStereoScreenSpaceUVAdjust( i.uv1.xy, _LowRez_ST));
		half4 poissonScale = _MainTex_TexelSize.xyxy * centerTap.a * _Offsets.z;
		half sampleCount = max( centerTap.a * 0.25, 0.1f);
		half3 smallBlur = centerTap * sampleCount;
		
		for( int i0 = 0; i0 < NumDiscSamples; ++i0)
		{
			float3 discKernel = DiscKernel[ i0];
			float2 sampleUV = i.uv1.xy + discKernel.xy * poissonScale.xy;
			half4 sample0 = tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( sampleUV, _MainTex_ST));
			half weight0 = BokehWeightDisc( sample0, discKernel.z, centerTap);
			smallBlur = sample0.rgb * weight0 + smallBlur;
			sampleCount += weight0;
		}

		smallBlur /= (sampleCount + 1e-5f);		
		smallBlur = BlendLowWithHighHQ( centerTap.a, smallBlur, bigBlur.rgb);

		return centerTap.a < 1e-2f ? centerTap : float4( smallBlur.rgb,centerTap.a);
	}
	fixed4 fragVisualize( v2f i) : SV_Target 
	{
		float d = Linear01Depth( SAMPLE_DEPTH_TEXTURE( 
			_CameraDepthTexture, UnityStereoScreenSpaceUVAdjust( i.uv1.xy, _CameraDepthTexture_ST)));
		d = _CurveParams.z * abs( d - _CurveParams.w) / (d + 1e-5f); 
		d = clamp( max( 0.0, d - _CurveParams.y), 0.0, _CurveParams.x);
		
		return fixed4( lerp( (0.0).xxx, (1.0).xxx, saturate( d / _CurveParams.x)), 1);
	}
	ENDCG
	
	Subshader
	{
		ZTest Always Cull Off ZWrite Off
		
		Pass
		{
			Name "0 CoC Calculation"
			
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragCoC
			ENDCG
		}
		Pass
		{ 
			Name "1 Prefilter"
			
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragPrefilter
			ENDCG
	  	}
		Pass
		{
			Name "2 Blur (medium)"
			
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragBlurInsaneMQ
			ENDCG
	  	}
		Pass
		{
			Name "3 Blur (high)"
			
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragBlurInsaneHQ
			ENDCG
	  	}
		Pass
		{
			Name "4 Combine"
			
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragCombine
			ENDCG
	  	}
		Pass
		{
			Name "5 Combine (medium)"
			
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragBlurUpsampleCombineMQ
			ENDCG
	  	}
		Pass
		{
			Name "6 Combine (high)"
			
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragBlurUpsampleCombineHQ
			ENDCG
	  	}
		Pass
		{
			Name "7 Visualize"
			
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragVisualize
			ENDCG
	  	}
	}
	Fallback Off
}
