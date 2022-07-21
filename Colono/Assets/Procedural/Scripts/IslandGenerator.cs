using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;

public class IslandGenerator : MonoBehaviour
{
    public const int mapChunkSize = 241;
    public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    public TerrainType[] regions;
    
    public Material mapMaterial;

    float[,] falloffMap;

    private void Start()
    {
        falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
    }

    public GameObject GenerateIsland(Vector2 position)
    {
        Island island = new Island(position, mapChunkSize - 1, transform, mapMaterial, this);
        return island.island;
    }

    public MeshData GenerateTerrainMesh(MapData mapData)
    {
        return MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve);
    }

    public MapData GenerateMapData(Vector2 centre)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, centre + offset);

        Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight >= regions[i].height)
                    {
                        colourMap[y * mapChunkSize + x] = regions[i].colour;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        return new MapData(noiseMap, colourMap);
    }

    void OnValidate()
    {
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }

        falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
    }
}

public class Island
{
    const float scale = 1f;
    Vector2 position;

    public GameObject island;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;
    MeshCollider meshTriggerCollider;
    MapData mapData;
    Mesh mesh;

    public Island(Vector2 coord, int size, Transform parent, Material material, IslandGenerator mapGenerator)
    {
        position = coord * size;
        Vector3 positionV3 = new Vector3(position.x, 0, position.y);

        island = new GameObject("Island");
        island.tag = "Island";
        meshRenderer = island.AddComponent<MeshRenderer>();
        meshFilter = island.AddComponent<MeshFilter>();
        meshCollider = island.AddComponent<MeshCollider>();
        meshRenderer.material = material;

        island.transform.position = positionV3 * scale;
        island.transform.parent = parent;
        island.transform.localScale = Vector3.one * scale;

        mapData = mapGenerator.GenerateMapData(position);

        Texture2D texture = TextureGenerator.TextureFromColourMap(mapData.colourMap, IslandGenerator.mapChunkSize, IslandGenerator.mapChunkSize);
        meshRenderer.material.mainTexture = texture;

        MeshData meshData = mapGenerator.GenerateTerrainMesh(mapData);
        mesh = meshData.CreateMesh();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;

        /*GameObject islandTriggerCollider = new GameObject("IslandTriggerCollider");
        meshTriggerCollider = islandTriggerCollider.AddComponent<MeshCollider>();
        meshTriggerCollider.isTrigger = true;
        meshTriggerCollider.sharedMesh = mesh;
        islandTriggerCollider.AddComponent<DistanceWithPlayerScript>();
        islandTriggerCollider.transform.parent = island.transform;
        islandTriggerCollider.transform.localScale = new Vector3(1.1f, 1.1f);*/

        CMR.ConvexDecomposition.Bake(island, CMR.VHACDSession.Create(), null, false, true, false);
        foreach (MeshCollider meshCollider in island.transform.GetChild(0).GetComponentsInChildren<MeshCollider>())
        {
            meshCollider.isTrigger = true;
        }
    }

}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color colour;
}

public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colourMap;

    public MapData(float[,] heightMap, Color[] colourMap)
    {
        this.heightMap = heightMap;
        this.colourMap = colourMap;
    }
}
