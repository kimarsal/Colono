using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CMR
{
    [RequireComponent(typeof(MeshFilter))]
    public class ConvexDecomposition : MonoBehaviour
    {
        static string BAKED_COLLIDERS_NAME = "BakedColliders";

        public static void Bake(GameObject gameObject, VHACDSession session,
            PhysicMaterial physicMaterial, bool createAsset, bool createColliders, bool createRenderers)
        {
            Mesh sourceMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
            if (sourceMesh == null)
            {
                Debug.LogErrorFormat("ConvexDecomposition.Bake() called with GameObject {0} that has no assigned mesh", gameObject);
            }

            string assetPath = null;
            if (createAsset)
            {
                string assetName = gameObject.name + "-colliders.asset";
                assetPath = UnityEditor.EditorUtility.SaveFilePanelInProject("Save collision meshes asset", assetName, "asset", "Please enter a file name to save the mesh asset to");
            }

            // Get or create the BakedColliders wrapper GameObject
            Transform bakedColliders_T = gameObject.transform.Find(BAKED_COLLIDERS_NAME);
            if (!bakedColliders_T)
            {
                GameObject childContainer = new GameObject(BAKED_COLLIDERS_NAME);
                bakedColliders_T = childContainer.transform;
                bakedColliders_T.SetParent(gameObject.transform, false);
            }
            bakedColliders_T.localPosition = Vector3.zero;
            bakedColliders_T.localRotation = Quaternion.identity;
            GameObject bakedColliders = bakedColliders_T.gameObject;

            // Destroy any previous collider GameObjects
            while (bakedColliders.transform.childCount != 0)
            {
                DestroyImmediate(bakedColliders.transform.GetChild(0).gameObject);
            }

            // Convert the mesh geometry for the parent GameObject into an array of convex meshes
            List<Mesh> convexMeshes = VHCDAPI.ConvexDecomposition(sourceMesh.vertices, sourceMesh.triangles, session);
            Debug.LogFormat("Generated {0} convex hull meshes with {1} total triangles", convexMeshes.Count, convexMeshes.Sum(x => x.triangles.Length));

            Shader parentShader = ConvexDecomposition.GetShader(gameObject);

            try
            {
                if (createAsset)
                {
                    AssetDatabase.StartAssetEditing();
                }

                for (int i = 0; i < convexMeshes.Count; i++)
                {
                    Mesh convexMesh = convexMeshes[i];
                    convexMesh.name = string.Format("{0} collider-mesh{1:000}", gameObject.name, i);

                    GameObject go = new GameObject(string.Format("{0} collider{1:000}", gameObject.name, i));

                    if (createColliders)
                    {
                        MeshCollider curMeshCollider = go.AddComponent<MeshCollider>();
                        curMeshCollider.sharedMesh = convexMesh;
                        curMeshCollider.sharedMaterial = physicMaterial;
                        curMeshCollider.convex = true;
                    }

                    if (createRenderers && parentShader)
                    {
                        Color color = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f);

                        MeshFilter curMeshFilter = go.AddComponent<MeshFilter>();
                        curMeshFilter.sharedMesh = convexMesh;

                        MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
                        meshRenderer.sharedMaterial = new Material(parentShader);
                        meshRenderer.sharedMaterial.SetColor("_BaseColor", color);
                        meshRenderer.sharedMaterial.SetColor("_Color", color);
                    }

                    if (createAsset)
                    {
                        if (i == 0)
                        {
                            AssetDatabase.CreateAsset(convexMesh, assetPath);
                        }
                        else
                        {
                            AssetDatabase.AddObjectToAsset(convexMesh, assetPath);
                        }
                    }

                    go.transform.SetParent(bakedColliders_T, false);
                }
            }
            finally
            {
                if (createAsset)
                {
                    AssetDatabase.StopAssetEditing();
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }

        public static Shader GetShader(GameObject gameObject)
        {
            MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
            if (renderer)
            {
                Material material = renderer.sharedMaterial;
                if (material)
                {
                    return material.shader;
                }
            }
            return null;
        }
    }
}
