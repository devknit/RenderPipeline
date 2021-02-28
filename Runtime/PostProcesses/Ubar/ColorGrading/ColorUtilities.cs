﻿
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace RenderingPipeline
{
	public static class ColorUtilities
	{
		public static float StandardIlluminantY(float x)
		{
			return 2.87f * x - 3.0f * x * x - 0.27509507f;
		}
		public static Vector3 CIExyToLMS(float x, float y)
		{
			float Y = 1.0f;
			float X = Y * x / y;
			float Z = Y * (1.0f - x - y) / y;
			
			float L = 0.7328f * X + 0.4296f * Y - 0.1624f * Z;
			float M = -0.7036f * X + 1.6975f * Y + 0.0061f * Z;
			float S = 0.0030f * X + 0.0136f * Y + 0.9834f * Z;
			
			return new Vector3(L, M, S);
		}
		public static Vector3 ComputeColorBalance( float temperature, float tint)
		{
			float t1 = temperature / 60f;
			float t2 = tint / 60f;
			
			float x = 0.31271f - t1 * (t1 < 0.0f ? 0.1f : 0.05f);
			float y = StandardIlluminantY( x) + t2 * 0.05f;
			
			var w1 = new Vector3( 0.949237f, 1.03542f, 1.08728f);
			var w2 = CIExyToLMS( x, y);
			return new Vector3( w1.x / w2.x, w1.y / w2.y, w1.z / w2.z);
		}
		public static Vector3 ColorToLift( Vector4 color)
		{
			var S = new Vector3( color.x, color.y, color.z);
			float lumLift = S.x * 0.2126f + S.y * 0.7152f + S.z * 0.0722f;
			S = new Vector3( S.x - lumLift, S.y - lumLift, S.z - lumLift);
			
			float liftOffset = color.w;
			return new Vector3( S.x + liftOffset, S.y + liftOffset, S.z + liftOffset);
		}
		public static Vector3 ColorToInverseGamma(Vector4 color)
		{
			var M = new Vector3( color.x, color.y, color.z);
			float lumGamma = M.x * 0.2126f + M.y * 0.7152f + M.z * 0.0722f;
			M = new Vector3( M.x - lumGamma, M.y - lumGamma, M.z - lumGamma);
			
			float gammaOffset = color.w + 1.0f;
			return new Vector3(
				1.0f / Mathf.Max( M.x + gammaOffset, 1e-03f),
				1.0f / Mathf.Max( M.y + gammaOffset, 1e-03f),
				1.0f / Mathf.Max( M.z + gammaOffset, 1e-03f)
			);
		}
		public static Vector3 ColorToGain( Vector4 color)
		{
			var H = new Vector3( color.x, color.y, color.z);
			float lumGain = H.x * 0.2126f + H.y * 0.7152f + H.z * 0.0722f;
			H = new Vector3( H.x - lumGain, H.y - lumGain, H.z - lumGain);
			
			float gainOffset = color.w + 1.0f;
			return new Vector3( H.x + gainOffset, H.y + gainOffset, H.z + gainOffset);
		}
	}
}
