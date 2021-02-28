
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace RenderingPipeline
{
	public sealed class ClearRenderTargetSettings : Settings<ClearRenderTargetProperties>
	{
	}
	[System.Serializable]
	public sealed class ClearRenderTargetProperties : IGenericProperties
	{
		public bool Enabled
		{
			get{ return enabled; }
			set{ enabled = value; }
		}
		public PostProcessEvent Phase
		{
			get{ return phase; }
		}
		public bool ClearColor
		{
			get{ return clearColor; }
			set{ clearColor = value; }
		}
		public bool ClearDepth
		{
			get{ return clearDepth; }
			set{ clearDepth = value; }
		}
		public Color Color
		{
			get{ return color; }
			set{ color = value; }
		}
		public float Depth
		{
			get{ return depth; }
			set{ depth = value; }
		}
		public void ClearCache()
		{
			cacheEnabled = null;
			cacheClearColor = null;
			cacheClearDepth = null;
			cacheColor = null;
			cacheDepth = null;
		}
		public bool UpdateProperties( RenderPipeline pipeline, Material material)
		{
			bool rebuild = false;
			
			if( cacheEnabled != enabled)
			{
				rebuild = true;
				cacheEnabled = enabled;
			}
			if( enabled != false)
			{
				if( cacheClearColor != clearColor)
				{
					rebuild = true;
					cacheClearColor = clearColor;
				}
				if( cacheClearDepth != clearDepth)
				{
					rebuild = true;
					cacheClearDepth = clearDepth;
				}
				if( clearColor != false)
				{
					if( cacheColor != color)
					{
						rebuild = true;
						cacheColor = color;
					}
				}
				if( clearDepth != false)
				{
					if( cacheDepth != depth)
					{
						rebuild = true;
						cacheDepth = depth;
					}
				}
			}
			return rebuild;
		}
		
		[SerializeField]
		bool enabled = true;
		[SerializeField]
		PostProcessEvent phase = PostProcessEvent.BeforeImageEffectsOpaque;
		[SerializeField]
		bool clearColor = false;
		[SerializeField]
		bool clearDepth = true;
		[SerializeField]
		Color color = Color.clear;
		[SerializeField, Range( 0, 1)]
		float depth = 1.0f;
		
		[System.NonSerialized]
		bool? cacheEnabled;
		[System.NonSerialized]
		bool? cacheClearColor;
		[System.NonSerialized]
		bool? cacheClearDepth;
		[System.NonSerialized]
		Color? cacheColor;
		[System.NonSerialized]
		float? cacheDepth;
	}
}
