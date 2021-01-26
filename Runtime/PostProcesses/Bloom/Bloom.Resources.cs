
using UnityEngine;

namespace RenderingPipeline
{
	public sealed partial class Bloom
	{
		bool UpdateResources( int updateFlags)
		{
			bool rebuild = false;
			
			if( (updateFlags & BloomProperties.kChangeThresholds) != 0)
			{
				if( SystemInfo.SupportsRenderTextureFormat( RenderTextureFormat.ARGBHalf) != false
				||	SystemInfo.SupportsRenderTextureFormat( RenderTextureFormat.ARGBFloat) != false
				||	SystemInfo.SupportsRenderTextureFormat( RenderTextureFormat.RGB111110Float) != false)
				{
					if( material.IsKeywordEnabled( kShaderKeywordLDR) != false)
					{
						material.DisableKeyword( kShaderKeywordLDR);
					}
					material.SetFloat( kShaderPropertyThresholds, Properties.Thresholds);
				}
				else
				{
					if( material.IsKeywordEnabled( kShaderKeywordLDR) == false)
					{
						material.EnableKeyword( kShaderKeywordLDR);
					}
					var colorTransform = new Vector4();
					
					if( Properties.Thresholds >= 1.0f)
					{
						colorTransform.x = 0.0f;
						colorTransform.y = 0.0f;
					}
					else
					{
						colorTransform.x = 1.0f / (1.0f - Properties.Thresholds);
						colorTransform.y = -Properties.Thresholds / (1.0f - Properties.Thresholds);
					}
					material.SetVector( kShaderPropertyColorTransform, colorTransform);
				}
			}
			if( (updateFlags & BloomProperties.kChangeSigmaInPixel) != 0)
			{
				material.SetFloat( kShaderPropertyInvertOffsetScale01, CalculateGaussianSamples( Properties.SigmaInPixel));
				updateFlags |= BloomProperties.kChangeGaussianBlurMesh;
			}
			if( (updateFlags & BloomProperties.kVerifyCombinePassCount) != 0)
			{
				combinePassCount = (bloomRects.Length + Properties.DownSampleLevel) - Properties.CombineStartLevel;
				combinePassCount = Mathf.Clamp( combinePassCount, 0, bloomRects.Length);
				bloomRectCount = bloomRects.Length - combinePassCount;
				bloomRectCount = Mathf.Clamp( bloomRectCount, 0, bloomRects.Length - 1);
				
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
					updateFlags |= BloomProperties.kChangeCombinePassCount;
				}
				if( cacheBloomRectCount != bloomRectCount)
				{
					/* Composition */
					if( (bloomRectCount & 0x4) != 0)
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
					if( (bloomRectCount & 0x2) != 0)
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
					if( (bloomRectCount & 0x1) != 0)
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
					cacheBloomRectCount = bloomRectCount;
					updateFlags |= BloomProperties.kChangeBloomRectCount;
				}
			}
			if( (updateFlags & BloomProperties.kVerifyCombineComposition) != 0)
			{
				/* Combine */
				BloomRect toRect = bloomRects[ bloomRectCount];
				float combinedStrengthSum = 0.0f;
				float combinedStrength = 1.0f;
				
				for( int i0 = 0; i0 < combinePassCount; ++i0)
				{
					combinedStrengthSum += combinedStrength;
					combinedStrength *= Properties.IntensityMultiplier;
				}
				float normalizeFactor = 1.0f / combinedStrengthSum;
				combinedStrength = 1.0f;
				
				for( int i0 = 0; i0 < combinePassCount; ++i0)
				{
					int fromLevel = bloomRectCount + i0;
					var fromRect = bloomRects[ fromLevel];

					material.SetFloat( 
						kShaderPropertyWeights[ i0], 
						combinedStrength * normalizeFactor);

					Vector4 uvTransform;
					uvTransform.x = (float)fromRect.width / (float)toRect.width;
					uvTransform.y = (float)fromRect.height / (float)toRect.height;
					uvTransform.z = ((float)fromRect.x - ((float)toRect.x * uvTransform.x)) / (float)blurDescriptor.width;
					uvTransform.w = ((float)fromRect.y - ((float)toRect.y * uvTransform.y)) / (float)blurDescriptor.height;
					material.SetVector( kShaderPropertyUvTransforms[ i0], uvTransform);
					combinedStrength *= Properties.IntensityMultiplier;
				}
				
				/* Composition */
				float compositeStrengthSum = 0.0f;
				float compositeStrength = 1.0f;
				
				for( int i0 = 0; i0 < bloomRects.Length; ++i0)
				{
					compositeStrengthSum += compositeStrength;
					compositeStrength *= Properties.IntensityMultiplier;
				}
				compositeStrength = Properties.Intensity / compositeStrengthSum;
				
				for( int i0 = 0; i0 < bloomRectCount; ++i0)
				{
					var rect = bloomRects[ i0];
					material.SetFloat( rect.weightShaderPropertyId, compositeStrength);
					material.SetVector( 
						rect.uvTransformShaderPropertyId, 
						new Vector4(
							(float)rect.width / (float)blurDescriptor.width,
							(float)rect.height / (float)blurDescriptor.height,
							(float)rect.x / (float)blurDescriptor.width,
							(float)rect.y / (float)blurDescriptor.height));
					compositeStrength *= Properties.IntensityMultiplier;
				}
				if( combinePassCount > 0)
				{
					var rect = bloomRects[ bloomRectCount];
					float weight = compositeStrength * combinedStrengthSum;
					material.SetFloat( kShaderPropertyBloomWeightCombined, weight);
					material.SetVector( kShaderPropertyBloomUvTransformCombined, 
						new Vector4(
							(float)rect.width / (float)blurDescriptor.width,
							(float)rect.height / (float)blurDescriptor.height,
							(float)rect.x / (float)blurDescriptor.width,
							(float)rect.y / (float)blurDescriptor.height));
				}
			}
			if( (updateFlags & BloomProperties.kChangeBrightnessExtractionMesh) != 0)
			{
				UpdateBrightnessExtractionMesh();
				rebuild = true;
			}
			if( (updateFlags & BloomProperties.kChangeGaussianBlurMesh) != 0)
			{
				UpdateGaussianBlurHorizontalMesh();
				UpdateGaussianBlurVerticalMesh();
				rebuild = true;
			}
			if( (updateFlags & BloomProperties.kChangeCombineMesh) != 0)
			{
				if( combinePassCount > 0)
				{
					UpdateCombineMesh();
					rebuild = true;
				}
			}
			return rebuild;
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
		void UpdateGaussianBlurHorizontalMesh()
		{
			int vertexCount = bloomRects.Length * 4;
			var vertex = new Vector3[ vertexCount];
			var uv0 = new Vector3[ vertexCount];
			var uv1 = new Vector3[ vertexCount];
			var uv2 = new Vector3[ vertexCount];
			var uv3 = new Vector3[ vertexCount];
			var indices = new int[ vertexCount];
			int vertexIndex = 0;
			
			int scale = brightnessExtractionDescriptor.width;
			
			for( int i0 = 0; i0 < bloomRects.Length; ++i0)
			{
				var rect = bloomRects[ i0];
				
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
		void UpdateGaussianBlurVerticalMesh()
		{
			int vertexCount = bloomRects.Length * 4;
			var vertex = new Vector3[ vertexCount];
			var uv0 = new Vector3[ vertexCount];
			var uv1 = new Vector3[ vertexCount];
			var uv2 = new Vector3[ vertexCount];
			var uv3 = new Vector3[ vertexCount];
			var indices = new int[ vertexCount];
			int vertexIndex = 0;
			
			for( int i0 = 0; i0 < bloomRects.Length; ++i0)
			{
				var rect = bloomRects[ i0];
				
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
			BloomRect toRect = bloomRects[ bloomRects.Length - combinePassCount];
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
	}
}
