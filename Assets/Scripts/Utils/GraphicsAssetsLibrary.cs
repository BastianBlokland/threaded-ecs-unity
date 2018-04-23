using System;
using UnityEngine;

namespace Utils
{
	[Serializable] 
	public class GraphicsAssets
	{
		[SerializeField] private byte graphicsID;			
		[SerializeField] private Mesh mesh;
		[SerializeField] private Material material;

		public byte GraphicsID { get { return graphicsID; } }
		public Mesh Mesh { get { return mesh; } }
		public Material Material { get { return material; } }
	}

	[CreateAssetMenu(fileName = "GraphicsAssetLibrary", menuName = "GraphicsAssetLibrary")]
	public class GraphicsAssetsLibrary : ScriptableObject
	{
		[SerializeField] private GraphicsAssets[] entries;

		public GraphicsAssets GetAssets(byte graphicID)
		{
			for (int i = 0; i < entries.Length; i++)
				if(entries[i].GraphicsID == graphicID)
					return entries[i];
			return null;
		}
	}
}