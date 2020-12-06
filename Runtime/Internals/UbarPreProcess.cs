
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipeline
{
	public abstract class UbarPreProcess<TSettings, TProperties> : UbarProcess<TSettings, TProperties>
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
		public sealed override bool UpdateProperties( RenderPipeline pipeline, bool clearCache)
		{
			if( clearCache != false)
			{
				Properties.ClearCache();
			}
			return Properties.UpdateProperties( material, false);
		}
		public sealed override bool PreProcess()
		{
			return true;
		}
		
		[SerializeField]
		Shader shader = default;
		[System.NonSerialized]
		protected Material material;
	}
}
