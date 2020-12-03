Shader "Hidden/RenderPipeline/Mosaic"
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
	
	sampler2D _MainTex;
	float4 _Pixelation;
	
	struct VertexOutput
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};
	void vert( appdata_img v, out VertexOutput o)
	{
		o.pos = UnityObjectToClipPos( v.vertex);
		o.uv = v.texcoord.xy;
	}
	float4 frag( VertexOutput i) : SV_Target 
	{
		return tex2D( _MainTex, round( i.uv.xy * _Pixelation.xy) * _Pixelation.zw);
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
