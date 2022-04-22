
using UnityEngine;
using System.Collections.Generic;

namespace RenderingPipeline
{
	public sealed class GaussianBlurResources
	{
		public void Create()
		{
			brightnessExtractionMesh = new Mesh();
			brightnessExtractionMesh.MarkDynamic();
			blurHorizontalMesh = new Mesh();
			blurHorizontalMesh.MarkDynamic();
			blurVerticalMesh = new Mesh();
			blurVerticalMesh.MarkDynamic();
			combineMesh = new Mesh();
			combineMesh.MarkDynamic();
		}
		public void Dispose()
		{
			if( brightnessExtractionMesh != null)
			{
				ObjectUtility.Release( brightnessExtractionMesh);
				brightnessExtractionMesh = null;
			}
			if( blurHorizontalMesh != null)
			{
				ObjectUtility.Release( blurHorizontalMesh);
				blurHorizontalMesh = null;
			}
			if( blurVerticalMesh != null)
			{
				ObjectUtility.Release( blurVerticalMesh);
				blurVerticalMesh = null;
			}
			if( combineMesh != null)
			{
				ObjectUtility.Release( combineMesh);
				combineMesh = null;
			}
		}
		public void UpdateSigmaInPixel( Material material, float sigmaInPixel)
		{
			material.SetFloat( kShaderPropertyInvertOffsetScale01, CalculateGaussianSamples( sigmaInPixel));
		}
		public void UpdateDescriptors( Material material, int width, int height, 
			RenderTextureFormat renderTextureFormat, int downSampleLevel, int downSampleCount, int combineStartLevel)
		{
			if( width > 0 && height > 0)
			{
				if( SystemInfo.SupportsRenderTextureFormat( renderTextureFormat) == false)
				{
					renderTextureFormat = RenderTextureFormat.ARGB32;
				}
				int topBlurWidth = width >> downSampleLevel;
				int topBlurHeight = height >> downSampleLevel;
				int blurWidth, blurHeight;
				
				brightnessExtractionDescriptor = new RenderTextureDescriptor(
					TextureUtil.ToPow2RoundUp( topBlurWidth), 
					TextureUtil.ToPow2RoundUp( topBlurHeight), 
					renderTextureFormat, 0);
				brightnessExtractionDescriptor.useMipMap = true;
				brightnessExtractionDescriptor.autoGenerateMips = true;
				brightnessNetWidth = width >> downSampleLevel;
				brightnessNetHeight = height >> downSampleLevel;
				brightnessOffsetX = (brightnessExtractionDescriptor.width - brightnessNetWidth) / 2;
				brightnessOffsetY = (brightnessExtractionDescriptor.height - brightnessNetHeight) / 2;
				
				blurRects = CalculateMipmapArrangement(
					out blurWidth,
					out blurHeight,
					topBlurWidth,
					topBlurHeight,
					16,
					downSampleCount);
				
				blurDescriptor = new RenderTextureDescriptor(
					blurWidth, blurHeight, renderTextureFormat, 0);
				blurDescriptor.useMipMap = false;
				blurDescriptor.autoGenerateMips = false;
				
				combineDescriptor = new RenderTextureDescriptor(
					blurWidth, blurHeight, renderTextureFormat, 0);
				combineDescriptor.useMipMap = false;
				combineDescriptor.autoGenerateMips = false;
				
				combinePassCount = (blurRects.Length + downSampleLevel) - combineStartLevel;
				combinePassCount = Mathf.Clamp( combinePassCount, 0, blurRects.Length);
				blurRectCount = blurRects.Length - combinePassCount;
				blurRectCount = Mathf.Clamp( blurRectCount, 0, blurRects.Length - 1);
				
				UpdateBrightnessExtractionMesh();
				
				if( combinePassCount > 0)
				{
					UpdateCombineMesh();
				}
				if( cacheCombinePassCount != combinePassCount)
				{
					/* Combine */
					if( (combinePassCount & 0x4) != 0)
					{
						if( material.IsKeywordEnabled( kShaderKeywordCombineSample4) == false)
						{
							material.EnableKeyword( kShaderKeywordCombineSample4);
						}
					}
					else if( material.IsKeywordEnabled( kShaderKeywordCombineSample4) != false)
					{
						material.DisableKeyword( kShaderKeywordCombineSample4);
					}
					if( (combinePassCount & 0x2) != 0)
					{
						if( material.IsKeywordEnabled( kShaderKeywordCombineSample2) == false)
						{
							material.EnableKeyword( kShaderKeywordCombineSample2);
						}
					}
					else if( material.IsKeywordEnabled( kShaderKeywordCombineSample2) != false)
					{
						material.DisableKeyword( kShaderKeywordCombineSample2);
					}
					if( (combinePassCount & 0x1) != 0)
					{
						if( material.IsKeywordEnabled( kShaderKeywordCombineSample1) == false)
						{
							material.EnableKeyword( kShaderKeywordCombineSample1);
						}
					}
					else if( material.IsKeywordEnabled( kShaderKeywordCombineSample1) != false)
					{
						material.DisableKeyword( kShaderKeywordCombineSample1);
					}
					
					/* Composition */
					if( combinePassCount > 0)
					{
						if( material.IsKeywordEnabled( kShaderKeywordCompositionCombined) == false)
						{
							material.EnableKeyword( kShaderKeywordCompositionCombined);
						}
					}
					else if( material.IsKeywordEnabled( kShaderKeywordCompositionCombined) != false)
					{
						material.DisableKeyword( kShaderKeywordCompositionCombined);
					}
					cacheCombinePassCount = combinePassCount;
				}
				if( cacheBlurRectCount != blurRectCount)
				{
					/* Composition */
					if( (blurRectCount & 0x4) != 0)
					{
						if( material.IsKeywordEnabled( kShaderKeywordCompositionSample4) == false)
						{
							material.EnableKeyword( kShaderKeywordCompositionSample4);
						}
					}
					else if( material.IsKeywordEnabled( kShaderKeywordCompositionSample4) != false)
					{
						material.DisableKeyword( kShaderKeywordCompositionSample4);
					}
					if( (blurRectCount & 0x2) != 0)
					{
						if( material.IsKeywordEnabled( kShaderKeywordCompositionSample2) == false)
						{
							material.EnableKeyword( kShaderKeywordCompositionSample2);
						}
					}
					else if( material.IsKeywordEnabled( kShaderKeywordCompositionSample2) != false)
					{
						material.DisableKeyword( kShaderKeywordCompositionSample2);
					}
					if( (blurRectCount & 0x1) != 0)
					{
						if( material.IsKeywordEnabled( kShaderKeywordCompositionSample1) == false)
						{
							material.EnableKeyword( kShaderKeywordCompositionSample1);
						}
					}
					else if( material.IsKeywordEnabled( kShaderKeywordCompositionSample1) != false)
					{
						material.DisableKeyword( kShaderKeywordCompositionSample1);
					}
					cacheBlurRectCount = blurRectCount;
				}
			}
		}
		public void UpdateCombineComposition( Material material, float intensity, float intensityMultiplier)
		{
			/* Combine */
			BlurRect toRect = blurRects[ blurRectCount];
			float combinedStrengthSum = 0.0f;
			float combinedStrength = 1.0f;
			
			for( int i0 = 0; i0 < combinePassCount; ++i0)
			{
				combinedStrengthSum += combinedStrength;
				combinedStrength *= intensityMultiplier;
			}
			float normalizeFactor = 1.0f / combinedStrengthSum;
			combinedStrength = 1.0f;
			
			for( int i0 = 0; i0 < combinePassCount; ++i0)
			{
				int fromLevel = blurRectCount + i0;
				var fromRect = blurRects[ fromLevel];
				
				material.SetFloat( 
					kShaderPropertyWeights[ i0], 
					combinedStrength * normalizeFactor);
				
				Vector4 uvTransform;
				uvTransform.x = (float)fromRect.width / (float)toRect.width;
				uvTransform.y = (float)fromRect.height / (float)toRect.height;
				uvTransform.z = ((float)fromRect.x - ((float)toRect.x * uvTransform.x)) / (float)blurDescriptor.width;
				uvTransform.w = ((float)fromRect.y - ((float)toRect.y * uvTransform.y)) / (float)blurDescriptor.height;
				material.SetVector( kShaderPropertyUvTransforms[ i0], uvTransform);
				combinedStrength *= intensityMultiplier;
			}
				
			/* Composition */
			float compositeStrengthSum = 0.0f;
			float compositeStrength = 1.0f;
			
			for( int i0 = 0; i0 < blurRects.Length; ++i0)
			{
				compositeStrengthSum += compositeStrength;
				compositeStrength *= intensityMultiplier;
			}
			compositeStrength = intensity / compositeStrengthSum;
			
			for( int i0 = 0; i0 < blurRectCount; ++i0)
			{
				var rect = blurRects[ i0];
				material.SetFloat( rect.weightShaderPropertyId, compositeStrength);
				material.SetVector( 
					rect.uvTransformShaderPropertyId, 
					new Vector4(
						(float)rect.width / (float)blurDescriptor.width,
						(float)rect.height / (float)blurDescriptor.height,
						(float)rect.x / (float)blurDescriptor.width,
						(float)rect.y / (float)blurDescriptor.height));
				compositeStrength *= intensityMultiplier;
			}
			if( combinePassCount > 0)
			{
				var rect = blurRects[ blurRectCount];
				float weight = compositeStrength * combinedStrengthSum;
				material.SetFloat( kShaderPropertyBlurWeightCombined, weight);
				material.SetVector( kShaderPropertyBlurUvTransformCombined, 
					new Vector4(
						(float)rect.width / (float)blurDescriptor.width,
						(float)rect.height / (float)blurDescriptor.height,
						(float)rect.x / (float)blurDescriptor.width,
						(float)rect.y / (float)blurDescriptor.height));
			}
		}
		void UpdateBrightnessExtractionMesh()
		{
			brightnessExtractionMesh.Clear();
			
			float x0 = (float)brightnessOffsetX / (float)brightnessExtractionDescriptor.width;
			float x1 = (float)(brightnessOffsetX + brightnessNetWidth) / (float)brightnessExtractionDescriptor.width;
			float y0 = (float)brightnessOffsetY / (float)brightnessExtractionDescriptor.height;
			float y1 = (float)(brightnessOffsetY + brightnessNetHeight) / (float)brightnessExtractionDescriptor.height;
		#if false
			float w1 = 8.0f / (float)brightnessExtractionDescriptor.width;
			float h1 = 8.0f / (float)brightnessExtractionDescriptor.height;
			float w2 = w1 * 0.5f;
			float h2 = h1 * 0.5f;
			float u0 = Mathf.Max( 0.0f, x0 - w2);
			float u1 = Mathf.Min( 1.0f, u0 + w1);
			float u3 = Mathf.Min( 1.0f, x1 + w2);
			float u2 = Mathf.Max( 0.0f, u3 - w1);
			float v0 = Mathf.Max( 0.0f, y0 - h2);
			float v1 = Mathf.Min( 1.0f, v0 + h1);
			float v3 = Mathf.Min( 1.0f, y1 + h2);
			float v2 = Mathf.Max( 0.0f, v3 - h1);
			
			brightnessExtractionMesh.SetVertices(
				new Vector3[]{
					new Vector3( u0, v0, 0), new Vector3( u0, v1, 0), new Vector3( u0, v2, 0), new Vector3( u0, v3, 0),
					new Vector3( u1, v0, 0), new Vector3( u1, v3, 0), new Vector3( u2, v0, 0), new Vector3( u2, v3, 0),
					new Vector3( u3, v0, 0), new Vector3( u3, v1, 0), new Vector3( u3, v2, 0), new Vector3( u3, v3, 0),
					new Vector3( x0, y0, 0), new Vector3( x0, y1, 0), new Vector3( x1, y1, 0), new Vector3( x1, y0, 0)
				});
			brightnessExtractionMesh.SetColors(
				new Color[]{
					Color.clear, Color.clear, Color.clear, Color.clear,
					Color.clear, Color.clear, Color.clear, Color.clear,
					Color.clear, Color.clear, Color.clear, Color.clear,
					Color.white, Color.white, Color.white, Color.white
				});
			brightnessExtractionMesh.SetUVs( 
				0,
				new Vector2[]{
					Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero,
					Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero,
					Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero,
					Vector2.zero, Vector2.up, Vector2.one, Vector2.right   
				});
			brightnessExtractionMesh.SetIndices(
				new int[]{
					0, 1, 9, 8,
					0, 3, 5, 4,
					2, 3, 11, 10,
					6, 7, 11, 8,
					12, 13, 14, 15,
				}, MeshTopology.Quads, 0);
		#else
			brightnessExtractionMesh.SetVertices(
				new Vector3[]{
					new Vector3( x0, y0, 0),
					new Vector3( x0, y1, 0),
					new Vector3( x1, y1, 0),
					new Vector3( x1, y0, 0)
				});
			brightnessExtractionMesh.SetColors(
				new Color[]{ Color.white, Color.white, Color.white, Color.white });
			brightnessExtractionMesh.SetUVs( 
				0, new Vector2[]{ Vector2.zero, Vector2.up, Vector2.one, Vector2.right });
			brightnessExtractionMesh.SetIndices(
				new int[]{ 0, 1, 2, 3 }, MeshTopology.Quads, 0);
		#endif
			brightnessExtractionMesh.UploadMeshData( false);
		}
		public void UpdateGaussianBlurHorizontalMesh()
		{
			int vertexCount = blurRects.Length * 4;
			var vertex = new Vector3[ vertexCount];
			var uv0 = new Vector3[ vertexCount];
			var uv1 = new Vector3[ vertexCount];
			var uv2 = new Vector3[ vertexCount];
			var uv3 = new Vector3[ vertexCount];
			var indices = new int[ vertexCount];
			int vertexIndex = 0;
			
			int scale = brightnessExtractionDescriptor.width;
			
			for( int i0 = 0; i0 < blurRects.Length; ++i0)
			{
				var rect = blurRects[ i0];
				
				UpdateGaussianBlurMesh( 
					vertexIndex, vertex, uv0, uv1, uv2, uv3,
					brightnessExtractionDescriptor, brightnessOffsetX, brightnessOffsetY, brightnessNetWidth, brightnessNetHeight,
					blurDescriptor, rect.x, rect.y, rect.width, rect.height, 1.0f / (float)scale, true);
					
				for( int i1 = 0; i1 < 4; ++i1)
				{
					indices[ vertexIndex] = vertexIndex;
					++vertexIndex;
				}
				scale /= 2;
			}
			blurHorizontalMesh.Clear();
			blurHorizontalMesh.SetVertices( vertex);
			blurHorizontalMesh.SetUVs( 0, uv0);
			blurHorizontalMesh.SetUVs( 1, uv1);
			blurHorizontalMesh.SetUVs( 2, uv2);
			blurHorizontalMesh.SetUVs( 3, uv3);
			blurHorizontalMesh.SetIndices( indices, MeshTopology.Quads, 0);
			blurHorizontalMesh.UploadMeshData( false);
		}
		public void UpdateGaussianBlurVerticalMesh()
		{
			int vertexCount = blurRects.Length * 4;
			var vertex = new Vector3[ vertexCount];
			var uv0 = new Vector3[ vertexCount];
			var uv1 = new Vector3[ vertexCount];
			var uv2 = new Vector3[ vertexCount];
			var uv3 = new Vector3[ vertexCount];
			var indices = new int[ vertexCount];
			int vertexIndex = 0;
			
			for( int i0 = 0; i0 < blurRects.Length; ++i0)
			{
				var rect = blurRects[ i0];
				
				UpdateGaussianBlurMesh( 
					vertexIndex, vertex, uv0, uv1, uv2, uv3,
					blurDescriptor, rect.x, rect.y, rect.width, rect.height,
					blurDescriptor, rect.x, rect.y, rect.width, rect.height, 
					1.0f / (float)blurDescriptor.height, false);
					
				for( int i1 = 0; i1 < 4; ++i1)
				{
					indices[ vertexIndex] = vertexIndex;
					++vertexIndex;
				}
			}
			blurVerticalMesh.Clear();
			blurVerticalMesh.SetVertices( vertex);
			blurVerticalMesh.SetUVs( 0, uv0);
			blurVerticalMesh.SetUVs( 1, uv1);
			blurVerticalMesh.SetUVs( 2, uv2);
			blurVerticalMesh.SetUVs( 3, uv3);
			blurVerticalMesh.SetIndices( indices, MeshTopology.Quads, 0);
			blurVerticalMesh.UploadMeshData( false);
		}
		void UpdateGaussianBlurMesh( int vtxIndex,
			Vector3[] vtx, Vector3[] uv0, Vector3[] uv1, Vector3[] uv2, Vector3[] uv3, 
			RenderTextureDescriptor from, int fromX, int fromY, int fromWidth, int fromHeight,
			RenderTextureDescriptor to, int toX, int toY, int toWidth, int toHeight,
			float offsetScale, bool forX)
		{
			float x0 = (float)toX / (float)to.width;
			float x1 = (float)(toX + toWidth) / (float)to.width;
			float y0 = (float)toY / (float)to.height;
			float y1 = (float)(toY + toHeight) / (float)to.height;
			
			float u0 = (float)fromX / (float)from.width;
			float u1 = (float)(fromX + fromWidth) / (float)from.width;
			float v0 = (float)fromY / (float)from.height;
			float v1 = (float)(fromY + fromHeight) / (float)from.height;
			
			float uOffset0 = blurSample0.offset * offsetScale;
			float vOffset0 = blurSample0.offset * offsetScale;
			float uOffset1 = blurSample1.offset * offsetScale;
			float vOffset1 = blurSample1.offset * offsetScale;
			float uOffset2 = blurSample2.offset * offsetScale;
			float vOffset2 = blurSample2.offset * offsetScale;
			float uOffset3 = blurSample3.offset * offsetScale;
			float vOffset3 = blurSample3.offset * offsetScale;
			
			if( forX != false)
			{
				vOffset0 = vOffset1 = vOffset2 = vOffset3 = 0f;
			}
			else
			{
				uOffset0 = uOffset1 = uOffset2 = uOffset3 = 0f;
			}
			vtx[ vtxIndex + 0] = new Vector3( x0, y0, 0);
			uv0[ vtxIndex + 0] = new Vector3( u0 + uOffset0, v0 + vOffset0, blurSample0.weight);
			uv1[ vtxIndex + 0] = new Vector3( u0 + uOffset1, v0 + vOffset1, blurSample1.weight);
			uv2[ vtxIndex + 0] = new Vector3( u0 + uOffset2, v0 + vOffset2, blurSample2.weight);
			uv3[ vtxIndex + 0] = new Vector3( u0 + uOffset3, v0 + vOffset3, blurSample3.weight);
			
			vtx[ vtxIndex + 1] = new Vector3( x0, y1, 0);
			uv0[ vtxIndex + 1] = new Vector3( u0 + uOffset0, v1 + vOffset0, blurSample0.weight);
			uv1[ vtxIndex + 1] = new Vector3( u0 + uOffset1, v1 + vOffset1, blurSample1.weight);
			uv2[ vtxIndex + 1] = new Vector3( u0 + uOffset2, v1 + vOffset2, blurSample2.weight);
			uv3[ vtxIndex + 1] = new Vector3( u0 + uOffset3, v1 + vOffset3, blurSample3.weight);
			
			vtx[ vtxIndex + 2] = new Vector3( x1, y1, 0);
			uv0[ vtxIndex + 2] = new Vector3( u1 + uOffset0, v1 + vOffset0, blurSample0.weight);
			uv1[ vtxIndex + 2] = new Vector3( u1 + uOffset1, v1 + vOffset1, blurSample1.weight);
			uv2[ vtxIndex + 2] = new Vector3( u1 + uOffset2, v1 + vOffset2, blurSample2.weight);
			uv3[ vtxIndex + 2] = new Vector3( u1 + uOffset3, v1 + vOffset3, blurSample3.weight);
			
			vtx[ vtxIndex + 3] = new Vector3( x1, y0, 0);
			uv0[ vtxIndex + 3] = new Vector3( u1 + uOffset0, v0 + vOffset0, blurSample0.weight);
			uv1[ vtxIndex + 3] = new Vector3( u1 + uOffset1, v0 + vOffset1, blurSample1.weight);
			uv2[ vtxIndex + 3] = new Vector3( u1 + uOffset2, v0 + vOffset2, blurSample2.weight);
			uv3[ vtxIndex + 3] = new Vector3( u1 + uOffset3, v0 + vOffset3, blurSample3.weight);
		}
		void UpdateCombineMesh()
		{
			BlurRect toRect = blurRects[ blurRects.Length - combinePassCount];
			float x0 = (float)toRect.x / (float)combineDescriptor.width;
			float x1 = (float)(toRect.x + toRect.width) / (float)combineDescriptor.width;
			float y0 = (float)toRect.y / (float)combineDescriptor.height;
			float y1 = (float)(toRect.y + toRect.height) / (float)combineDescriptor.height;
			
			combineMesh.Clear();
			combineMesh.SetVertices(
				new Vector3[]{
					new Vector3( x0, y0, 0),
					new Vector3( x0, y1, 0),
					new Vector3( x1, y1, 0),
					new Vector3( x1, y0, 0)
				});
			combineMesh.SetIndices(
				new int[]{ 0, 1, 2, 3 }, MeshTopology.Quads, 0);
			combineMesh.UploadMeshData( false);
		}
		BlurRect[] CalculateMipmapArrangement(
			out int dstWidth,
			out int dstHeight,
			int width,
			int height,
			int padding,
			int levelCount)
		{
			var rects = new List<BlurRect>();
			bool right = height > width;
			int x = padding;
			int y = padding;
			int maxX = 0;
			int maxY = 0;
			
			while( levelCount > 0 && width > 0 && height > 0)
			{
				var rect = new BlurRect();
				rect.x = x;
				rect.y = y;
				rect.width = width;
				rect.height = height;
				rect.uvTransformShaderPropertyId = Shader.PropertyToID( "_BlurUvTransform" + rects.Count);
				rect.weightShaderPropertyId = Shader.PropertyToID( "_BlurWeight" + rects.Count);
				rects.Add( rect);
				maxX = Mathf.Max( maxX, x + width + padding);
				maxY = Mathf.Max( maxY, y + height + padding);
				if( right != false)
				{
					x += width + padding;
				}
				else
				{
					y += height + padding;
				}
				right = !right;
				
				width /= 2;
				height /= 2;
				
				if( width < 0 || height < 0)
				{
					break;
				}
				levelCount--;
			}
			dstWidth = maxX;
			dstHeight = maxY;
			
			return rects.ToArray();
		}
		float CalculateGaussianSamples( float sigma)
		{
			float w0 = Gauss( sigma, 0.0f) * 0.5f;
			float w1 = Gauss( sigma, 1.0f);
			float w2 = Gauss( sigma, 2.0f);
			float w3 = Gauss( sigma, 3.0f);
			float w4 = Gauss( sigma, 4.0f);
			float w5 = Gauss( sigma, 5.0f);
			float w6 = Gauss( sigma, 6.0f);
			float w7 = Gauss( sigma, 7.0f);
			
			float w01 = w0 + w1;
			float x01 = (w01 != 0.0f)? 0.0f + (w1 / w01) : 0.0f;
			float w23 = w2 + w3;
			float x23 = (w23 != 0.0f)? 2.0f + (w3 / w23) : 0.0f;
			float w45 = w4 + w5;
			float x45 = (w45 != 0.0f)? 4.0f + (w5 / w45) : 0.0f;
			float w67 = w6 + w7;
			float x67 = (w67 != 0.0f)? 6.0f + (w7 / w67) : 0.0f;
			float wSum = (w01 + w23 + w45 + w67) * 2.0f;
			float iSum = (wSum != 0.0f)? 1.0f / wSum : 0.0f;
			
			w01 *= iSum;
			w23 *= iSum;
			w45 *= iSum;
			w67 *= iSum;
			
			blurSample0.offset = x01;
			blurSample0.weight = w01;
			blurSample1.offset = x23;
			blurSample1.weight = w23;
			blurSample2.offset = x45;
			blurSample2.weight = w45;
			blurSample3.offset = x67;
			blurSample3.weight = w67;
			
			float x0123 = x01 - x23;
			return (x0123 != 0.0f)? (x01 * 2.0f) / Mathf.Abs( x0123) : 0.0f;
		}
		static float Gauss( float sigma, float x)
		{
			if( sigma == 0.0)
			{
				return 0.0f;
			}
			return Mathf.Exp( -(x * x) / (sigma * sigma * 2.0f));
		}
		sealed class BlurRect
		{
			public int x;
			public int y;
			public int width;
			public int height;
			public int uvTransformShaderPropertyId;
			public int weightShaderPropertyId;
		}
		struct BlurSample
		{
			public float offset;
			public float weight;
		}
		const string kShaderKeywordCombineSample1 = "COMBINE_SAMPLE1";
		const string kShaderKeywordCombineSample2 = "COMBINE_SAMPLE2";
		const string kShaderKeywordCombineSample4 = "COMBINE_SAMPLE4";
		const string kShaderKeywordCompositionSample1 = "COMPOSITION_SAMPLE1";
		const string kShaderKeywordCompositionSample2 = "COMPOSITION_SAMPLE2";
		const string kShaderKeywordCompositionSample4 = "COMPOSITION_SAMPLE4";
		const string kShaderKeywordCompositionCombined = "COMPOSITION_COMBINED";
		
