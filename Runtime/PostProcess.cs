
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	public interface IPostProcess
	{
		/**
		 * \brief リソースの作成を行います。
		 */
		void Create();
		/**
		 * \brief リソースの破棄を行います。
		 */
		void Dispose();
		/**
		 * \brief マテリアルの復元を行います。
		 * \note エディタ動作時のみ呼び出されます。
		 */
		bool RestoreMaterials();
		/**
		 * \brief 有効な状態かどうかを確認します。
		 * \return 確認結果。以下の状態が返ります。
		 * \retval true 有効な場合に返ります。
		 * \retval false 無効な場合に返ります。
		 */
		bool Valid();
		/**
		 * \brief プロパティのキャッシュをクリアします。
		 */
		void ClearPropertiesCache();
		/**
		 * \brief プロパティを更新します。
		 * \param clearCache [in] true の場合キャッシュをクリアします。
		 */
		bool UpdateProperties( bool clearCache);
		/**
		 * \brief プロセスを実行するイベントを取得します。
		 * \return カメライベント。以下の値が現状有効値として返ります。
		 * \retval CameraEvent.BeforeImageEffectsOpaque
		 * \retval CameraEvent.BeforeImageEffects 
		 \ \note https://docs.unity3d.com/2019.2/Documentation/uploads/SL/CameraRenderFlowCmdBuffers.png
		 */
		CameraEvent GetCameraEvent();
		/**
		 * \brief カメラの深度テクスチャレンダリングモードを取得します。
		 * \return 深度テクスチャモードが返ります。
		 */
		DepthTextureMode GetDepthTextureMode();
		/**
		 * \brief High Dynamic Range が必要かどうかを確認します。
		 * \return 確認結果。以下の値が返ります。
		 * \retval true HDRが必要な場合に返ります。
		 * \retval false HDRが不要な場合に返ります。
		 */
		bool IsHighDynamicRange();
		/**
		 * \brief ステンシルのハッシュ値を取得する。
		 * \return ステンシルのハッシュが返ります。
		 */
		long GetDepthStencilHashCode();
		
		void BuildCommandBuffer( 
			CommandBuffer commandBuffer, TargetContext context, 
			System.Func<int, int, int, FilterMode, RenderTextureFormat, int> GetTemporaryRT);
	}
	public abstract class PostProcess : MonoBehaviour, IPostProcess
	{
		public abstract void Create();
		public abstract void Dispose();
		public abstract bool RestoreMaterials();
		public abstract bool Valid();
		public abstract void ClearPropertiesCache();
		public abstract bool UpdateProperties( bool clearCache);
		public abstract CameraEvent GetCameraEvent();
		public abstract DepthTextureMode GetDepthTextureMode();
		public abstract bool IsHighDynamicRange();
		public abstract void BuildCommandBuffer( 
			CommandBuffer commandBuffer, TargetContext context, 
			System.Func<int, int, int, FilterMode, RenderTextureFormat, int> GetTemporaryRT);
		
		public virtual long GetDepthStencilHashCode()
		{
			return DepthStencil.kDefaultHash;
		}
		protected RenderPipeline Pipeline
	#if UNITY_EDITOR
			{ get; private set; }
	#else
			;
	#endif
		protected IPostProcess NextProcess
	#if UNITY_EDITOR
			{ get; private set; }
	#else
			;
	#endif
		internal void Initialize( RenderPipeline pipeline)
		{
			Pipeline = pipeline;
		}
		internal void SetNextProcess( IPostProcess nextProcess)
		{
			NextProcess = nextProcess;
		}
	}
	public abstract class InternalPostProcess : PostProcess
	{
		internal bool DuplicateMRT()
		{
			if( SystemInfo.supportedRenderTargetCount > 1)
			{
				return OnDuplicate();
			}
			return false;
		}
		protected virtual bool OnDuplicate()
		{
			return false;
		}
	}
}
