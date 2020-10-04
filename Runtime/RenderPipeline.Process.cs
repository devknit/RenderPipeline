
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace RenderPipeline
{
	public sealed partial class RenderPipeline : MonoBehaviour
	{
		public EdgeDetection EdgeDetection
		{
			get{ return FindProcess<EdgeDetection>(); }
		}
		public SSAO SSAO
		{
			get{ return FindProcess<SSAO>(); }
		}
		public Bloom Bloom
		{
			get{ return FindProcess<Bloom>(); }
		}
		public DepthOfField DepthOfField
		{
			get{ return FindProcess<DepthOfField>(); }
		}
		public CameraMotionBlur CameraMotionBlur
		{
			get{ return FindProcess<CameraMotionBlur>(); }
		}
		public Mosaic Mosaic
		{
			get{ return FindProcess<Mosaic>(); }
		}
		public FXAA FXAA
		{
			get{ return FindProcess<FXAA>(); }
		}
		public ScreenBlend ScreenBlend
		{
			get{ return FindProcess<ScreenBlend>(); }
		}
		T FindProcess<T>() where T : PostProcess
		{
			for( int i0 = 0; i0 < postProcesses.Length; ++i0)
			{
				if( postProcesses[ i0] is T t)
				{
					return t;
				}
			}
			return null;
		}
		bool CollectionProcesses()
		{
			GameObject targetObject = (postProcessesTarget != null)? postProcessesTarget : gameObject;
			bool rebuild = false;
			
			PostProcess[] components = targetObject.GetComponents<PostProcess>();
			var collection = new List<IPostProcess>();
			IPostProcess component;
			UbarProcess opaqueUbar, newOpaqueUbar = null;
			UbarProcess postUbar, newPostUbar = null;
			int i0, i1;
			
			opaqueUbar = postProcesses[ postProcesses.Length - 2] as UbarProcess;
			postUbar = postProcesses[ postProcesses.Length - 1] as UbarProcess;
			postProcesses[ postProcesses.Length - 2] = null;
			postProcesses[ postProcesses.Length - 1] = null;
			
			for( i1 = 0; i1 < components.Length; ++i1)
			{
				component = components[ i1];
				
				for( i0 = 0; i0 < postProcesses.Length; ++i0)
				{
					if( object.ReferenceEquals( postProcesses[ i0], component) != false)
					{
						postProcesses[ i0] = null;
						break;
					}
				}
				if( i0 == postProcesses.Length)
				{
					component.Create();
					component.UpdateProperties( this, true);
					rebuild = true;
				}
				collection.Add( component);
			}
			for( i0 = 0; i0 < postProcesses.Length; ++i0)
			{
				if( object.ReferenceEquals( postProcesses[ i0], null) == false)
				{
					postProcesses[ i0].Dispose();
					rebuild = true;
				}
			}
			collection.Add( null);
			collection.Add( null);
			postProcesses = collection.ToArray();
			
			for( i0 = 0; i0 < postProcesses.Length; ++i0)
			{
				if( postProcesses[ i0] is UbarProperty ubarProperty)
				{
					switch( ubarProperty.GetPostProcessEvent())
					{
						case PostProcessEvent.BeforeImageEffectsOpaque:
						{
							if( newOpaqueUbar == null)
							{
								if( opaqueUbar == null)
								{
									newOpaqueUbar = new UbarProcess( ubarShader, PostProcessEvent.BeforeImageEffectsOpaque);
									newOpaqueUbar.Create();
									rebuild = true;
								}
								else
								{
									newOpaqueUbar = opaqueUbar;
									newOpaqueUbar.ResetProperty();
								}
							}
							newOpaqueUbar.SetProperty( ubarProperty);
							break;
						}
						case PostProcessEvent.BeforeImageEffects:
						{
							if( newPostUbar == null)
							{
								if( postUbar == null)
								{
									newPostUbar = new UbarProcess( ubarShader, PostProcessEvent.BeforeImageEffects);
									newPostUbar.Create();
									rebuild = true;
								}
								else
								{
									newPostUbar = postUbar;
									newPostUbar.ResetProperty();
								}
							}
							newPostUbar.SetProperty( ubarProperty);
							break;
						}
					}
				}
			}
			if( newOpaqueUbar == null && opaqueUbar != null)
			{
				opaqueUbar.Dispose();
				rebuild = true;
			}
			if( newPostUbar == null && postUbar != null)
			{
				postUbar.Dispose();
				rebuild = true;
			}
			postProcesses[ postProcesses.Length - 2] = newOpaqueUbar;
			postProcesses[ postProcesses.Length - 1] = newPostUbar;
			
			return rebuild;
		}
	}
}
	