		public static readonly int kShaderPropertyBrightnessExtractionTarget = Shader.PropertyToID( "_BrightnessExtractionTarget");
		public static readonly int kShaderPropertyGaussianBlurHorizontalTarget = Shader.PropertyToID( "_GaussianBlurHorizontalTarget");
		public static readonly int kShaderPropertyGaussianBlurVerticalTarget = Shader.PropertyToID( "_GaussianBlurVerticalTarget");
		public static readonly int kShaderPropertyCombineTarget = Shader.PropertyToID( "_CombineTarget");
		public static readonly int kShaderPropertyBlurTex = Shader.PropertyToID( "_BlurTex");
		public static readonly int kShaderPropertyBlurCombinedTex = Shader.PropertyToID( "_BlurCombinedTex");
		public static readonly int kShaderPropertyThresholds = Shader.PropertyToID( "_Thresholds");
		public static readonly int kShaderPropertyColorTransform = Shader.PropertyToID( "_ColorTransform");
		public static readonly int kShaderPropertyInvertOffsetScale01 = Shader.PropertyToID( "_InvertOffsetScale01");
		static readonly int[] kShaderPropertyUvTransforms = new int[]
		{
			Shader.PropertyToID( "_UvTransform0"),
			Shader.PropertyToID( "_UvTransform1"),
			Shader.PropertyToID( "_UvTransform2"),
			Shader.PropertyToID( "_UvTransform3"),
			Shader.PropertyToID( "_UvTransform4"),
			Shader.PropertyToID( "_UvTransform5"),
			Shader.PropertyToID( "_UvTransform6")
		};
		static readonly int[] kShaderPropertyWeights = new int[]
		{
			Shader.PropertyToID( "_Weight0"),
			Shader.PropertyToID( "_Weight1"),
			Shader.PropertyToID( "_Weight2"),
			Shader.PropertyToID( "_Weight3"),
			Shader.PropertyToID( "_Weight4"),
			Shader.PropertyToID( "_Weight5"),
			Shader.PropertyToID( "_Weight6")
		};
		static readonly int kShaderPropertyBlurWeightCombined = Shader.PropertyToID( "_BlurWeightCombined");
		static readonly int kShaderPropertyBlurUvTransformCombined = Shader.PropertyToID( "_BlurUvTransformCombined");
		
		public Mesh brightnessExtractionMesh;
		public Mesh blurHorizontalMesh;
		public Mesh blurVerticalMesh;
		public Mesh combineMesh;
		
		public RenderTextureDescriptor brightnessExtractionDescriptor;
		public RenderTextureDescriptor blurDescriptor;
		public RenderTextureDescriptor combineDescriptor;
		
		BlurSample blurSample0;
		BlurSample blurSample1;
		BlurSample blurSample2;
		BlurSample blurSample3;
		BlurRect[] blurRects;
		
		int brightnessOffsetX;
		int brightnessOffsetY;
		int brightnessNetWidth;
		int brightnessNetHeight;
		
		public int combinePassCount;
		int? cacheCombinePassCount;
		public int blurRectCount;
		int? cacheBlurRectCount;
	}
}
