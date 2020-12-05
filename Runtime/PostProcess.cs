
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
		
		void BuildCommandBuffer( RenderPipeline pipeline,
			CommandBuffer commandBuffer, TargetContext context, IPostProcess nextProcess);
	}
	public abstract class PostProcess : MonoBehaviour, IPostProcess
	{
		public abstract void Create();
		public abstract void Dispose();
		public abstract bool RestoreMaterials();
		public abstract bool Valid();
		public abstract void ClearPropertiesCache();
		public abstract bool UpdateProperties( RenderPipeline pipeline, bool clearCache);
		public abstract PostProcessEvent GetPostProcessEvent();
		public abstract DepthTextureMode GetDepthTextureMode();
		public abstract bool IsRequiredHighDynamicRange();
		public abstract void BuildCommandBuffer( RenderPipeline pipeline,
			CommandBuffer commandBuffer, TargetContext context, IPostProcess nextProcess);
		
		public virtual long GetDepthStencilHashCode()
		{
			return DepthStencil.kDefaultHash;
		}
		
	#if UNITY_EDITOR
		internal bool ChangePostProcessEvent()
		{
			PostProcessEvent postProcessEvent = GetPostProcessEvent();
			if( cachePostProcessEvent != postProcessEvent)
			{
				cachePostProcessEvent = postProcessEvent;
				return true;
			}
			return false;
		}
		PostProcessEvent? cachePostProcessEvent;
	#endif
	}
	public abstract class InternalProcess : PostProcess
	{
		internal abstract bool Enabled
		{
			get;
		}
		internal bool DuplicateMRT()
		{
			if( SystemInfo.supportedRenderTargetCount > 1)
			{
				return false;
			}
			return false;
		}
	}
	public interface IUbarProperty<T> where T : Properties
	{
		T Properties
		{
			get;
		}
	}
	public abstract class UbarProperty : InternalProcess
	{
		public override void Create()
		{
		}
		public override void Dispose()
		{
		}
		public override bool RestoreMaterials()
		{
			return false;
		}
		public override bool Valid()
		{
			return false;
		}
		public override void ClearPropertiesCache()
		{
			cacheIndependent = null;
		}
		public override bool UpdateProperties( RenderPipeline pipeline, bool clearCache)
		{
			return false;
		}
		public override DepthTextureMode GetDepthTextureMode()
		{
			return DepthTextureMode.None;
		}
		public override bool IsRequiredHighDynamicRange()
		{
			return false;
		}
		public override void BuildCommandBuffer( RenderPipeline pipeline,
			CommandBuffer commandBuffer, TargetContext context, IPostProcess nextProcess)
		{
		}
		internal bool HasIndependent( ref bool rebuild)
		{
			bool independent = Independent();
			
			if( cacheIndependent != independent)
			{
				cacheIndependent = independent;
				ClearPropertiesCache();
				rebuild = true;
			}
			return independent;
		}
		internal virtual bool Independent()
		{
			return DepthStencil.HasIndependent( GetDepthStencilHashCode());
		}
		internal abstract IUbarProperties GetProperties();
		
		bool? cacheIndependent;
	}
}
