
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
			for( int i0 = 0; i0 < caches.Length; ++i0)
			{
				if( caches[ i0] is T process)
				{
					return process;
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
			UbarProcess opaqueCaches = null, opaqueUbar = null;
			UbarProcess beforeCaches = null, beforeUbar = null;
			IPostProcess component;
			int i0, i1;
			
			opaqueCaches = caches[ caches.Length - 2] as UbarProcess;
			caches[ caches.Length - 2] = null;
			beforeCaches = caches[ caches.Length - 1] as UbarProcess;
			caches[ caches.Length - 1] = null;
			
			for( i1 = 0; i1 < components.Length; ++i1)
			{
				component = components[ i1];
				
				/* 既存のコンポーネントであればキャッシュからコレクションに移動する */
				for( i0 = 0; i0 < caches.Length; ++i0)
				{
					if( object.ReferenceEquals( caches[ i0], component) != false)
					{
					#if UNITY_EDITOR
						if( caches[ i0] is PostProcess process)
						{
							if( process.ChangePostProcessEvent() != false)
							{
								rebuild = true;
							}
						}
					#endif
						caches[ i0] = null;
						break;
					}
				}
				/* 新規のコンポーネントは初期化する */
				if( i0 == caches.Length)
				{
					component.Create();
					component.UpdateProperties( this, true);
					rebuild = true;
				}
				collection.Add( component);
			}
			/* 移動しきれていないキャッシュがある場合、コンポーネントが削除されたとみなして破棄する。*/
			for( i0 = 0; i0 < caches.Length; ++i0)
			{
				if( object.ReferenceEquals( caches[ i0], null) == false)
				{
					caches[ i0]?.Dispose();
					rebuild = true;
				}
			}
			/* 不透明用の Ubar プロセスの生成状態を確認 */
			if( opaqueCaches == null)
			{
				opaqueUbar = new UbarProcess( ubarShader, PostProcessEvent.BeforeImageEffectsOpaque);
				opaqueUbar.Create();
				rebuild = true;
			}
			/* 半透明用の Ubar プロセスの生成状態を確認 */
			if( beforeCaches == null)
			{
				beforeUbar = new UbarProcess( ubarShader, PostProcessEvent.BeforeImageEffects);
				beforeUbar.Create();
				rebuild = true;
			}
			/* 再構成する必要が無い場合はそのままキャッシュに格納する */
			if( rebuild == false && caches.Length == collection.Count + 2)
			{
				for( i0 = collection.Count - 1; i0 >= 0; --i0)
				{
					caches[ i0] = collection[ i0];
				}
				caches[ caches.Length - 2] = opaqueCaches;
				caches[ caches.Length - 1] = beforeCaches;
			}
			else
			{
				if( opaqueCaches != null)
				{
					opaqueUbar = opaqueCaches;
					opaqueUbar.ResetProperty();
				}
				if( beforeCaches != null)
				{
					beforeUbar = beforeCaches;
					beforeUbar.ResetProperty();
				}
				collection.Add( opaqueUbar);
				collection.Add( beforeUbar);
				caches = collection.ToArray();
				
				for( i0 = 0; i0 < caches.Length; ++i0)
				{
					if( caches[ i0] is UbarProperty ubarProperty)
					{
						switch( ubarProperty.GetPostProcessEvent())
						{
							case PostProcessEvent.BeforeImageEffectsOpaque:
							{
								opaqueUbar.SetProperty( ubarProperty);
								break;
							}
							case PostProcessEvent.BeforeImageEffects:
							{
								beforeUbar.SetProperty( ubarProperty);
								break;
							}
						}
					}
				}
				if( opaqueUbar == null && opaqueCaches != null)
				{
					opaqueCaches.Dispose();
					rebuild = true;
				}
				if( beforeUbar == null && beforeCaches != null)
				{
					beforeCaches.Dispose();
					rebuild = true;
				}
			}
			return rebuild;
		}
	}
}
	