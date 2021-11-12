
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace RenderingPipeline
{
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[RequireComponent( typeof( Camera))]
	public sealed partial class RenderPipeline : MonoBehaviour
	{
		void Awake()
		{
			cacheCamera = GetComponent<Camera>();
			fillMesh = new Mesh();
			fillMesh.SetVertices(
				new Vector3[]{
					new Vector3( 0, 0, 0),
					new Vector3( 0, 1, 0),
					new Vector3( 1, 1, 0),
					new Vector3( 1, 0, 0)
				});
			fillMesh.SetUVs( 
				0,
				new Vector2[]{
					new Vector2( 0, 0),
					new Vector2( 0, 1),
					new Vector2( 1, 1),
					new Vector2( 1, 0)
				});
			fillMesh.SetIndices(
				new int[]{ 0, 1, 2, 3 }, MeshTopology.Quads, 0, false);	
			fillMesh.Optimize();
			fillMesh.UploadMeshData( true);
			
			if( copyShader != null && copyMaterial == null)
			{
				copyMaterial = new Material( copyShader);
			}
		}
		void Start()
		{
			CollectionProcesses();
			RebuildCommandBuffers();
		}
	#if UNITY_EDITOR
		void OnEnable()
		{
			RenderPipelineEvent.saveAssets = () =>
			{
				ClearPropertiesCache();
			};
		}
		void OnDisable()
		{
			if( overrideTargetEvent.GetPersistentEventCount() > 0)
			{
				overrideTargetEvent.Invoke( null);
			}
			cacheCamera.SetTargetBuffers( Display.main.colorBuffer, Display.main.depthBuffer);
			cacheCamera.depthTextureMode = DepthTextureMode.None;
			cacheCamera.forceIntoRenderTexture = false;
			cacheCamera.allowHDR = false;
			cacheCamera.allowMSAA = false;
			
			RemoveCommandBuffers();
			ClearPropertiesCache();
		}
		void ClearPropertiesCache()
		{
			for( int i0 = 0; i0 < caches.Length; ++i0)
			{
				caches[ i0]?.ClearPropertiesCache();
			}
		}
	#endif
		void OnDestroy()
		{
		#if UNITY_EDITOR
			cacheCamera.SetTargetBuffers( Display.main.colorBuffer, Display.main.depthBuffer);
		#endif
			RemoveCommandBuffers();
			
			for( int i0 = 0; i0 < caches.Length; ++i0)
			{
				caches[ i0]?.Dispose();
			}
			if( opaqueScreenShot != null)
			{
				opaqueScreenShot.Release();
				opaqueScreenShot = null;
			}
			if( postScreenShot != null)
			{
				postScreenShot.Release();
				postScreenShot = null;
			}
			if( colorBuffer != null)
			{
				colorBuffer.Release();
				colorBuffer = null;
			}
			if( depthBuffer != null)
			{
				depthBuffer.Release();
				depthBuffer = null;
			}
			if( copyMaterial != null)
			{
				ObjectUtility.Release( copyMaterial);
				copyMaterial = null;
			}
			if( fillMesh != null)
			{
				ObjectUtility.Release( fillMesh);
				fillMesh = null;
			}
		}
		void OnPreRender()
		{
			bool fourceCacheClear = false;
		#if UNITY_EDITOR
			if( Application.isPlaying == false && enabled == false)
			{
				return;
			}
			if( CollectionProcesses() != false)
			{
				isRebuildCommandBuffers = true;
				fourceCacheClear = true;
			}
			if( cacheOverrideTargetBuffers != OverrideTargetBuffers)
			{
				cacheOverrideTargetBuffers = OverrideTargetBuffers;
				isRebuildCommandBuffers = true;
			}
			if( cacheOverrideCameraDepthTexture != OverrideCameraDepthTexture)
			{
				cacheOverrideCameraDepthTexture = OverrideCameraDepthTexture;
				isRebuildCommandBuffers = true;
			}
			if( cacheDefaultDepthTextureMode != defaultDepthTextureMode)
			{
				cacheDefaultDepthTextureMode = defaultDepthTextureMode;
				isRebuildCommandBuffers = true;
			}
			if( copyShader != null && copyMaterial == null)
			{
				copyMaterial = new Material( copyShader);
			}
		#endif
			if( OverrideTargetBuffers != false)
			{
				if( cacheScreenWidth != Screen.width || cacheScreenHeight != Screen.height || cacheResolutionScale != resolutionScale)
				{
					isRebuildCommandBuffers = true;
				}
			}
			if( cacheCamera.allowMSAA != false)
			{
				cacheCamera.allowMSAA = false;
				isRebuildCommandBuffers = true;
			}
			if( cacheDisplayWidth != Screen.width
			||	cacheDisplayHeight != Screen.height)
			{
				isRebuildCommandBuffers = true;
				cacheDisplayWidth = Screen.width;
				cacheDisplayHeight = Screen.height;
			}
			for( int i0 = 0; i0 < caches.Length; ++i0)
			{
				IPostProcess process = caches[ i0];
				bool cacheClear = fourceCacheClear;
				
				if( process != null)
				{
					if( process is IUbarProcess ubarProcess)
					{
						if( ubarProcess.PreProcess() == false
						&&	ubarProcess.HasIndependent( ref isRebuildCommandBuffers) == false)
						{
							continue;
						}
					}
				#if UNITY_EDITOR
					if( process.RestoreMaterials() != false)
					{
						cacheClear = true;
					}
				#endif
					if( process.UpdateProperties( this, cacheClear) != false)
					{
						isRebuildCommandBuffers = true;
					}
				}
			}
			if( isRebuildCommandBuffers != false)
			{
				RebuildCommandBuffers();
			}
			RebuildScreenShotCommandBuffer( "CameraPipeline::OpaqueScreenShot", ref phaseOpaqueScreenShot, 
				ref opaqueScreenShot, CameraEvent.BeforeImageEffectsOpaque, ref commandBufferOpaqueScreenShot, ref onOpaqueScreenShot);
			RebuildScreenShotCommandBuffer( "CameraPipeline::PostScreenShot", ref phasePostScreenShot, 
				ref postScreenShot,	CameraEvent.BeforeImageEffects, ref commandBufferPostScreenShot, ref onPostScreenShot);
		}
		void RemoveCommandBuffers()
		{
			if( commandBufferDepthTexture != null)
			{
				cacheCamera.RemoveCommandBuffer( CameraEvent.AfterForwardOpaque, commandBufferDepthTexture);
				commandBufferDepthTexture = null;
			}
			if( commandBufferOpaqueProcesses != null)
			{
				cacheCamera.RemoveCommandBuffer( CameraEvent.BeforeImageEffectsOpaque, commandBufferOpaqueProcesses);
				commandBufferOpaqueProcesses = null;
			}
			if( commandBufferPostProcesses != null)
			{
				cacheCamera.RemoveCommandBuffer( CameraEvent.BeforeImageEffects, commandBufferPostProcesses);
				commandBufferPostProcesses = null;
			}
			if( commandBufferOpaqueScreenShot != null)
			{
				cacheCamera.RemoveCommandBuffer( CameraEvent.BeforeImageEffectsOpaque, commandBufferOpaqueScreenShot);
				commandBufferOpaqueScreenShot = null;
			}
			if( commandBufferPostScreenShot != null)
			{
				cacheCamera.RemoveCommandBuffer( CameraEvent.BeforeImageEffects, commandBufferPostScreenShot);
				commandBufferPostScreenShot = null;
			}
		}
		int EnabledProcessCount( IPostProcess[] caches, PostProcessEvent postProcessEvent, ref DepthTextureMode depthTextureMode, ref bool highDynamicRangeTarget)
		{
			IPostProcess process;
			int i0, count = 0;
			
			for( i0 = 0; i0 < caches.Length; ++i0)
			{
				process = caches[ i0];
				
				if( process?.GetPostProcessEvent() != postProcessEvent)
				{
					continue;
				}
				if( process.Valid() != false)
				{
					depthTextureMode |= process.GetDepthTextureMode();
					
					if( process.IsRequiredHighDynamicRange() != false)
					{
						highDynamicRangeTarget = true;
					}
					++count;
				}
			}
			return count;
		}
		void RebuildCommandBuffers()
		{
			var depthTextureMode = defaultDepthTextureMode;
			bool highDynamicRangeTarget = false;
			bool forceIntoRenderTexture = false;
			IPostProcess nextProcess, process;
			int i0;
			
			/* 既存のコマンドバッファを解放する */
			RemoveCommandBuffers();
			
			/* 有効なプロセス数を求める */
			int enabledOpaqueProcessCount = EnabledProcessCount( 
				caches, PostProcessEvent.BeforeImageEffectsOpaque, 
				ref depthTextureMode, ref highDynamicRangeTarget);
			int enabledProcessCount = EnabledProcessCount( 
				caches, PostProcessEvent.BeforeImageEffects,
				ref depthTextureMode, ref highDynamicRangeTarget);
			
			/* [2019.4.1f1]
			 * SetTargetBuffers の引数に Display.main.*****Buffer を渡しても実機では正しく動作しない。
			 * エディタ上では動作し、SetTargetBuffers を呼び出す前と同じ状態に戻る。
			 * 実機では現状元に戻す方法が存在しないと思われる。
			 * そのため以下の条件文はランタイム中に切り替わることは想定しない。
			 */
			if( OverrideTargetBuffers == false)
			{
				if( overrideTargetEvent.GetPersistentEventCount() > 0)
				{
					overrideTargetEvent.Invoke( null);
				}
				forceIntoRenderTexture = true;
			#if UNITY_EDITOR
				cacheCamera.SetTargetBuffers( Display.main.colorBuffer, Display.main.depthBuffer);
			#endif
			}
			else
			{
			#if UNITY_WEBGL && !UNITY_EDITOR
				int targetWidth = Screen.width;
				int targetHeight = Screen.height;
				
				if( targetHeight < 720)
				{
					resolutionScale = 720.0f / targetHeight;
				}
				else if( targetHeight > 1080)
				{
					resolutionScale = 1080.0f / targetHeight;
				}
				else
				{
					resolutionScale = 1.0f;
				}
				targetWidth = (int)((float)targetWidth * resolutionScale);
				targetHeight = (int)((float)targetHeight * resolutionScale);
			#else
				int targetWidth = (int)((float)Screen.width * resolutionScale);
				int targetHeight = (int)((float)Screen.height * resolutionScale);
			#endif
				bool refreshColorBuffer = colorBuffer == null || colorBuffer.width != targetWidth || colorBuffer.height != targetHeight;
				bool refreshDepthBuffer = depthBuffer == null || depthBuffer.width != targetWidth || depthBuffer.height != targetHeight;
				
				if( refreshColorBuffer != false || refreshDepthBuffer != false)
				{
					if( overrideTargetEvent.GetPersistentEventCount() > 0)
					{
						overrideTargetEvent.Invoke( null);
					}
					cacheCamera.SetTargetBuffers( Display.main.colorBuffer, Display.main.depthBuffer);
					
					if( refreshColorBuffer != false)
					{
						if( colorBuffer != null)
						{
							colorBuffer.Release();
						}
						var colorBufferFormat = RenderTextureFormat.ARGB32;
						
						if( SystemInfo.SupportsRenderTextureFormat( (RenderTextureFormat)overrideTargetFormat) != false)
						{
							colorBufferFormat = (RenderTextureFormat)overrideTargetFormat;
						}
						colorBuffer = new RenderTexture( targetWidth, targetHeight, 0, colorBufferFormat);
						colorBuffer.filterMode = FilterMode.Point;
						colorBuffer.name = "CameraPipeline::ColorBuffer";
					}
					if( refreshDepthBuffer != false)
					{
						if( depthBuffer != null)
						{
							depthBuffer.Release();
						}
						depthBuffer = new RenderTexture( targetWidth, targetHeight, 24, RenderTextureFormat.Depth);
						depthBuffer.filterMode = FilterMode.Point;
						depthBuffer.name = "CameraPipeline::DepthBuffer";
					}
				}
				cacheCamera.SetTargetBuffers( colorBuffer.colorBuffer, depthBuffer.depthBuffer);
				cacheScreenWidth = Screen.width;
				cacheScreenHeight = Screen.height;
				cacheResolutionScale = resolutionScale;
				
				if( overrideTargetEvent.GetPersistentEventCount() > 0)
				{
					overrideTargetEvent.Invoke( colorBuffer);
				}
				if( (depthTextureMode & DepthTextureMode.Depth) != 0 && OverrideCameraDepthTexture != false)
				{
					commandBufferDepthTexture = new CommandBuffer();
					commandBufferDepthTexture.name = "CameraPipeline::DepthTexture";
					commandBufferDepthTexture.Clear();
					commandBufferDepthTexture.SetProjectionMatrix( Matrix4x4.Ortho( 0, 1, 0, 1, 0, 1));
					commandBufferDepthTexture.SetViewMatrix( Matrix4x4.identity);
					
					RenderTextureFormat format = RenderTextureFormat.R8;
					
					//EDGE(WebGL)だとRFloatが使えない；
					if( SystemInfo.SupportsRenderTextureFormat( RenderTextureFormat.RHalf) != false)
					{
						format = RenderTextureFormat.RHalf;
					}
					if( SystemInfo.SupportsRenderTextureFormat( RenderTextureFormat.RFloat) != false)
					{
						format = RenderTextureFormat.RFloat;
					}
					var overrideDepthTexture = new RenderTargetIdentifier( ShaderProperty.OverrideDepthTexture);
					commandBufferDepthTexture.GetTemporaryRT( ShaderProperty.OverrideDepthTexture, 
						-1, -1, 0, FilterMode.Bilinear, format);
					commandBufferDepthTexture.SetRenderTarget( 
						overrideDepthTexture, 
						RenderBufferLoadAction.DontCare,
						RenderBufferStoreAction.Store,
						RenderBufferLoadAction.DontCare,
						RenderBufferStoreAction.DontCare);
					commandBufferDepthTexture.SetGlobalTexture( ShaderProperty.MainTex, depthBuffer);
					DrawCopy( commandBufferDepthTexture);
					commandBufferDepthTexture.SetGlobalTexture( ShaderProperty.CameraDepthTexture, overrideDepthTexture);
					cacheCamera.AddCommandBuffer( CameraEvent.AfterForwardOpaque, commandBufferDepthTexture);
				}
			}
			/* 不透明系プロセス */
			if( enabledOpaqueProcessCount > 0)
			{
				commandBufferOpaqueProcesses = new CommandBuffer();
				commandBufferOpaqueProcesses.name = "CameraPipeline::OpaqueProcesses";
				commandBufferOpaqueProcesses.Clear();
				commandBufferOpaqueProcesses.SetProjectionMatrix( Matrix4x4.Ortho( 0, 1, 0, 1, 0, 1));
				commandBufferOpaqueProcesses.SetViewMatrix( Matrix4x4.identity);
				ResetTemporaryRT( commandBufferOpaqueProcesses);
				
				var context = new TargetContext();
				
				if( OverrideTargetBuffers == false)
				{
					context.SetBuffers();
					context.SetSource0( BuiltinRenderTextureType.CameraTarget);
					context.SetTarget0( BuiltinRenderTextureType.CameraTarget);
				}
				else
				{
					context.SetBuffers( colorBuffer, depthBuffer);
					context.SetSource0( colorBuffer);
					context.SetTarget0( colorBuffer);
				}
				for( i0 = 0, process = null; i0 < caches.Length; ++i0)
				{
					nextProcess = caches[ i0];
					
					if( nextProcess?.GetPostProcessEvent() != PostProcessEvent.BeforeImageEffectsOpaque)
					{
						continue;
					}
					if( nextProcess.Valid() != false)
					{
						if( nextProcess is IUbarProcess ubarProcess)
						{
							if( ubarProcess.Independent() == false)
							{
								continue;
							}
						}
						if( process != null)
						{
							RecycleTemporaryRT( context);
							
							if( process.BuildCommandBuffer( this, commandBufferOpaqueProcesses, context, nextProcess) != false)
							{
								context.Next();
							}
						}
						process = nextProcess;
					}
				}
				if( process != null)
				{
					RecycleTemporaryRT( context);
					
					if( OverrideTargetBuffers == false)
					{
						context.SetTarget0( BuiltinRenderTextureType.CameraTarget);
					}
					else
					{
						context.SetTarget0( colorBuffer);
					}
					process.BuildCommandBuffer( this, commandBufferOpaqueProcesses, context, null);
				}
				ReleaseTemporaryRT();
				commandBufferOpaqueProcesses.SetRenderTarget( -1);
				cacheCamera.AddCommandBuffer( CameraEvent.BeforeImageEffectsOpaque, commandBufferOpaqueProcesses);
			}
			/* 半透明系プロセス+FinalPass */
			if( enabledProcessCount > 0 || OverrideTargetBuffers != false)
			{
				commandBufferPostProcesses = new CommandBuffer();
				commandBufferPostProcesses.name = "CameraPipeline::PostProcesses";
				commandBufferPostProcesses.Clear();
				commandBufferPostProcesses.SetProjectionMatrix( Matrix4x4.Ortho( 0, 1, 0, 1, 0, 1));
				commandBufferPostProcesses.SetViewMatrix( Matrix4x4.identity);
				ResetTemporaryRT( commandBufferPostProcesses);
				
				if( enabledProcessCount > 0)
				{
					var context = new TargetContext();
					
					if( OverrideTargetBuffers == false)
					{
						context.SetBuffers();
						context.SetSource0( BuiltinRenderTextureType.CameraTarget);
						context.SetTarget0( BuiltinRenderTextureType.CameraTarget);
					}
					else
					{
						context.SetBuffers( colorBuffer, depthBuffer);
						context.SetSource0( colorBuffer);
						context.SetTarget0( colorBuffer);
					}
					for( i0 = 0, process = null; i0 < caches.Length; ++i0)
					{
						nextProcess = caches[ i0];
						
						if( nextProcess?.GetPostProcessEvent() != PostProcessEvent.BeforeImageEffects)
						{
							continue;
						}
						if( nextProcess.Valid() != false)
						{
							if( nextProcess is IUbarProcess ubarProcess)
							{
								if( ubarProcess.Independent() == false)
								{
									continue;
								}
							}
							if( process != null)
							{
								RecycleTemporaryRT( context);
								
								if( process.BuildCommandBuffer( this, commandBufferPostProcesses, context, nextProcess) != false)
								{
									context.Next();
								}
							}
							process = nextProcess;
						}
					}
					if( process != null)
					{
						RecycleTemporaryRT( context);
						
						if( OverrideTargetBuffers != false && overrideTargetEvent.GetPersistentEventCount() > 0)
						{
							context.SetTarget0( colorBuffer);
						}
						else
						{
							context.SetTarget0( BuiltinRenderTextureType.CameraTarget);
						}
						process.BuildCommandBuffer( this, commandBufferPostProcesses, context, null);
					}
					ReleaseTemporaryRT();
				}
				else if( overrideTargetEvent.GetPersistentEventCount() == 0)
				{
					commandBufferPostProcesses.SetRenderTarget( 
						BuiltinRenderTextureType.CameraTarget, 
						RenderBufferLoadAction.DontCare,
						RenderBufferStoreAction.Store,
						RenderBufferLoadAction.DontCare,
						RenderBufferStoreAction.DontCare);
					commandBufferPostProcesses.SetViewport( new Rect( 0, 0, Screen.width, Screen.height));
					commandBufferPostProcesses.SetGlobalTexture( ShaderProperty.MainTex, colorBuffer);
					DrawCopy( commandBufferPostProcesses);
				}
				if( OverrideCameraDepthTexture != false)
				{
					commandBufferPostProcesses.ClearRenderTarget( true, false, Color.clear, 1.0f);
					
					if( (depthTextureMode & DepthTextureMode.Depth) != 0)
					{
						commandBufferPostProcesses.ReleaseTemporaryRT( ShaderProperty.OverrideDepthTexture);
						
						if( depthTextureMode == DepthTextureMode.Depth)
						{
							depthTextureMode &= ~DepthTextureMode.Depth;
						}
					}
				}
				/* [2019.4.1f1、2021.15f1～]
				 * 一部の Android 端末において RenderPipeline でポストプロセスを処理した後に
				 * 別の Camera で DepthStencilBuffer をクリアしても正しくクリアされない症状を確認
				 * ※これは Cameara.clearFlags だけでなく CommandBuffer.ClearRenderTarget を使用しても同様の結果になります
				 * 
				 * 他のプラットフォームや大多数の Android 端末では問題ないため、CommandBuffer に対する
				 * SetRenderTarget の対象が間違っているとは考えにくく、ディスプレイバッファに戻っていると判断できる。
				 * 
				 * ここで気になるのはコマンドバッファが処理されるのが BeforeImageEffects という点にあります
				 * 基本的にこの後には AfterImageEffects と AfterEverything しか存在せず、ほぼレンダリングの最終工程に位置します
				 *
				 * 基本的に Unity はダブルバッファ以上で処理されているため、レンダリングの最終工程にはバックバッファのスワップが含まれているはず
				 * そのため、同じ種類のターゲットを指してはいるが、同一フレームで対象としてきたバックバッファとは異なる対象が設定されたままになっているのではと判断
				 *
				 * ドキュメントには記載されていないが SetRenderTarget で -1 を渡すことでバックバッファが設定出来るもよう
				 * http://blackmasqueradegames.com/2015/04/unity-5-afterimageeffects-and-aftereverything-traps/
				 */
				commandBufferPostProcesses.SetRenderTarget( -1);
				cacheCamera.AddCommandBuffer( CameraEvent.BeforeImageEffects, commandBufferPostProcesses);
			}
			cacheCamera.allowHDR = highDynamicRangeTarget;
			cacheCamera.depthTextureMode = depthTextureMode;
			cacheCamera.forceIntoRenderTexture = forceIntoRenderTexture;
			isRebuildCommandBuffers = false;
		}
		internal bool Capture( PostProcessEvent phase, System.Action<Texture> onComplete)
		{
			if( phase == PostProcessEvent.BeforeImageEffectsOpaque)
			{
				if( onOpaqueScreenShot == null
				||	onOpaqueScreenShot == onComplete)
				{
					if( onComplete == null)
					{
						onComplete = (texture) => {};
					}
					onOpaqueScreenShot = onComplete;
					phaseOpaqueScreenShot |= kScreenShotPhaseCapture;
					return true;
				}
			}
			else if( phase == PostProcessEvent.BeforeImageEffects)
			{
				if( onPostScreenShot == null
				||	onPostScreenShot == onComplete)
				{
					if( onComplete == null)
					{
						onComplete = (texture) => {};
					}
					onPostScreenShot = onComplete;
					phasePostScreenShot |= kScreenShotPhaseCapture;
					return true;
				}
			}
			return false;
		}
		void RebuildScreenShotCommandBuffer( string name, ref int phase, ref RenderTexture targetBuffer,
			CameraEvent cameraEvent, ref CommandBuffer commandBuffer, ref System.Action<Texture> onComplete)
		{
			if( (phase & kScreenShotPhaseComplete) != 0)
			{
				onComplete?.Invoke( targetBuffer);
				onComplete = null;
				phase &= ~kScreenShotPhaseComplete;
			}
			if( (phase & kScreenShotPhaseCapture) != 0)
			{
				int screenWidth = ScreenWidth;
				int screenHeight = ScreenHeight;
				
				if( targetBuffer == null
				||	targetBuffer.width != screenWidth
				||	targetBuffer.height != screenHeight)
				{
					targetBuffer?.Release();
					targetBuffer = new RenderTexture( screenWidth, screenHeight, 0, RenderTextureFormat.ARGB32);
					targetBuffer.filterMode = FilterMode.Bilinear;
					targetBuffer.name = name;
					
					if( commandBuffer != null)
					{
						cacheCamera.RemoveCommandBuffer( cameraEvent, commandBuffer);
						commandBuffer = null;
					}
				}
				if( commandBuffer == null)
				{
					commandBuffer = new CommandBuffer();
					commandBuffer.name = name;
					commandBuffer.Clear();
					commandBuffer.SetProjectionMatrix( Matrix4x4.Ortho( 0, 1, 0, 1, 0, 1));
					commandBuffer.SetViewMatrix( Matrix4x4.identity);
					commandBuffer.Blit( BuiltinRenderTextureType.CameraTarget, targetBuffer);
					cacheCamera.AddCommandBuffer( cameraEvent, commandBuffer);
				}
				phase &= ~kScreenShotPhaseCapture;
				phase |= kScreenShotPhaseComplete;
			}
			else if( commandBuffer != null)
			{
				cacheCamera.RemoveCommandBuffer( cameraEvent, commandBuffer);
				commandBuffer = null;
			}
		}
		internal void DrawCopy( CommandBuffer commandBuffer)
		{
			commandBuffer.DrawMesh( fillMesh, Matrix4x4.identity, copyMaterial, 0, 0);
		}
		internal void DrawFill( CommandBuffer commandBuffer, Material material, int shaderPass)
		{
			commandBuffer.DrawMesh( fillMesh, Matrix4x4.identity, material, 0, shaderPass);
		}
		internal void SetViewport( CommandBuffer commandBuffer, IPostProcess nextProcess)
		{
			if( commandBuffer == commandBufferPostProcesses && nextProcess == null)
			{
				commandBuffer.SetViewport( new Rect( 0, 0, Screen.width, Screen.height));
			}
		}
		internal bool IsScreenShotTexture( Texture texture)
		{
			if( texture != null)
			{
				return texture == opaqueScreenShot || texture == postScreenShot;
			}
			return false;
		}
		internal Camera CacheCamera
		{
			get => cacheCamera;
		}
		internal RenderTargetIdentifier DepthStencilBuffer
		{
			get
			{
				if( OverrideCameraDepthTexture != false)
				{
					return depthBuffer;
				}
			#if UNITY_EDITOR
				return BuiltinRenderTextureType.RenderTexture;
			#else
				return BuiltinRenderTextureType.CameraTarget;
			#endif
			}
		}
		internal bool OverrideTargetBuffers
		{
			get
			{
			#if UNITY_EDITOR
				if( Application.isPlaying == false)
				{
					return false;
				}
			#endif
				return overrideTargetBuffers;
			}
		}
		internal bool OverrideCameraDepthTexture
		{
			get
			{
				if( OverrideTargetBuffers != false)
				{
					return overrideCameraDepthTexture;
				}
				return false;
			}
		}
		internal int ScreenWidth
		{
			get{ return (cacheScreenWidth.HasValue != false)? cacheScreenWidth.Value : Screen.width; }
		}
		internal int ScreenHeight
		{
			get{ return (cacheScreenHeight.HasValue != false)? cacheScreenHeight.Value : Screen.height; }
		}
		const string kTipsOverrideTargetBuffers = 
			"レンダリングパスでUpdateDepthTextureが処理されていない状態でも_CameraDepthTextureに深度情報が書き込まれるようになります。\n\n" +
			"この機能を有効にした場合、UpdateDepthTextureで行われるDrawCallが無駄になるため、UpdateDepthTextureが発生しない様にする必要があります。\n\n" +
			"UpdateDepthTextureが発生しない様にするにはModeがRealtimeに設定されているLightのShadowTypeにNoShadowsが設定されている必要があります。\n\n" +
			"※この機能はUpdateDepthTextureで_CameraDepthTextureが利用可能になる場合と異なり、ForwardOpaque中に使用することが出来ません。\n\n" +
			"※ポストプロセスの使用状況によって_CameraDepthTextureに書き込まれない場合があるため、強制する場合は DefaultDepthTextureMode の Depth を有効にしてください。";
		const int kScreenShotPhaseCapture = 0x01;
		const int kScreenShotPhaseComplete = 0x02;
		
		enum RenderTargetFormat
		{
			Default = RenderTextureFormat.Default,
			DefaultHDR = RenderTextureFormat.DefaultHDR,
			ARGBA32 = RenderTextureFormat.ARGB32,
		}
		enum RenderTargetDepth
		{
			Bit24 = 24,
			Bit16 = 16,
		}
		
		[SerializeField]
		Shader copyShader = default;
		[SerializeField]
		Shader ubarShader = default;
		[SerializeField]
		RenderTargetFormat defaultColorTargetFormat = RenderTargetFormat.DefaultHDR;
		[SerializeField]
		DepthTextureMode defaultDepthTextureMode = default;
		[SerializeField, TooltipAttribute( kTipsOverrideTargetBuffers)]
		bool overrideTargetBuffers = false;
		[SerializeField]
		RenderTargetFormat overrideTargetFormat = RenderTargetFormat.DefaultHDR;
		[SerializeField, Range( 0.1f, 5.0f)]
		float resolutionScale = 1.0f;
		[SerializeField]
		bool overrideCameraDepthTexture = true;
		[SerializeField]
		GameObject postProcessesTarget = default;
		[System.Serializable]
		sealed class OverrideTargetEvent : UnityEvent<RenderTexture>{}
		[SerializeField]
		OverrideTargetEvent overrideTargetEvent = new OverrideTargetEvent();
		
		Mesh fillMesh;
		Material copyMaterial;
		RenderTexture colorBuffer;
		RenderTexture depthBuffer;
		RenderTexture opaqueScreenShot;
		RenderTexture postScreenShot;
		bool isRebuildCommandBuffers;
		
		IPostProcess[] caches = new IPostProcess[ 2];
		CommandBuffer commandBufferDepthTexture;
		CommandBuffer commandBufferOpaqueProcesses;
		CommandBuffer commandBufferPostProcesses;
		CommandBuffer commandBufferOpaqueScreenShot;
		CommandBuffer commandBufferPostScreenShot;
		
		System.Action<Texture> onOpaqueScreenShot;
		System.Action<Texture> onPostScreenShot;
		int phaseOpaqueScreenShot;
		int phasePostScreenShot;
		
		Camera cacheCamera;
		int? cacheDisplayWidth;
		int? cacheDisplayHeight;
		int? cacheScreenWidth;
		int? cacheScreenHeight;
		float? cacheResolutionScale;
	#if UNITY_EDITOR
		DepthTextureMode? cacheDefaultDepthTextureMode;
		bool? cacheOverrideTargetBuffers;
		bool? cacheOverrideCameraDepthTexture;
	#endif
	}
#if UNITY_EDITOR
	internal class RenderPipelineEvent : UnityEditor.AssetModificationProcessor
	{
		static string[] OnWillSaveAssets( string[] paths)
		{
			saveAssets?.Invoke();
			return paths;
		}
		internal static System.Action saveAssets;
	}
#endif
}
	