Shader "Hidden/RenderPipeline/Ubar"
{
	SubShader
	{
		Cull Off
		ZWrite Off
		ZTest Always
		Blend Off
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_local _ _FLIP_X
			#pragma multi_compile_local _ _FLIP_Y
			#pragma multi_compile_local _ _LENSDISTORTION
			#pragma multi_compile_local _ _CHROMATICABERRATION
			#pragma multi_compile_local _ _CHROMATICABERRATION_FASTMODE
			#pragma multi_compile_local _ _NOISE
			#pragma multi_compile_local _ _VIGNETTE
			#include "../Ubar/LensDistortion/Shaders/LensDistortion.cginc"
			#include "../Ubar/Vignette/Shaders/Vignette.cginc"
			#include "../Ubar/Noise/Shaders/Noise.cginc"
			
			UNITY_DECLARE_SCREENSPACE_TEXTURE( _MainTex);
			float4 _MainTex_TexelSize;
		#if defined(_CHROMATICABERRATION)
			sampler2D _ChromaticAberrationSpectralLut;
			half _ChromaticAberrationIntensity;
		#endif
			struct VertexInput
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			struct VertexOutput
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			void vert( VertexInput i, out VertexOutput o)
			{
				UNITY_SETUP_INSTANCE_ID( i);
				UNITY_INITIALIZE_OUTPUT(VertexOutput, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o);
				o.pos = UnityObjectToClipPos( i.pos);
				o.uv = UnityStereoTransformScreenSpaceTex( i.uv);
			#if defined(_FLIP_X)
				o.uv.x = 1.0 - o.uv.x;
			#endif
			#if defined(_FLIP_Y)
				o.uv.y = 1.0 - o.uv.y;
			#endif
			}
			fixed4 frag( VertexOutput i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i);
				float2 uv = LensDistortion( i.uv);
				fixed4 color = fixed4( 0, 0, 0, 0);
				
		#if defined(_CHROMATICABERRATION)
			#if defined(_CHROMATICABERRATION_FASTMODE)
				float2 coords = uv * 2.0 - 1.0;
			    float2 delta = ((uv - coords * dot( coords, coords) * _ChromaticAberrationIntensity) - uv) / 3.0;
			    half4 filterA = half4( tex2Dlod( _ChromaticAberrationSpectralLut, float4( 0.5 / 3, 0, 0, 0)).rgb, 1);
			    half4 filterB = half4( tex2Dlod( _ChromaticAberrationSpectralLut, float4( 1.5 / 3, 0, 0, 0)).rgb, 1);
			    half4 filterC = half4( tex2Dlod( _ChromaticAberrationSpectralLut, float4( 2.5 / 3, 0, 0, 0)).rgb, 1);
			    half4 texelA = tex2Dlod( _MainTex, float4( UnityStereoTransformScreenSpaceTex( LensDistortion( uv)), 0, 0));
			    half4 texelB = tex2Dlod( _MainTex, float4( UnityStereoTransformScreenSpaceTex( LensDistortion( delta + uv)), 0, 0));
			    half4 texelC = tex2Dlod( _MainTex, float4( UnityStereoTransformScreenSpaceTex( LensDistortion( delta * 2 + uv)), 0, 0));
			    color = (texelA * filterA + texelB * filterB + texelC * filterC) / (filterA + filterB + filterC);
			#else
				float2 coords = uv * 2.0 - 1.0;
                float2 diff = (uv - coords * dot( coords, coords) * _ChromaticAberrationIntensity) - uv;
                int samples = clamp( int( length( _MainTex_TexelSize.zw * diff / 2.0)), 3, 16);
                float2 delta = diff / samples;
                
                half4 colorSum = 0, filterSum = 0;
                coords = uv;

                for( int i0 = 0; i0 < samples; ++i0)
                {
					half4 filter = half4( tex2Dlod( _ChromaticAberrationSpectralLut, float4( (i0 + 0.5h) / samples, 0, 0, 0)).rgb, 1);
                    colorSum += tex2Dlod( _MainTex, float4( UnityStereoTransformScreenSpaceTex( LensDistortion( coords)), 0, 0)) * filter;
                    filterSum += filter;
                    coords += delta;
                }
                color = colorSum / filterSum;
			#endif
		#else
				color = UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, uv);	
		#endif
			#if defined(_NOISE)
				color = Noise( color, uv);
			#endif
			#if defined(_VIGNETTE)
				color = Vignette( color, uv);
			#endif
				return color;
			}
			ENDCG
		}
	}
}
