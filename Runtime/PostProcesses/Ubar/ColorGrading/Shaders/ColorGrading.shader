Shader "Hidden/RenderPipeline/ColorGrading"
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
			#include "Colors.cginc"
			
			sampler2D _MainTex;
			float4 _Lut2D_Params;
			
			float3 _ColorBalance;
	        float3 _ColorFilter;
	        float3 _HueSatCon;
	        float _Brightness; // LDR only
	        
	        float3 _ChannelMixerRed;
	        float3 _ChannelMixerGreen;
	        float3 _ChannelMixerBlue;
	        
	        float3 _Lift;
	        float3 _InvGamma;
	        float3 _Gain;
			
			sampler2D _Curves;
			
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
			}
			
			float3 ApplyCommonGradingSteps( float3 colorLinear)
	        {
	            colorLinear = WhiteBalance( colorLinear, _ColorBalance);
	            colorLinear *= _ColorFilter;
	            colorLinear = ChannelMixer( colorLinear, _ChannelMixerRed, _ChannelMixerGreen, _ChannelMixerBlue);
	            colorLinear = LiftGammaGainHDR( colorLinear, _Lift, _InvGamma, _Gain);
	            colorLinear = max( 0.0, colorLinear);
	            float3 hsv = RgbToHsv( colorLinear);
	            
	            float satMult = saturate( tex2Dlod( _Curves, float4( hsv.x, 0.25, 0, 0)).y) * 2.0;
	            satMult *= saturate( tex2Dlod( _Curves, float4( hsv.y, 0.25, 0, 0)).z) * 2.0;
	            satMult *= saturate( tex2Dlod( _Curves, float4( Luminance( colorLinear), 0.25, 0, 0)).w) * 2.0;

	            float hue = hsv.x + _HueSatCon.x;
	            float offset = saturate( tex2Dlod( _Curves, float4( hue, 0.25, 0, 0)).x) - 0.5;
	            hue += offset;
	            hsv.x = RotateHue( hue, 0.0, 1.0);

	            colorLinear = HsvToRgb(hsv);
	            colorLinear = Saturation( colorLinear, _HueSatCon.y * satMult);

	            return colorLinear;
	        }
			float3 ColorGradeLDR( float3 colorLinear)
			{
				colorLinear *= _Brightness;
				
				const float kMidGrey = pow( 0.5, 2.2);
          		colorLinear = Contrast( colorLinear, kMidGrey, _HueSatCon.z);
          		colorLinear = ApplyCommonGradingSteps( colorLinear);
          		colorLinear = YrgbCurve( saturate( colorLinear), _Curves);
          		return saturate( colorLinear);
			}
			fixed4 frag( VertexOutput i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i);
				float3 colorLinear = GetLutStripValue( i.uv, _Lut2D_Params);
				float3 graded = ColorGradeLDR( colorLinear);
				return fixed4( graded, 1);
			}
			ENDCG
		}
	}
}
