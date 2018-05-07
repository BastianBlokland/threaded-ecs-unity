using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
	[Serializable] 
	public class GraphicsAsset
	{
		public Mesh Mesh;
		public Material Material;
		public int MaxRenderCount;
	}

	[CreateAssetMenu(fileName = "GraphicsAssetLibrary", menuName = "GraphicsAssetLibrary")]
	public class GraphicsAssetsLibrary : ScriptableObject, IEnumerable<GraphicsAsset>
	{
		public int AssetCount => entries.Count;

		[SerializeField] private List<GraphicsAsset> entries;

		public GraphicsAsset GetAsset(byte graphicID)
		{
			if(graphicID >= entries.Count)
				throw new Exception($"[{nameof(GraphicsAssetsLibrary)}] Graphic id '{graphicID}' is higher then the amount of configured entries");
			return entries[graphicID];
		}

		public IEnumerator<GraphicsAsset> GetEnumerator() => entries.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)entries).GetEnumerator();
	}
}