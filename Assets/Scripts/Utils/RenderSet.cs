using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Utils
{
	public class RenderSet
	{
		private struct BatchData
		{
			public readonly byte GraphicsID;
			public readonly Matrix4x4[] Matrices;
			public int Count;

			public BatchData(byte graphicsID, Matrix4x4[] matrices)
			{
				GraphicsID = graphicsID;
				Matrices = matrices;
				Count = 0;
			}
		}

		//1023, is the max render count for a single call (https://docs.unity3d.com/ScriptReference/Graphics.DrawMeshInstanced.html)
		public const int MAX_BATCH_SIZE = 1023;

		private readonly GraphicsAssetsLibrary assetsLibrary;
		private readonly MaterialPropertyBlock propertyBlock;
		private readonly List<BatchData> batches;

		public RenderSet(GraphicsAssetsLibrary assetsLibrary)
		{
			this.assetsLibrary = assetsLibrary;
			this.propertyBlock = new MaterialPropertyBlock();
			this.batches = new List<BatchData>();
		}

		public void Add(byte graphicsID, Matrix4x4 matrix)
		{
			lock(batches)
			{
				bool added = false;
				for (int i = 0; i < batches.Count; i++)
				{
					BatchData batch = batches[i];
					if(batch.GraphicsID == graphicsID && batch.Count < MAX_BATCH_SIZE)
					{
						batch.Matrices[batch.Count] = matrix;
						batch.Count++;
						batches[i] = batch;
						added = true;
						break;
					}
				}

				if(!added)
				{
					BatchData newBatch = new BatchData(graphicsID, new Matrix4x4[MAX_BATCH_SIZE]);
					newBatch.Matrices[0] = matrix;
					newBatch.Count = 1;
					batches.Add(newBatch);
				}
			}
		}

		public void Clear()
		{
			lock(batches)
			{
				for (int i = 0; i < batches.Count; i++)
				{
					BatchData batch = batches[i];
					batch.Count = 0;
					batches[i] = batch;
				}
			}
		}

		public void Render()
		{
			lock(batches)
			{
				for (int i = 0; i < batches.Count; i++)
				{
					BatchData batch = batches[i];
					if(batch.Count == 0)
						continue;
					
					GraphicsAssets assets = assetsLibrary.GetAssets(batch.GraphicsID);
					if(assets == null)
						continue;
					
					Graphics.DrawMeshInstanced
					(
						mesh: assets.Mesh,
						submeshIndex: 0, 
						material: assets.Material,
						matrices: batch.Matrices,
						count: batch.Count,
						properties: propertyBlock
					);
				}
			}
		}
	}
}