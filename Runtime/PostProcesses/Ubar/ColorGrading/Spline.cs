
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace RenderingPipeline
{
	[System.Serializable]
	public sealed class Spline
	{
		public Spline( AnimationCurve curve, float zeroValue, bool loop, Vector2 bounds)
		{
			this.curve = curve;
			this.zeroValue = zeroValue;
			this.loop = loop;
			this.range = bounds.magnitude;
			cachedData = new float[ kPrecision];
		}
		public void Cache( int frame)
		{
			if( frame != frameCount)
			{
				var length = curve.length;
				
				if( loop && length > 1)
				{
					if( internalLoopingCurve == null)
					{
						internalLoopingCurve = new AnimationCurve();
					}
					var prev = curve[ length - 1];
					prev.time -= range;
					var next = curve[ 0];
					next.time += range;
					internalLoopingCurve.keys = curve.keys;
					internalLoopingCurve.AddKey( prev);
					internalLoopingCurve.AddKey( next);
				}
				for( int i0 = 0; i0 < kPrecision; ++i0)
				{
					cachedData[ i0] = Evaluate( (float)i0 * kStep, length);
				}
				
				frameCount = Time.renderedFrameCount;
			}
		}
		public float Evaluate( float t, int length)
		{
			if( length == 0)
			{
				return zeroValue;
			}
			if( loop == false || length == 1)
			{
				return curve.Evaluate( t);
			}
			return internalLoopingCurve.Evaluate( t);
		}
		public float Evaluate( float t)
		{
			return Evaluate( t, curve.length);
		}
		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + curve.GetHashCode();
				return hash;
			}
		}
		
		public const int kPrecision = 128;
		public const float kStep = 1.0f / kPrecision;
		
		[SerializeField]
		AnimationCurve curve;
		[SerializeField]
		float zeroValue;
		[SerializeField]
		bool loop;
		[SerializeField]
		float range;
		[SerializeField]
		internal float[] cachedData;
		
		int frameCount = -1;
		
		AnimationCurve internalLoopingCurve;
	}
}
