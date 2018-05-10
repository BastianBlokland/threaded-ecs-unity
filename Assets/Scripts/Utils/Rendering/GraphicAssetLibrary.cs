using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils.Rendering
{
	[Serializable]
	public class GraphicAsset
	{
		public Mesh Mesh;
		public int SubmeshIndex;
		public Material Material;
		public int MaxRenderCount;
	}

	[CreateAssetMenu(fileName = "GraphicAssetLibrary", menuName = "GraphicAssetLibrary")]
	public class GraphicAssetLibrary : ScriptableObject, IEnumerable<GraphicAsset>
	{
		public int AssetCount => entries.Count;

		[SerializeField] private List<GraphicAsset> entries;

		public GraphicAsset GetAsset(byte graphicID)
		{
			if(graphicID >= entries.Count)
				throw new Exception($"[{nameof(GraphicAssetLibrary)}] Graphic id '{graphicID}' is higher then the amount of configured entries");
			return entries[graphicID];
		}

		public IEnumerator<GraphicAsset> GetEnumerator() => entries.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)entries).GetEnumerator();
	}
}