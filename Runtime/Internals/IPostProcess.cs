
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderingPipeline
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
		bool UpdateProperties( RenderPipeline pipeline, bool clearCache);
		/**
		 * \brief プロセスを実行するイベントを取得します。
		 * \return カメライベント。以下の値が現状有効値として返ります。
		 * \retval PostProcessEvent.BeforeImageEffectsOpaque
		 * \retval PostProcessEvent.BeforeImageEffects 
		 * \note https://docs.unity3d.com/2019.2/Documentation/uploads/SL/CameraRenderFlowCmdBuffers.png
		 */
		PostProcessEvent GetPostProcessEvent();
		/**
		 * \brief カメラの深度テクスチャレンダリングモードを取得します。
		 * \return 深度テクスチャモードが返ります。
		 */
		DepthTextureMode GetDepthTextureMode();
		/**
		 * \brief High Dynamic Range が必須かどうかを確認します。
		 * \return 確認結果。以下の値が返ります。
		 * \retval true HDRが必要な場合に返ります。
		 * \retval false HDRが不要な場合に返ります。
		 */
		bool IsRequiredHighDynamicRange();
		/**
		 * \brief ステンシルのハッシュ値を取得する。
		 * \return ステンシルのハッシュが返ります。
		 */
		long GetDepthStencilHashCode();
		/**
		 * \brief コマンドを生成する。
		 * \param pipeline [in] パイプライン
		 * \param commandBuffer [in] コマンドバッファ
		 * \param context [in] コンテキスト
		 * \param nextProcess [in] 次に実行されるプロセス
		 */
		void BuildCommandBuffer( 
			RenderPipeline pipeline,
			CommandBuffer commandBuffer, 
			TargetContext context, 
			IPostProcess nextProcess);
	}
}
