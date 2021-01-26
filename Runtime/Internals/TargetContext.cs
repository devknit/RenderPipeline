
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderingPipeline
{
	public enum TargetType
	{
		kSource0,
		kTarget0,
		kSource1,
		kTarget1
	}
	public sealed class TargetContext
	{
		public bool ConfirmUsePropertyId( int propertyId)
		{
			return	source0propertyId == propertyId
			||		source1propertyId == propertyId
			||		target0propertyId == propertyId
			||		target1propertyId == propertyId;
		}
		public void SetBuffers()
		{
			colorBuffer = new RenderTargetIdentifier( BuiltinRenderTextureType.CameraTarget);
			depthBuffer = new RenderTargetIdentifier( BuiltinRenderTextureType.CameraTarget);
		}
		public void SetBuffers( RenderTexture color, RenderTexture depth)
		{
			colorBuffer = new RenderTargetIdentifier( color);
			depthBuffer = new RenderTargetIdentifier( depth);
		}
		public void SetSource0( BuiltinRenderTextureType type)
		{
			source0 = new RenderTargetIdentifier( type);
			source0type = type;
			source0texture = null;
			source0propertyId = null;
		}
		public void SetSource0( Texture texture)
		{
			source0 = new RenderTargetIdentifier( texture);
			source0type = null;
			source0texture = texture;
			source0propertyId = null;
		}
		public void SetSource0( int propertyId)
		{
			source0 = new RenderTargetIdentifier( propertyId);
			source0type = null;
			source0texture = null;
			source0propertyId = propertyId;
		}
		public void SetTarget0( BuiltinRenderTextureType type)
		{
			target0 = new RenderTargetIdentifier( type);
			target0type = type;
			target0texture = null;
			target0propertyId = null;
		}
		public void SetTarget0( Texture texture)
		{
			target0 = new RenderTargetIdentifier( texture);
			target0type = null;
			target0texture = texture;
			target0propertyId = null;
		}
		public void SetTarget0( int propertyId)
		{
			target0 = new RenderTargetIdentifier( propertyId);
			target0type = null;
			target0texture = null;
			target0propertyId = propertyId;
		}
		public void SetTarget1( BuiltinRenderTextureType type)
		{
			target1 = new RenderTargetIdentifier( type);
			target1type = type;
			target1texture = null;
			target1propertyId = null;
		}
		public void SetTarget1( Texture texture)
		{
			target1 = new RenderTargetIdentifier( texture);
			target1type = null;
			target1texture = texture;
			target1propertyId = null;
		}
		public void SetTarget1( int propertyId)
		{
			target1 = new RenderTargetIdentifier( propertyId);
			target1type = null;
			target1texture = null;
			target1propertyId = propertyId;
		}
		public bool CompareSource0ToTarget0()
		{
			return ( source0 == target0
			&&	source0type == target0type
			&&	source0texture == target0texture
			&&	source0propertyId == target0propertyId);
		}
		public void Next()
		{
			source0 = target0;
			source0type = target0type;
			source0texture = target0texture;
			source0propertyId = target0propertyId;
			source1 = target1;
			source1type = target1type;
			source1texture = target1texture;
			source1propertyId = target1propertyId;
		}
		public void Copy( TargetType dst, TargetType src)
		{
			switch( dst)
			{
				case TargetType.kSource0:
				{
					switch( src)
					{
						case TargetType.kTarget0:
						{
							source0 = target0;
							source0type = target0type;
							source0texture = target0texture;
							source0propertyId = target0propertyId;
							break;
						}
						case TargetType.kSource1:
						{
							source0 = source1;
							source0type = source1type;
							source0texture = source1texture;
							source0propertyId = source1propertyId;
							break;
						}
						case TargetType.kTarget1:
						{
							source0 = target1;
							source0type = target1type;
							source0texture = target1texture;
							source0propertyId = target1propertyId;
							break;
						}
					}
					break;
				}
				case TargetType.kTarget0:
				{
					switch( src)
					{
						case TargetType.kSource0:
						{
							target0 = source0;
							target0type = source0type;
							target0texture = source0texture;
							target0propertyId = source0propertyId;
							break;
						}
						case TargetType.kSource1:
						{
							target0 = source1;
							target0type = source1type;
							target0texture = source1texture;
							target0propertyId = source1propertyId;
							break;
						}
						case TargetType.kTarget1:
						{
							target0 = target1;
							target0type = target1type;
							target0texture = target1texture;
							target0propertyId = target1propertyId;
							break;
						}
					}
					break;
				}
				case TargetType.kSource1:
				{
					switch( src)
					{
						case TargetType.kSource0:
						{
							source1 = source0;
							source1type = source0type;
							source1texture = source0texture;
							source1propertyId = source0propertyId;
							break;
						}
						case TargetType.kTarget0:
						{
							source1 = target0;
							source1type = target0type;
							source1texture = target0texture;
							source1propertyId = target0propertyId;
							break;
						}
						case TargetType.kTarget1:
						{
							source1 = target1;
							source1type = target1type;
							source1texture = target1texture;
							source1propertyId = target1propertyId;
							break;
						}
					}
					break;
				}
				case TargetType.kTarget1:
				{
					switch( src)
					{
						case TargetType.kSource0:
						{
							target1 = source0;
							target1type = source0type;
							target1texture = source0texture;
							target1propertyId = source0propertyId;
							break;
						}
						case TargetType.kTarget0:
						{
							target1 = target0;
							target1type = target0type;
							target1texture = target0texture;
							target1propertyId = target0propertyId;
							break;
						}
						case TargetType.kSource1:
						{
							target1 = source1;
							target1type = source1type;
							target1texture = source1texture;
							target1propertyId = source1propertyId;
							break;
						}
					}
					break;
				}
			}
		}
		
		public RenderTargetIdentifier colorBuffer
#if UNITY_EDITOR
		{ get; private set; }
#else
		;
#endif
		public RenderTargetIdentifier depthBuffer
#if UNITY_EDITOR
		{ get; private set; }
#else
		;
#endif
		
		public RenderTargetIdentifier source0
#if UNITY_EDITOR
		{ get; private set; }
#else
		;
#endif
		BuiltinRenderTextureType? source0type;
		Texture source0texture;
		int? source0propertyId;
		
		public RenderTargetIdentifier source1
#if UNITY_EDITOR
		{ get; private set; }
#else
		;
#endif
		BuiltinRenderTextureType? source1type;
		Texture source1texture;
		int? source1propertyId;
		
		public RenderTargetIdentifier target0
#if UNITY_EDITOR
		{ get; private set; }
#else
		;
#endif
		BuiltinRenderTextureType? target0type;
		Texture target0texture;
		int? target0propertyId;
		
		public RenderTargetIdentifier target1
#if UNITY_EDITOR
		{ get; private set; }
#else
		;
#endif
		BuiltinRenderTextureType? target1type;
		Texture target1texture;
		int? target1propertyId;
		
		public bool duplicated;
	}
}
