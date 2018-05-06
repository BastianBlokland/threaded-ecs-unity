using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Utils
{
	public sealed class RenderSet
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

		//Every executor has its own list of batches so there is no need for locking
		private readonly List<BatchData>[] batchesPerThread;

		public RenderSet(int executorCount, GraphicsAssetsLibrary assetsLibrary)
		{
			this.assetsLibrary = assetsLibrary;
			
			batchesPerThread = new List<BatchData>[executorCount + 1]; //1 extra for the main-thread
			for (int i = 0; i < batchesPerThread.Length; i++)
				batchesPerThread[i] = new List<BatchData>();
		}

		public void Add(int execID, byte graphicsID, Matrix4x4 matrix)
		{
			List<BatchData> list = batchesPerThread[execID + 1]; //+1 because main-thread uses execID of -1 and the executors use 0 and up
			
			bool added = false;
			for (int i = 0; i < list.Count; i++)
			{
				BatchData batch = list[i];
				if(batch.GraphicsID == graphicsID && batch.Count < MAX_BATCH_SIZE)
				{
					batch.Matrices[batch.Count] = matrix;
					batch.Count++;
					list[i] = batch;
					added = true;
					break;
				}
			}
			if(!added)
			{
				BatchData newBatch = new BatchData(graphicsID, new Matrix4x4[MAX_BATCH_SIZE]);
				newBatch.Matrices[0] = matrix;
				newBatch.Count = 1;
				list.Add(newBatch);
			}
		}

		public void Clear()
		{
			for (int i = 0; i < batchesPerThread.Length; i++)
			for (int j = 0; j < batchesPerThread[i].Count; j++)
			{
				BatchData batch = batchesPerThread[i][j];
				batch.Count = 0;
				batchesPerThread[i][j] = batch;
			}
		}

		public void Render()
		{
			for (int i = 0; i < batchesPerThread.Length; i++)
			for (int j = 0; j < batchesPerThread[i].Count; j++)
			{
				BatchData batch = batchesPerThread[i][j];
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
					count: batch.Count
				);
			}
		}
	}
}