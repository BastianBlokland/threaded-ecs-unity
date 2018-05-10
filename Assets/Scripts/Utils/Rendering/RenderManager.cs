using System;

namespace Utils.Rendering
{
	public sealed class RenderManager : IDisposable
	{
		private readonly GraphicBatch[] graphicBatches;

		public RenderManager(int executorCount, GraphicAssetLibrary assetLibrary)
		{
			graphicBatches = new GraphicBatch[assetLibrary.AssetCount];
			for (byte graphicID = 0; graphicID < graphicBatches.Length; graphicID++)
				graphicBatches[graphicID] = new GraphicBatch(executorCount, assetLibrary.GetAsset(graphicID));
		}

		public void Add(int execID, byte graphicID, Float3x4 matrix, float age)
		{
			if(graphicID >= graphicBatches.Length)
				throw new Exception($"[{nameof(GraphicBatch)}] {nameof(graphicID)} is out of bounds! Is the graphic setup in the {nameof(GraphicAssetLibrary)}?");

			graphicBatches[graphicID].Add(execID, matrix, age);
		}

		public void Render()
		{
			for (byte graphicID = 0; graphicID < graphicBatches.Length; graphicID++)
				graphicBatches[graphicID].Render();
		}

		public void Clear()
		{
			for (byte graphicID = 0; graphicID < graphicBatches.Length; graphicID++)
				graphicBatches[graphicID].Clear();
		}

		public void Dispose()
		{
			for (byte graphicID = 0; graphicID < graphicBatches.Length; graphicID++)
				graphicBatches[graphicID].Dispose();
		}		
	}
}