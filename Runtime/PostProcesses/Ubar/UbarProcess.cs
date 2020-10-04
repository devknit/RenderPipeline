﻿
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;

namespace RenderPipeline
{
	[System.Serializable]
	public sealed class UbarProcess : IPostProcess
	{
		public UbarProcess( Shader ubarShader, CameraEvent ubarCameraEvent)
		{
			shader = ubarShader;
			cameraEvent = ubarCameraEvent;
		}
		public void Create()
		{
			if( shader != null && material == null)
			{
				material = new Material( shader);
			}
		}
		public void Dispose()
		{
			if( material != null)
			{
				ObjectUtility.Release( material);
				material = null;
			}
		}
		public bool RestoreMaterials()
		{
			bool rebuild = false;
			
			if( shader != null && material == null)
			{
				material = new Material( shader);
				rebuild = true;
			}
			return rebuild;
		}
		public bool Valid()
		{
			if( material != null)
			{
				foreach( var property in properties)
				{
					if( property.Independent() == false)
					{
						if( property.GetProperties().Enabled != false)
						{
							return true;
						}
					}
				}
			}
			return false;
		}
		public void ClearPropertiesCache()
		{
			foreach( var property in properties)
			{
				if( property.Independent() == false)
				{
					property.GetProperties().ClearCache();
				}
			}
		}
		public bool UpdateProperties( RenderPipeline pipeline, bool clearCache)
		{
			bool rebuild = false;
			
			if( clearCache != false)
			{
				ClearPropertiesCache();
			}
			foreach( var property in properties)
			{
				if( property.GetProperties().UpdateProperties( material, property.Independent()) != false)
				{
					rebuild = true;
				}
			}
			return rebuild;
		}
		public CameraEvent GetCameraEvent()
		{
			return cameraEvent;
		}
		public DepthTextureMode GetDepthTextureMode()
		{
			return DepthTextureMode.None;
		}
		public bool IsRequiredHighDynamicRange()
		{
			return false;
		}
		public void BuildCommandBuffer( RenderPipeline pipeline,
			CommandBuffer commandBuffer, TargetContext context, IPostProcess nextProcess)
		{
			if( context.CompareSource0ToTarget0() != false)
			{
				int temporary = pipeline.GetTemporaryRT();
				if( nextProcess == null)
				{
					commandBuffer.Blit( context.source0, temporary);
					context.SetSource0( temporary);
				}
				else
				{
					context.SetTarget0( temporary);
				}
			}
			commandBuffer.SetRenderTarget( 
				context.target0, 
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.Store,
				RenderBufferLoadAction.DontCare,	
				RenderBufferStoreAction.DontCare);
			commandBuffer.SetGlobalTexture( ShaderProperty.MainTex, context.source0);
			pipeline.DrawFill( commandBuffer, material, 0);
			context.duplicated = false;
		}
		public long GetDepthStencilHashCode()
		{
			return DepthStencil.kDefaultHash;
		}
		internal void ResetProperty()
		{
			properties.Clear();
		}
		internal void SetProperty( UbarProperty property)
		{
			if( property != null)
			{
				properties.Add( property);
			}
		}
		
		[System.NonSerialized]
        Shader shader;
		[System.NonSerialized]
		Material material;
		[System.NonSerialized]
		CameraEvent cameraEvent;
		[System.NonSerialized]
		List<UbarProperty> properties = new List<UbarProperty>();
	}
}