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

		//Even tho this seems like the perfect usage for ThreadLocal<List<BatchData>> but i was see-ing considerable
		//slowdowns of ThreadLocal vs just keeping a dictionary indexed by threadID. Very strange if anyone knows why, let me know.
		private readonly ConcurrentDictionary<int, List<BatchData>> batchesLookupPerThread;

		public RenderSet(GraphicsAssetsLibrary assetsLibrary)
		{
			this.assetsLibrary = assetsLibrary;
			this.batchesLookupPerThread = new ConcurrentDictionary<int, List<BatchData>>();
		}

		public void Add(byte graphicsID, Matrix4x4 matrix)
		{
			int threadId = Thread.CurrentThread.ManagedThreadId;
			List<BatchData> list = batchesLookupPerThread.GetOrAdd(threadId, (_) => new List<BatchData>());
			
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
			foreach(KeyValuePair<int, List<BatchData>> kvp in batchesLookupPerThread)
			{
				for (int i = 0; i < kvp.Value.Count; i++)
				{
					BatchData batch = kvp.Value[i];
					batch.Count = 0;
					kvp.Value[i] = batch;
				}
			}
		}

		public void Render()
		{
			foreach(KeyValuePair<int, List<BatchData>> kvp in batchesLookupPerThread)
			{
				for (int i = 0; i < kvp.Value.Count; i++)
				{
					BatchData batch = kvp.Value[i];
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
}