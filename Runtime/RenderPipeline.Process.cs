
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace RenderPipeline
{
	public sealed partial class RenderPipeline : MonoBehaviour
	{
		public EdgeDetection EdgeDetection
		{
			get{ return opaqueProcesses[ kOpaquePriorityEdgeDetection] as EdgeDetection; }
		}
		public SSAO SSAO
		{
			get{ return opaqueProcesses[ kOpaquePrioritySSAO] as SSAO; }
		}
		public UbarProcess OpaqueUbar
		{
			get{ return opaqueProcesses[ opaqueProcesses.Length - 1] as UbarProcess; }
			set{ opaqueProcesses[ opaqueProcesses.Length - 1] = value; }
		}
		public Bloom Bloom
		{
			get{ return postProcesses[ kPostPriorityBloom] as Bloom; }
		}
		public DepthOfField DepthOfField
		{
			get{ return postProcesses[ kPostPriorityDepthOfField] as DepthOfField; }
		}
		public CameraMotionBlur CameraMotionBlur
		{
			get{ return postProcesses[ kPostPriorityCameraMotionBlur] as CameraMotionBlur; }
		}
		public Mosaic Mosaic
		{
			get{ return postProcesses[ kPostPriorityMosaic] as Mosaic; }
		}
		public FXAA FXAA
		{
			get{ return postProcesses[ kPostPriorityFXAA] as FXAA; }
		}
		public ScreenBlend ScreenBlend
		{
			get{ return postProcesses[ kPostPriorityScreenBlend] as ScreenBlend; }
		}
		public UbarProcess PostUbar
		{
			get{ return postProcesses[ postProcesses.Length - 1] as UbarProcess; }
			set{ postProcesses[ postProcesses.Length - 1] = value as UbarProcess; }
		}
		bool CollectionProcesses()
		{
			GameObject targetObject = (postProcessesTarget != null)? postProcessesTarget : gameObject;
			bool rebuild = false;
			
		#if UNITY_EDITOR
			if( (opaqueProcesses?.Length ?? 0) != kOpaqueProcesses.Length + 1)
			{
				opaqueProcesses = new IPostProcess[ kOpaqueProcesses.Length + 1];
				rebuild = true;
			}
			if( (postProcesses?.Length ?? 0) != kPostProcesses.Length + 1)
			{
				postProcesses = new IPostProcess[ kPostProcesses.Length + 1];
				rebuild = true;
			}
		#endif
			(System.Type type, int index) composition;
			int i0;
			
			for( i0 = 0; i0 < kOpaqueProcesses.Length; ++i0)
			{
				composition = kOpaqueProcesses[ i0];
				
				if( VerifyProcess( targetObject, composition.type, opaqueProcesses, composition.index) != false)
				{
					rebuild = true;
				}
			}
			for( i0 = 0; i0 < kPostProcesses.Length; ++i0)
			{
				composition = kPostProcesses[ i0];
				
				if( VerifyProcess( targetObject, composition.type, postProcesses, composition.index) != false)
				{
					rebuild = true;
				}
			}
			
			/* post ubar */
			UbarProcess ubarProcess = null;
			
			for( i0 = 0; i0 < postProcesses.Length; ++i0)
			{
				if( postProcesses[ i0] is UbarProperty ubarProperty)
				{
					if( ubarProcess == null)
					{
						if( PostUbar == null)
						{
							ubarProcess = new UbarProcess( ubarShader);
							ubarProcess.Create();
							PostUbar = ubarProcess;
							rebuild = true;
						}
						else
						{
							ubarProcess = PostUbar;
							ubarProcess.ResetProperty();
						}
					}
					ubarProcess.SetProperty( ubarProperty);
				}
			}
			if( ubarProcess == null)
			{
				PostUbar?.Dispose();
				PostUbar = null;
				rebuild = true;
			}
			return rebuild;
		}
		bool VerifyProcess( GameObject gameObject, System.Type type, IPostProcess[] processes, int index)
		{
			var component = gameObject.GetComponent( type) as PostProcess;
			
			if( object.ReferenceEquals( processes[ index], component) == false)
			{
				if( object.ReferenceEquals( processes[ index], null) == false)
				{
					processes[ index].Dispose();
					processes[ index] = null;
				}
				if( object.ReferenceEquals( component, null) == false)
				{
					component.Create();
					component.UpdateProperties( this, true);
					processes[ index] = component;
				}
				return true;
			}
			return false;
		}

		const int kOpaquePriorityEdgeDetection = 0;
		const int kOpaquePrioritySSAO = 1;
		
		const int kPostPriorityBloom = 0;
		const int kPostPriorityDepthOfField = 1;
		const int kPostPriorityCameraMotionBlur = 2;
		const int kPostPriorityGlitch = 3;
		const int kPostPriorityMosaic = 4;
		const int kPostPriorityFXAA = 5;
		const int kPostPriorityScreenBlend = 6;
		const int kPostPriorityVignette = 7;
		
		static readonly (System.Type, int)[] kOpaqueProcesses = new []
		{
			(typeof( EdgeDetection), kOpaquePriorityEdgeDetection),
			(typeof( SSAO), kOpaquePrioritySSAO),
		};
		static readonly (System.Type, int)[] kPostProcesses = new []
		{
			(typeof( Bloom), kPostPriorityBloom),
			(typeof( DepthOfField), kPostPriorityDepthOfField),
			(typeof( CameraMotionBlur), kPostPriorityCameraMotionBlur),
			(typeof( Glitch), kPostPriorityGlitch),
			(typeof( Mosaic), kPostPriorityMosaic),
			(typeof( FXAA), kPostPriorityFXAA),
			(typeof( ScreenBlend), kPostPriorityScreenBlend),
			(typeof( Vignette), kPostPriorityVignette),
		};
	}
}
	