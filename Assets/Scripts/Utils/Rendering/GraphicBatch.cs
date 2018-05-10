using System;
using UnityEngine;

namespace Utils.Rendering
{
	public sealed class GraphicBatch : IDisposable
	{
		private struct Data
		{
			public bool Full => Count >= Matrices.Length;

			public readonly Float3x4[] Matrices;
			public int Count;

			public Data(int capacity)
			{
				Matrices = new Float3x4[capacity];
				Count = 0;
			}

			public void Add(Float3x4 matrix)
			{
				if(Full)
					throw new Exception($"[{nameof(GraphicBatch)}] Unable to add: Allready full");
				Matrices[Count] = matrix;
				Count++;
			}

			public void Clear()
			{
				Count = 0;
			}
		}

		private readonly Mesh mesh;
		private readonly int submeshIndex;
		private readonly Material material;

		//We keep separate data per thread so we can do lock-less insertions
		private readonly Data[] dataPerThread;
		private readonly uint[] renderArgs; 
		
		//GPU buffers
		private readonly ComputeBuffer matrixBuffer;
		private readonly ComputeBuffer renderArgBuffer;

		public GraphicBatch(int executorCount, GraphicAsset graphicAsset)
		{
			mesh = graphicAsset.Mesh;
			submeshIndex = graphicAsset.SubmeshIndex;
			material = graphicAsset.Material == null ? null : new Material(graphicAsset.Material);

			//Use data collections per executor, for lockless insertions.
			//'executorCount' + 1 because we also want to accommodate for the main-thread pushing graphics
			dataPerThread = new Data[executorCount + 1];
			for (int i = 0; i < dataPerThread.Length; i++)
				dataPerThread[i] = new Data(graphicAsset.MaxRenderCount);			
			
			renderArgs = new uint[5] 
			{ 
				mesh == null ? 0 : mesh.GetIndexCount(submeshIndex), //index count per instance
				0, //Instance count
				mesh == null ? 0 : mesh.GetIndexStart(submeshIndex), //start index location
				mesh == null ? 0 : mesh.GetBaseVertex(submeshIndex), //base vertex location
				0 //start instance location.
			};

			//Create GPU buffers
			matrixBuffer = new ComputeBuffer(graphicAsset.MaxRenderCount, Float3x4.SIZE);
			renderArgBuffer = new ComputeBuffer(1, sizeof(uint) * renderArgs.Length, ComputeBufferType.IndirectArguments);

			//Pass reference to the buffers to the material instance
			material?.SetBuffer("matrixBuffer", matrixBuffer);
		}

		public void Add(int execID, Float3x4 matrix)
		{
			//+1 because the main-thread uses execID -1
			dataPerThread[execID + 1].Add(matrix);
		}

		public void UploadData()
		{
			int totalCount = 0;
			for (int i = 0; i < dataPerThread.Length; i++)
			{
				matrixBuffer.SetData
				(
					data: dataPerThread[i].Matrices, 
					managedBufferStartIndex: 0,
					computeBufferStartIndex: (int)totalCount,
					count: dataPerThread[i].Count
				);
				totalCount += dataPerThread[i].Count;
			}
			renderArgs[1] = (uint)totalCount;
			renderArgBuffer.SetData(renderArgs);
		}

		public void Render()
		{
			if(mesh == null || material == null)
				return;

			UploadData();

			Graphics.DrawMeshInstancedIndirect
			(
				mesh: mesh,
				submeshIndex: submeshIndex, 
				material: material, 
				bounds: new Bounds(Vector3.zero, Vector3.one * float.MaxValue), //Disabled the frustum culling (as we have no data on how big the content is atm)
				bufferWithArgs: renderArgBuffer
			);
		}

		public void Clear()
		{
			for (int i = 0; i < dataPerThread.Length; i++)
				dataPerThread[i].Clear();
		}

		public void Dispose()
		{
			//Destroy instantiated material
			UnityEngine.Object.Destroy(material);

			//Release gpu buffers
			matrixBuffer.Dispose();
			renderArgBuffer.Dispose();
		}
	}
}