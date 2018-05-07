using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Utils
{
	public sealed class RenderSet : IDisposable
	{
		private sealed class BatchData : IDisposable
		{
			private readonly Mesh mesh;
			private readonly Material material;

			//Managed collections
			private readonly Float3x4[] matrices;

			//Content of render-args
			//0. index count per instance, 
			//1. instance count
			//2. start index location
			//3. base vertex location
			//4. start instance location.
			private readonly uint[] renderArgs; 
			
			//Gpu buffers
			private readonly ComputeBuffer matrixBuffer;
			private readonly ComputeBuffer renderArgBuffer;

			public BatchData(GraphicsAsset graphicAsset)
			{
				mesh = graphicAsset.Mesh;
				material = graphicAsset.Material == null ? null : new Material(graphicAsset.Material);

				matrices = new Float3x4[graphicAsset.MaxRenderCount];
				renderArgs = new uint[5] 
				{ 
					mesh == null ? 0 : mesh.GetIndexCount(0), 
					0, //Instance count
					mesh == null ? 0 : mesh.GetIndexStart(submesh: 0),
					mesh == null ? 0 : mesh.GetBaseVertex(submesh: 0),
					0
				};

				//Create gpu buffers
				matrixBuffer = new ComputeBuffer(graphicAsset.MaxRenderCount, sizeof(float) * 4 * 4);
				renderArgBuffer = new ComputeBuffer(1, sizeof(uint) * renderArgs.Length, ComputeBufferType.IndirectArguments);

				//Pass reference to the buffers to the material instance
				material?.SetBuffer("matrixBuffer", matrixBuffer);
			}

			public void Add(Float3x4 matrix)
			{
				uint count = renderArgs[1];
				if(count >= matrices.Length)
					throw new Exception($"[{nameof(BatchData)}] Unable to add: Max count of '{count}' allready reached");

				matrices[(int)count] = matrix;
				renderArgs[1] = count + 1;
			}
			public void Render()
			{
				if(mesh == null || material == null)
					return;

				matrixBuffer.SetData(matrices, managedBufferStartIndex: 0, computeBufferStartIndex: 0, count: (int)renderArgs[1]);
				renderArgBuffer.SetData(renderArgs, managedBufferStartIndex: 0, computeBufferStartIndex: 0, count: renderArgs.Length);

				Graphics.DrawMeshInstancedIndirect
				(
					mesh: mesh,
					submeshIndex: 0, 
					material: material, 
					bounds: new Bounds(Vector3.zero, Vector3.zero), //Not sure what these bounds are used for, not for culling at least
					bufferWithArgs: renderArgBuffer
				);
			}

			public void Clear()
			{
				renderArgs[1] = 0;
			}

			public void Dispose()
			{
				//Destroy instantiated material
				UnityEngine.Object.Destroy(material);

				//Release gpu buffers
				matrixBuffer.Release();
				renderArgBuffer.Release();
			}
		}

		//Every executor has its own list of batches so there is no need for locking
		private readonly BatchData[,] batchesPerThread;

		public RenderSet(int executorCount, GraphicsAssetsLibrary assetsLibrary)
		{
			int threadCount = executorCount + 1; //1 extra for the main-thread
			batchesPerThread = new BatchData[threadCount, assetsLibrary.AssetCount];

			for (int threadID = 0; threadID < batchesPerThread.GetLength(0); threadID++)
			for (byte graphicID = 0; graphicID < batchesPerThread.GetLength(1); graphicID++)
			{
				GraphicsAsset graphic = assetsLibrary.GetAsset(graphicID);
				batchesPerThread[threadID, graphicID] = new BatchData(graphic);
			}
		}

		public void Add(int execID, byte graphicsID, Float3x4 matrix)
		{
			if(graphicsID >= batchesPerThread.GetLength(1))
				throw new Exception($"[{nameof(BatchData)}] {nameof(graphicsID)} is out of bounds! Is the graphic setup in the {nameof(GraphicsAssetsLibrary)}");

			int threadID = execID + 1; //+1 because main-thread uses execID of -1 and the executors use 0 and up
			batchesPerThread[threadID, graphicsID].Add(matrix);
		}

		public void Render()
		{
			for (int threadID = 0; threadID < batchesPerThread.GetLength(0); threadID++)
			for (byte graphicID = 0; graphicID < batchesPerThread.GetLength(1); graphicID++)
			{
				batchesPerThread[threadID, graphicID].Render();
			}
		}

		public void Clear()
		{
			for (int threadID = 0; threadID < batchesPerThread.GetLength(0); threadID++)
			for (byte graphicID = 0; graphicID < batchesPerThread.GetLength(1); graphicID++)
			{
				batchesPerThread[threadID, graphicID].Clear();
			}
		}

		public void Dispose()
		{
			for (int threadID = 0; threadID < batchesPerThread.GetLength(0); threadID++)
			for (byte graphicID = 0; graphicID < batchesPerThread.GetLength(1); graphicID++)
			{
				batchesPerThread[threadID, graphicID].Dispose();
			}
		}		
	}
}