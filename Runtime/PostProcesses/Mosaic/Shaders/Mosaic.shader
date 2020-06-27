Shader "Hidden/RenderPipeline/Mosaic"
{
	Properties
	{
	//	_MainTex( "Texture", 2D) = "white" {}
		_StencilRef( "Stencil Reference", Range( 0, 255)) = 1
		_StencilReadMask( "Stencil Read Mask", Range( 0, 255)) = 255
		[Enum( UnityEngine.Rendering.CompareFunction)]
		_StencilComp( "Stencil Comparison Function", float) = 3	/* Equal */
	}
	CGINCLUDE
	
	#include "UnityCG.cginc"
	
	sampler2D _MainTex;
	float4 _MainTex_TexelSize;
	float4 _Pixelation;
	
	struct v2f
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};
	void vert( appdata_img v, out v2f o)
	{
		o.pos = UnityObjectToClipPos( v.vertex);
		o.uv = v.texcoord.xy;
	}
	float4 frag( v2f i) : SV_Target 
	{
        return tex2D( _MainTex, float2(
        	floor( i.uv.x * _Pixelation.x) * _Pixelation.z, 
        	floor( i.uv.y * _Pixelation.y) * _Pixelation.w));
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
			#pragma fragment frag
			ENDCG
		}
	}
}
