
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderingPipeline
{
	public static class DepthStencil
	{
		public static readonly long kDefaultHash = GetHashCode();
		
		public static long GetHashCode( 
			byte reference = 0, 
			byte readMask = 255, 
			byte writeMask = 255, 
			CompareFunction compFunc = CompareFunction.Always, 
			StencilOp passOp = StencilOp.Keep, 
			StencilOp failOp = StencilOp.Keep, 
			StencilOp zFailOp = StencilOp.Keep)
		{
			return (long)reference << 48 | (long)readMask << 40 | (long)writeMask << 32 | (long)compFunc << 24 | (long)passOp << 16 | (long)failOp << 8 | (long)zFailOp;
		}
		public static bool HasIndependent( long hash)
		{
			return (kDefaultHash & 0xffffffff) != (hash & 0xffffffff);
		}
		public static byte GetDepthStencilReference( long hash)
		{
			return (byte)((hash >> 48) & 0xff);
		}
		public static byte GetDepthStencilReadMask( long hash)
		{
			return (byte)((hash >> 40) & 0xff);
		}
		public static byte GetDepthStencilWriteMask( long hash)
		{
			return (byte)((hash >> 32) & 0xff);
		}
		public static CompareFunction GetDepthStencilCompareFunction( long hash)
		{
			return (CompareFunction)((hash >> 24) & 0xff);
		}
		public static StencilOp GetDepthStencilPassOp( long hash)
		{
			return (StencilOp)((hash >> 16) & 0xff);
		}
		public static StencilOp GetDepthStencilFailOp( long hash)
		{
			return (StencilOp)((hash >> 8) & 0xff);
		}
		public static StencilOp GetDepthStencilZFailOp( long hash)
		{
			return (StencilOp)(hash & 0xff);
		}
	}
}
