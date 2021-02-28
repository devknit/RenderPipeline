
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace RenderingPipeline
{
	public sealed partial class RenderPipeline : MonoBehaviour
	{
		void ResetTemporaryRT( CommandBuffer commandBuffer)
		{
			recycleTemporaries = new Dictionary<int, TemporaryTarget>();
			usedTemporaries = new Dictionary<int, TemporaryTarget>();
			currentCommandBuffer = commandBuffer;
			temporaryCount = 0;
		}
		void RecycleTemporaryRT( TargetContext context)
		{
			foreach( var userdTemporary in usedTemporaries.Values)
			{
				if( context.ConfirmUsePropertyId( userdTemporary.propertyId) == false)
				{
					if( recycleTemporaries.ContainsKey( userdTemporary.propertyId) == false)
					{
						recycleTemporaries.Add( userdTemporary.propertyId, userdTemporary);
					}
				}
			}
		}
		public int GetTemporaryRT( int width=-1, int height=-1, int depth=0, 
			FilterMode filterMode=FilterMode.Bilinear, RenderTextureFormat format=RenderTextureFormat.DefaultHDR)
		{
			foreach( var recycleTemporary in recycleTemporaries.Values)
			{
				if( recycleTemporary.width == width
				||	recycleTemporary.height == height
				||	recycleTemporary.depth == depth
				||	recycleTemporary.filterMode == filterMode
				||	recycleTemporary.format == format)
				{
					recycleTemporaries.Remove( recycleTemporary.propertyId);
					return recycleTemporary.propertyId;
				}
			}
			int temporary = Shader.PropertyToID( "CameraPipeline::PostTemporary" + temporaryCount);
			currentCommandBuffer.GetTemporaryRT( temporary, width, height, depth, filterMode, format);
			usedTemporaries.Add( temporary, new TemporaryTarget( temporary, width, height, depth, filterMode, format));
			++temporaryCount;
			return temporary;
		}
		void ReleaseTemporaryRT()
		{
			foreach( var userdTemporaryId in usedTemporaries.Keys)
			{
				currentCommandBuffer.ReleaseTemporaryRT( userdTemporaryId);
			}
			currentCommandBuffer = null;
		}
		Dictionary<int, TemporaryTarget> recycleTemporaries;
		Dictionary<int, TemporaryTarget> usedTemporaries;
		CommandBuffer currentCommandBuffer;
		int temporaryCount;
		
		sealed class TemporaryTarget
		{
			public TemporaryTarget( int propertyId, int width, int height, int depth, FilterMode filterMode, RenderTextureFormat format)
			{
				this.propertyId = propertyId;
				this.width = width;
				this.height = height;
				this.depth = depth;
				this.filterMode = filterMode;
				this.format = format;
			}
			public int propertyId;
			public int width;
			public int height;
			public int depth;
			public FilterMode filterMode;
			public RenderTextureFormat format;
		}
	}
}
	