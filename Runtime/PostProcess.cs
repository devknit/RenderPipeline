
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	public abstract class PostProcess : MonoBehaviour
	{
		/**
		 * \brief リソースの作成を行います。
		 */
		public abstract void Create();
		/**
		 * \brief リソースの破棄を行います。
		 */
		public abstract void Dispose();
		/**
		 * \brief マテリアルの復元を行います。
		 * \note エディタ動作時のみ呼び出されます。
		 */
		public abstract bool RestoreMaterials();
		/**
		 * \brief 有効な状態かどうかを確認します。
		 * \return 確認結果。以下の状態が返ります。
		 * \retval true 有効な場合に返ります。
		 * \retval false 無効な場合に返ります。
		 */
		public abstract bool Valid();
		/**
		 * \brief プロパティのキャッシュをクリアします。
		 */
		public abstract void ClearPropertiesCache();
		/**
		 * \brief プロパティを更新します。
		 * \param clearCache [in] true の場合キャッシュをクリアします。
		 */
		public abstract bool UpdateProperties( bool clearCache);
		/**
		 * \brief プロセスを実行するイベントを取得します。
		 * \return カメライベント。以下の値が現状有効値として返ります。
		 * \retval CameraEvent.BeforeImageEffectsOpaque
		 * \retval CameraEvent.BeforeImageEffects 
		 \ \note https://docs.unity3d.com/2019.2/Documentation/uploads/SL/CameraRenderFlowCmdBuffers.png
		 */
		public abstract CameraEvent GetCameraEvent();
		/**
		 * \brief カメラの深度テクスチャレンダリングモードを取得します。
		 * \return 深度テクスチャモードが返ります。
		 */
		public abstract DepthTextureMode GetDepthTextureMode();
		/**
		 * \brief High Dynamic Range が必要かどうかを確認します。
		 * \return 確認結果。以下の値が返ります。
		 * \retval true HDRが必要な場合に返ります。
		 * \retval false HDRが不要な場合に返ります。
		 */
		public abstract bool IsHighDynamicRange();
		
		public abstract void BuildCommandBuffer( 
			CommandBuffer commandBuffer, TargetContext context, 
			System.Func<int, int, int, FilterMode, RenderTextureFormat, int> GetTemporaryRT);
		
		/**
		 * \brief ステンシルのハッシュ値を取得する。
		 * \return ステンシルのハッシュが返ります。
		 */
		public virtual long GetDepthStencilHashCode()
		{
			return DepthStencil.kDefaultHash;
		}
		protected virtual bool OnDuplicate()
		{
			return false;
		}
		internal void Initialize( RenderPipeline pipeline)
		{
			this.pipeline = pipeline;
		}
		internal bool DuplicateMRT()
		{
			if( SystemInfo.supportedRenderTargetCount > 1)
			{
				return OnDuplicate();
			}
			return false;
		}
		internal void SetNextProcess( PostProcess nextProcess)
		{
			this.nextProcess = nextProcess;
		}
		
		protected static readonly int kShaderPropertyMainTex = Shader.PropertyToID( "_MainTex");
		protected static readonly int kShaderPropertyColor = Shader.PropertyToID( "_Color");
		
		protected RenderPipeline pipeline
	#if UNITY_EDITOR
			{ get; private set; }
	#else
			;
	#endif
		protected PostProcess nextProcess
	#if UNITY_EDITOR
			{ get; private set; }
	#else
			;
	#endif
	}
}
