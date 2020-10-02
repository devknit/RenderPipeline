Shader "Hidden/RenderPipeline/EdgeDetection"
{
	Properties
	{
		_StencilRef( "Stencil Reference", Range( 0, 255)) = 1
		_StencilReadMask( "Stencil Read Mask", Range( 0, 255)) = 255
		[Enum( UnityEngine.Rendering.CompareFunction)]
		_StencilComp( "Stencil Comparison Function", float) = 3	/* Equal */
	}
	CGINCLUDE
	
	#include "UnityCG.cginc"
	static const float4 kOne4 = float4( 1, 1, 1, 1);
	static const float4 kHorizDiagCoeff = float4( 1, 1, -1, -1);
	static const float4 kVertDiagCoeff = float4( -1, 1, -1, 1);
	static const float4 kHorizAxisCoeff = float4( 1, 0, 0, -1);
	static const float4 kVertAxisCoeff = float4( 0, 1, -1, 0);
	
	sampler2D _MainTex;
	float4 _MainTex_ST;
	float4 _MainTex_TexelSize;
	sampler2D _CameraDepthTexture;
	float4 _CameraDepthTexture_ST;
	fixed4 _EdgeColor;
	half _SampleDistance;
	
	struct VertexOutput
	{
		float4 pos : SV_POSITION;
		float2 uv0 : TEXCOORD0;
		float2 uv1 : TEXCOORD1;
	};
	void vert( appdata_img v, out VertexOutput o)
	{
		o.pos = UnityObjectToClipPos( v.vertex);
		o.uv0 = v.texcoord.xy;
	#if UNITY_UV_STARTS_AT_TOP
		o.uv1 = (_MainTex_TexelSize.y < 0)? 1.0 - v.texcoord.xy : v.texcoord.xy;
	#else
		o.uv1 = v.texcoord.xy;
	#endif
	}
	half4 cheap( VertexOutput i)
	{	
		float centerDepth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, UnityStereoScreenSpaceUVAdjust( i.uv1, _CameraDepthTexture_ST)));
		float4 depthsDiag;
		float4 depthsAxis;
		
		float2 uvDist = _SampleDistance * _MainTex_TexelSize.xy;

		depthsDiag.x = Linear01Depth( SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, UnityStereoScreenSpaceUVAdjust( i.uv1 + uvDist, _CameraDepthTexture_ST)));
		depthsDiag.y = Linear01Depth( SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, UnityStereoScreenSpaceUVAdjust( float2( -1, 1) * uvDist + i.uv1, _CameraDepthTexture_ST)));
		depthsDiag.z = Linear01Depth( SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, UnityStereoScreenSpaceUVAdjust( i.uv1 - uvDist * float2( -1, 1), _CameraDepthTexture_ST)));
		depthsDiag.w = Linear01Depth( SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, UnityStereoScreenSpaceUVAdjust( i.uv1 - uvDist, _CameraDepthTexture_ST)));

		depthsAxis.x = Linear01Depth( SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, UnityStereoScreenSpaceUVAdjust( float2( 0, 1) * uvDist + i.uv1, _CameraDepthTexture_ST)));
		depthsAxis.y = Linear01Depth( SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, UnityStereoScreenSpaceUVAdjust( i.uv1 - uvDist * float2( 1, 0), _CameraDepthTexture_ST)));
		depthsAxis.z = Linear01Depth( SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, UnityStereoScreenSpaceUVAdjust( float2( 1, 0) * uvDist + i.uv1, _CameraDepthTexture_ST)));
		depthsAxis.w = Linear01Depth( SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, UnityStereoScreenSpaceUVAdjust( i.uv1 - uvDist * float2( 0, 1), _CameraDepthTexture_ST)));

		depthsDiag -= centerDepth;
		depthsAxis /= centerDepth;

		float4 sobelH = depthsDiag * kHorizDiagCoeff + depthsAxis * kHorizAxisCoeff;
		float4 sobelV = depthsDiag * kVertDiagCoeff + depthsAxis * kVertAxisCoeff;

		float sobelX = dot( sobelH, kOne4);
		float sobelY = dot( sobelV, kOne4);
		float sobel = 1.0 - saturate( sqrt( sobelX * sobelX + sobelY * sobelY));

		return lerp( _EdgeColor, tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( i.uv0.xy, _MainTex_ST)), sobel);
	}
	half4 thin( VertexOutput i)
	{	
		float centerDepth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, UnityStereoScreenSpaceUVAdjust( i.uv1, _CameraDepthTexture_ST)));
		float4 depthsDiag;
		float4 depthsAxis;
		
		float2 uvDist = _SampleDistance * _MainTex_TexelSize.xy;

		depthsDiag.x = Linear01Depth( SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, UnityStereoScreenSpaceUVAdjust( i.uv1 + uvDist, _CameraDepthTexture_ST)));
		depthsDiag.y = Linear01Depth( SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, UnityStereoScreenSpaceUVAdjust( float2( -1, 1) * uvDist + i.uv1, _CameraDepthTexture_ST)));
		depthsDiag.z = Linear01Depth( SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, UnityStereoScreenSpaceUVAdjust( i.uv1 - uvDist * float2( -1, 1), _CameraDepthTexture_ST)));
		depthsDiag.w = Linear01Depth( SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, UnityStereoScreenSpaceUVAdjust( i.uv1 - uvDist, _CameraDepthTexture_ST)));

		depthsAxis.x = Linear01Depth( SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, UnityStereoScreenSpaceUVAdjust( float2( 0, 1) * uvDist + i.uv1, _CameraDepthTexture_ST)));
		depthsAxis.y = Linear01Depth( SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, UnityStereoScreenSpaceUVAdjust( i.uv1 - uvDist * float2( 1, 0), _CameraDepthTexture_ST)));
		depthsAxis.z = Linear01Depth( SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, UnityStereoScreenSpaceUVAdjust( float2( 1, 0) * uvDist + i.uv1, _CameraDepthTexture_ST)));
		depthsAxis.w = Linear01Depth( SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, UnityStereoScreenSpaceUVAdjust( i.uv1 - uvDist * float2( 0, 1), _CameraDepthTexture_ST)));

		depthsDiag = (depthsDiag > centerDepth.xxxx) ? depthsDiag : centerDepth.xxxx;
		depthsAxis = (depthsAxis > centerDepth.xxxx) ? depthsAxis : centerDepth.xxxx;

		depthsDiag -= centerDepth;
		depthsAxis /= centerDepth;

		float4 sobelH = depthsDiag * kHorizDiagCoeff + depthsAxis * kHorizAxisCoeff;
		float4 sobelV = depthsDiag * kVertDiagCoeff + depthsAxis * kVertAxisCoeff;

		float sobelX = dot( sobelH, kOne4);
		float sobelY = dot( sobelV, kOne4);
		float sobel = 1.0 - saturate( sqrt( sobelX * sobelX + sobelY * sobelY));
		
		return lerp( _EdgeColor, tex2D( _MainTex, UnityStereoScreenSpaceUVAdjust( i.uv0.xy, _MainTex_ST)), sobel);
	}
	half4 fragCheap( VertexOutput i) : COLOR
	{
		return cheap( i);
	}
	half4 fragThin( VertexOutput i) : COLOR
	{
		return thin( i);
	}
	ENDCG
	
	SubShader
	{
		Cull Off ZWrite Off ZTest Always
		
		Stencil
		{
			Ref [_StencilRef]
			ReadMask [_StencilReadMask]
			Comp [_StencilComp]
		}
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragCheap
			ENDCG
		}
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragThin
			ENDCG
		}
	}
}
