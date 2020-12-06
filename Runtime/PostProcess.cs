
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
		public abstract bool Enabled
		{
			get;
		}
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
		internal bool DuplicateMRT()
		{
			if( SystemInfo.supportedRenderTargetCount > 1)
			{
				return false; // OnDuplicateMRT();
			}
			return false;
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
	public abstract class GenericProcess<TSettings, TProperties> : PostProcess
		where TSettings : Settings<TProperties>
		where TProperties : IGenericProperties
	{
		public override bool Enabled
		{
			get{ return ((sharedSettings != null)? sharedSettings.properties : properties).Enabled; }
		}
		public TProperties Properties
		{
			get{ return (sharedSettings != null && useSharedProperties != false)? sharedSettings.properties : properties; }
		}
		public override void Create()
		{
			if( shader != null && material == null)
			{
				material = new Material( shader);
			}
		}
		public override void Dispose()
		{
			if( material != null)
			{
				ObjectUtility.Release( material);
				material = null;
			}
		}
		public override bool RestoreMaterials()
		{
			bool rebuild = false;
			
			if( shader != null && material == null)
			{
				material = new Material( shader);
				rebuild = true;
			}
			return rebuild;
		}
		public override bool Valid()
		{
			return Enabled != false && material != null;
		}
		public override void ClearPropertiesCache()
		{
			sharedSettings?.properties.ClearCache();
			properties.ClearCache();
		}
		
		[SerializeField]
		TSettings sharedSettings = default;
		[SerializeField]
		TProperties properties = default;
		[SerializeField]
		bool useSharedProperties = true;
		[SerializeField]
		Shader shader = default;
		[System.NonSerialized]
		protected Material material;
	}
	public abstract class UbarProperty : PostProcess
	{
		public override void ClearPropertiesCache()
		{
			cacheIndependent = null;
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
		
		[System.NonSerialized]
		bool? cacheIndependent;
	}
	public abstract class UbarPropertyEx<TSettings, TProperties> : UbarProperty
		where TSettings : Settings<TProperties>
		where TProperties : IUbarProperties
	{
		public override bool Enabled
		{
			get{ return ((sharedSettings != null)? sharedSettings.properties : properties).Enabled; }
		}
		public TProperties Properties
		{
			get{ return (sharedSettings != null && useSharedProperties != false)? sharedSettings.properties : properties; }
		}
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
			base.ClearPropertiesCache();
			sharedSettings?.properties.ClearCache();
			properties.ClearCache();
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
		internal override IUbarProperties GetProperties()
		{
			return Properties;
		}
		
		[SerializeField]
        protected TSettings sharedSettings = default;
        [SerializeField]
        protected TProperties properties = default;
        [SerializeField]
		protected bool useSharedProperties = true;
	}
	public abstract class UbarPropertyRx<TSettings, TProperties> : UbarPropertyEx<TSettings, TProperties>
		where TSettings : Settings<TProperties>
		where TProperties : IUbarProperties
	{
		public override void Create()
		{
			if( shader != null && material == null)
			{
				material = new Material( shader);
			}
		}
		public override void Dispose()
		{
			if( material != null)
			{
				ObjectUtility.Release( material);
				material = null;
			}
		}
		public override bool RestoreMaterials()
		{
			bool rebuild = false;
			
			if( shader != null && material == null)
			{
				material = new Material( shader);
				rebuild = true;
			}
			return rebuild;
		}
		public override bool Valid()
		{
			return Enabled != false && material != null;
		}
		
		[SerializeField]
		Shader shader = default;
		[System.NonSerialized]
		protected Material material;
	}
}
