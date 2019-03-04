using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

using static Utils.MathUtils;

namespace Editor
{
    public sealed class AnimationTextureBaker : EditorWindow
    {
        private GameObject skinnedMeshRoot;
        private BoneColorLookup boneColorLookup;
        private int boneIndexUVChannel = 1;
        private int textureSize = 64;
        private string meshOutputPath = "Data/Meshes/ExplosionMesh.asset";
        private string textureOutputPath = "Data/Textures/ExplosionAnimation.exr";

        [MenuItem("Tools/AnimationTextureBaker")]
        public static void OpenWindow() => EditorWindow.GetWindow<AnimationTextureBaker>().Show();

        private void OnGUI()
        {
            skinnedMeshRoot = EditorGUILayout.ObjectField($"Root game-object of the skinned-mesh root", skinnedMeshRoot, typeof(GameObject), allowSceneObjects: true) as GameObject;
            boneColorLookup = EditorGUILayout.ObjectField($"{nameof(BoneColorLookup)}", boneColorLookup, typeof(BoneColorLookup), allowSceneObjects: true) as BoneColorLookup;
            boneIndexUVChannel = EditorGUILayout.IntField($"{nameof(boneIndexUVChannel)}", boneIndexUVChannel);
            textureSize = EditorGUILayout.IntField($"{nameof(textureSize)}", textureSize);
            meshOutputPath = EditorGUILayout.TextField($"{nameof(meshOutputPath)}", meshOutputPath);
            textureOutputPath = EditorGUILayout.TextField($"{nameof(textureOutputPath)}", textureOutputPath);

            if (GUILayout.Button("Export mesh + texture") && skinnedMeshRoot != null)
            {
                Animation anim = skinnedMeshRoot.GetComponent<Animation>();
                SkinnedMeshRenderer smr = skinnedMeshRoot.GetComponentInChildren<SkinnedMeshRenderer>(includeInactive: true);
                if (anim != null && anim.clip != null && smr != null && smr.sharedMesh != null)
                {
                    float uniformScale = GetUniformLocalScale(smr.transform); //Get base scale for the mesh (can be non-zero if a fbx export scale is used)

                    CreateAnimationTexture(smr, boneColorLookup, anim.clip, textureSize, uniformScale, textureOutputPath);
                    ExportMeshWithBoneInUV(smr.sharedMesh, boneIndexUVChannel, uniformScale, $"Assets/{meshOutputPath}");
                }
            }
        }

        private static void CreateAnimationTexture(SkinnedMeshRenderer smr, BoneColorLookup bcl, AnimationClip ac, int texSize, float refScale, string outputPath)
        {
            Debug.Log($"Creating animation texture for mesh-renderer: {smr.name}");

            Texture2D texture = new Texture2D(texSize, texSize, TextureFormat.RGBAFloat, mipChain: false, linear: true);
            for (int boneIndex = 0; boneIndex < smr.bones.Length; boneIndex++)
            {
                int startY = boneIndex * 2; //because 2 elements per bone: color and (position, scale)
                for (int x = 0; x < texSize; x++)
                {
                    float prog = x == 0 ? 0 : ((float)x / (texSize - 1));

                    //Get color
                    Color color = bcl == null ? Color.white : bcl.GetColor(smr.bones[boneIndex].name, prog);

                    //Get position
                    ac.SampleAnimation(smr.transform.root.gameObject, prog * ac.length);
                    Vector3 position = smr.bones[boneIndex].localPosition;

                    //Get scale
                    float boneScale = GetUniformLocalScale(smr.bones[boneIndex]) / refScale;
                    float bindposeScale = GetUniformScale(smr.sharedMesh.bindposes[boneIndex].lossyScale);
                    float scale = boneScale * bindposeScale;

                    //Save the data in the texture
                    texture.SetPixel(x, startY, color);
                    texture.SetPixel(x, startY + 1, new Color(position.x, position.y, position.z, scale));
                }
            }
            texture.Apply(updateMipmaps: false, makeNoLongerReadable: false);

            byte[] exrData = ImageConversion.EncodeToEXR(texture, Texture2D.EXRFlags.OutputAsFloat);
            File.WriteAllBytes($"{Application.dataPath}/{outputPath}", exrData);
            AssetDatabase.Refresh();
        }

        private static void ExportMeshWithBoneInUV(Mesh mesh, int uvIndex, float scale, string outputPath)
        {
            Debug.Log($"Exporting mesh {mesh.name} with bone-index in uv-{uvIndex} with scale: {scale}");

            Mesh instantiatedMesh = Object.Instantiate(mesh) as Mesh;

            //Set bone-index into uv2
            var uv2 = new List<Vector2>(mesh.vertexCount);
            for (int i = 0; i < mesh.boneWeights.Length; i++)
                uv2.Add(new Vector2(FindIndex(mesh.boneWeights[i]), 1f));
            instantiatedMesh.SetUVs(uvIndex, uv2);

            //Scale vertices
            var vertices = new List<Vector3>();
            mesh.GetVertices(vertices);
            for (int i = 0; i < vertices.Count; i++)
                vertices[i] *= scale;
            instantiatedMesh.SetVertices(vertices);

            //Recalculate after scaling
            instantiatedMesh.RecalculateBounds();
            instantiatedMesh.RecalculateNormals();

            AssetDatabase.CreateAsset(instantiatedMesh, outputPath);
            AssetDatabase.Refresh();
        }

        private static float GetUniformLocalScale(Transform transform) => GetUniformScale(transform.localScale);

        private static float GetUniformScale(Vector3 scale) => (scale.x + scale.y + scale.z) / 3;

        private static int FindIndex(BoneWeight weight)
        {
            if (Bigger(weight.weight1, weight.weight0, weight.weight2, weight.weight3))
                return weight.boneIndex1;
            if (Bigger(weight.weight2, weight.weight0, weight.weight1, weight.weight3))
                return weight.boneIndex2;
            if (Bigger(weight.weight3, weight.weight0, weight.weight1, weight.weight2))
                return weight.boneIndex3;
            return weight.boneIndex0;
        }
    }
}
