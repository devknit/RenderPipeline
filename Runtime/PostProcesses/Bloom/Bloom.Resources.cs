﻿
using UnityEngine;

namespace RenderPipeline
{
	public sealed partial class Bloom
	{
		void UpdateResources( int updateFlags)
		{
			if( (updateFlags & BloomProperties.kChangeThresholds) != 0)
			{
				Material material = brightnessExtractionMaterial;

				if( SystemInfo.SupportsRenderTextureFormat( RenderTextureFormat.RGB111110Float) != false)
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
				CalculateGaussianSamples( Properties.SigmaInPixel);
				float diff = blurSample0.offset - blurSample1.offset;
				float scale = (diff != 0.0f) ? (blurSample0.offset * 2.0f) / Mathf.Abs( diff) : 0.0f;
				gaussianBlurMaterial.SetFloat( kShaderPropertyInvertOffsetScale01, scale);
			}
			if( (updateFlags & BloomProperties.kChangeCombinePassCount) != 0)
			{
				combinePassCount = (bloomRects.Length + Properties.DownSampleLevel) - Properties.CombineStartLevel;
				combinePassCount = Mathf.Clamp( combinePassCount, 0, bloomRects.Length);
				bloomRectCount = bloomRects.Length - combinePassCount;
				bloomRectCount = Mathf.Clamp( bloomRectCount, 0, bloomRects.Length - 1);
				
				Material material;
				
				/* Combine */
				material = combineMaterial;
				
				if( (combinePassCount & 0x4) != 0)
				{
					if( material.IsKeywordEnabled( kShaderKeywordSample4) == false)
					{
						material.EnableKeyword( kShaderKeywordSample4);
					}
				}
				else if( material.IsKeywordEnabled( kShaderKeywordSample4) != false)
				{
					material.DisableKeyword( kShaderKeywordSample4);
				}
				if( (combinePassCount & 0x2) != 0)
				{
					if( material.IsKeywordEnabled( kShaderKeywordSample2) == false)
					{
						material.EnableKeyword( kShaderKeywordSample2);
					}
				}
				else if( material.IsKeywordEnabled( kShaderKeywordSample2) != false)
				{
					material.DisableKeyword( kShaderKeywordSample2);
				}
				if( (combinePassCount & 0x1) != 0)
				{
					if( material.IsKeywordEnabled( kShaderKeywordSample1) == false)
					{
						material.EnableKeyword( kShaderKeywordSample1);
					}
				}
				else if( material.IsKeywordEnabled( kShaderKeywordSample1) != false)
				{
					material.DisableKeyword( kShaderKeywordSample1);
				}
				
				/* Composition */
				material = compositionMaterial;
				
				if( combinePassCount > 0)
				{
					if( material.IsKeywordEnabled( kShaderKeywordCombined) == false)
					{
						material.EnableKeyword( kShaderKeywordCombined);
					}
				}
				else if( material.IsKeywordEnabled( kShaderKeywordCombined) != false)
				{
					material.DisableKeyword( kShaderKeywordCombined);
				}
				if( (bloomRectCount & 0x4) != 0)
				{
					if( material.IsKeywordEnabled( kShaderKeywordSample4) == false)
					{
						material.EnableKeyword( kShaderKeywordSample4);
					}
				}
				else if( material.IsKeywordEnabled( kShaderKeywordSample4) != false)
				{
					material.DisableKeyword( kShaderKeywordSample4);
				}
				if( (bloomRectCount & 0x2) != 0)
				{
					if( material.IsKeywordEnabled( kShaderKeywordSample2) == false)
					{
						material.EnableKeyword( kShaderKeywordSample2);
					}
				}
				else if( material.IsKeywordEnabled( kShaderKeywordSample2) != false)
				{
					material.DisableKeyword( kShaderKeywordSample2);
				}
				if( (bloomRectCount & 0x1) != 0)
				{
					if( material.IsKeywordEnabled( kShaderKeywordSample1) == false)
					{
						material.EnableKeyword( kShaderKeywordSample1);
					}
				}
				else if( material.IsKeywordEnabled( kShaderKeywordSample1) != false)
				{
					material.DisableKeyword( kShaderKeywordSample1);
				}
				
				/* Mesh */
				UpdateBrightnessExtractionMesh();
				UpdateGaussianBlurHorizontalMesh();
				UpdateGaussianBlurVerticalMesh();
				
				if( combinePassCount > 0)
				{
					UpdateCombineMesh();
				}
			}
			if( (updateFlags & BloomProperties.kChangeCombineComposition) != 0)
			{
				/* Combine */
				Material material = combineMaterial;
				
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
				material = compositionMaterial;
				
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
		}
		void UpdateBrightnessExtractionMesh()
		{
			float x0 = (float)brightnessOffsetX / (float)brightnessExtractionDescriptor.width;
			float x1 = (float)(brightnessOffsetX + brightnessNetWidth) / (float)brightnessExtractionDescriptor.width;
			float y0 = (float)brightnessOffsetY / (float)brightnessExtractionDescriptor.height;
			float y1 = (float)(brightnessOffsetY + brightnessNetHeight) / (float)brightnessExtractionDescriptor.height;
			
			brightnessExtractionMesh.Clear();
			brightnessExtractionMesh.SetVertices(
				new Vector3[]{
					new Vector3( x0, y0, 0),
					new Vector3( x0, y1, 0),
					new Vector3( x1, y1, 0),
					new Vector3( x1, y0, 0)
				});
			brightnessExtractionMesh.SetUVs( 
				0,
				new Vector2[]{
					new Vector2( 0, 0),
					new Vector2( 0, 1),
					new Vector2( 1, 1),
					new Vector2( 1, 0)
				});
			brightnessExtractionMesh.SetIndices(
				new int[]{ 0, 1, 2, 3 }, MeshTopology.Quads, 0);
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
