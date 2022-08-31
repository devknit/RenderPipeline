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
	sampler2D _MaskTex;
	float4 _Pixelation;
	float _MipmapLevel;
	
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
	float4 frag0( VertexOutput i) : SV_Target 
	{
		return fixed4( 1, 1, 1, 1);
	}
	inline fixed4 mosaicTex2D( sampler2D mainTex, float2 uv)
	{
		return tex2D( mainTex, round( uv * _Pixelation.xy) * _Pixelation.zw);
	}
	float4 frag1( VertexOutput i) : SV_Target 
	{
		fixed4 baseColor = tex2D( _MainTex, i.uv);
		fixed4 mosaicColor = mosaicTex2D( _MainTex, i.uv);
		fixed mask = tex2Dlod( _MaskTex, float4( i.uv, 0, _MipmapLevel)).r;
		fixed t = 1.0 - step( mask, 0);
		return lerp( baseColor, mosaicColor, t);
	}
	float4 frag2( VertexOutput i) : SV_Target 
	{
		return mosaicTex2D( _MainTex, i.uv);
	}
	ENDCG
	
	SubShader
	{
		Cull Off ZWrite Off ZTest Always
		
		Pass
		{
			Stencil
			{
				Ref [_StencilRef]
				ReadMask [_StencilReadMask]
				Comp [_StencilComp]
			}
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag0
			ENDCG
		}
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag1
			ENDCG
		}
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag2
			ENDCG
		}
	}
}
