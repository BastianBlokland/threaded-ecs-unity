using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

using static Utils.MathUtils;

namespace Editor
{
	public sealed class AnimationTextureBaker : EditorWindow
	{
		private Mesh mesh;
		private int boneIndexUVChannel = 1;
		private int maxBoneCount = 10;
		private int textureWidth = 64;
		private int textureHeight = 64;
		private string textureOutputPath = "/Data/Textures/ExplosionAnimation.exr";

		[MenuItem("Tools/AnimationTextureBaker")]
		public static void OpenWindow() => EditorWindow.GetWindow<AnimationTextureBaker>().Show();

		private void OnGUI()
		{
			mesh = EditorGUILayout.ObjectField("Mesh to bake", mesh, typeof(Mesh), allowSceneObjects: true) as Mesh;
			boneIndexUVChannel = EditorGUILayout.IntField($"{nameof(boneIndexUVChannel)}", boneIndexUVChannel);
			maxBoneCount = EditorGUILayout.IntField($"{nameof(maxBoneCount)}", maxBoneCount);
			textureWidth = EditorGUILayout.IntField($"{nameof(textureWidth)}", textureWidth);
			textureHeight = EditorGUILayout.IntField($"{nameof(textureHeight)}", textureHeight);
			textureOutputPath = EditorGUILayout.TextField($"{nameof(textureOutputPath)}", textureOutputPath);

			if(GUILayout.Button($"{nameof(BakeBoneIndexToUV)}") && mesh != null)
				BakeBoneIndexToUV(mesh, boneIndexUVChannel, maxBoneCount);

			if(GUILayout.Button($"{nameof(CreateAnimationTexture)}"))
				CreateAnimationTexture(maxBoneCount, textureWidth, textureHeight, textureOutputPath);
		}

		private static void CreateAnimationTexture(int maxBones, int textureWidth, int textureHeight, string textureOutputPath)
		{
			Color[] colors = new Color[10]
			{
				Color.gray,
				Color.blue,
				Color.yellow,
				Color.clear,
				Color.magenta,
				Color.red,
				Color.green,
				Color.yellow,
				Color.cyan,
				Color.magenta
			};
			
			Texture2D texture = new Texture2D(textureWidth, textureHeight, format: TextureFormat.RGBAFloat, mipmap: false, linear: true);
			for (int boneIndex = 0; boneIndex < maxBones; boneIndex++)
			{
				for (int x = 0; x < textureWidth; x++)
				{
					float prog = (float)x / textureWidth;
					texture.SetPixel(x, boneIndex, Color.Lerp(colors[boneIndex], new Color(2f, 2f, 2f, 1f), prog));
				}
			}
			texture.Apply(updateMipmaps: false, makeNoLongerReadable: false);

			byte[] exrData = ImageConversion.EncodeToEXR(texture, Texture2D.EXRFlags.OutputAsFloat);
			File.WriteAllBytes($"{Application.dataPath}/{textureOutputPath}", exrData);
			AssetDatabase.Refresh();
		}

		private static void BakeBoneIndexToUV(Mesh mesh, int uvIndex, int maxBones)
		{
			Debug.Log($"Baking bone-index to uv-{uvIndex} for mesh: {mesh.name}");

			List<Vector2> uv2 = new List<Vector2>(mesh.vertexCount);
			for (int i = 0; i < mesh.boneWeights.Length; i++)
			{
				int boneIndex = FindIndex(mesh.boneWeights[i]);
				uv2.Add(new Vector2((float)boneIndex / maxBones, 1f));
			}
			mesh.SetUVs(uvIndex, uv2);

			EditorUtility.SetDirty(mesh);
			AssetDatabase.SaveAssets();
		}

		private static int FindIndex(BoneWeight weight)
		{
			if(Bigger(weight.weight1, weight.weight0, weight.weight2, weight.weight3))
				return weight.boneIndex1;
			if(Bigger(weight.weight2, weight.weight0, weight.weight1, weight.weight3))
				return weight.boneIndex2;
			if(Bigger(weight.weight3, weight.weight0, weight.weight1, weight.weight2))
				return weight.boneIndex3;
			return weight.boneIndex0;
		}
	}
}