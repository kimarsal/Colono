using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine.AI;

public class IslandGenerator : MonoBehaviour
{
    public static int mapChunkSize = 241;
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
    public TerrainType zoneType;
    
    public Material mapMaterial;

    float[,] falloffMap;

    private void Start()
    {
        falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
        zoneType.name = "Zone";
        zoneType.colour = Color.white;
        zoneType.height = 0;
    }

    public Island GenerateIsland(Vector2 position, IslandEditor islandEditor)
    {
        return new Island(position, transform, mapMaterial, this, islandEditor);
    }

    public MeshData GenerateTerrainMesh(MapData mapData)
    {
        return MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve);
    }

    public MapData GenerateMapData(Vector2 centre)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, centre + offset);
        float[,] detailMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, 0.1f, 1, persistance, lacunarity, centre + offset);
        Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
        int[,] regionMap = new int[mapChunkSize, mapChunkSize];

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
                        regionMap[x, y] = i;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        return new MapData(noiseMap, colourMap, detailMap, regionMap);
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

    public GameObject island;
    public MeshData meshData;
    public int[,] regionMap;

    public Island(Vector2 coord, Transform parent, Material material, IslandGenerator islandGenerator, IslandEditor islandEditor)
    {
        Vector2 position = coord;
        Vector3 positionV3 = new Vector3(position.x, 0, position.y);

        island = new GameObject("Island");
        island.tag = "Island";
        //island.layer = 6;
        island.isStatic = true;

        MeshRenderer meshRenderer = island.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = island.AddComponent<MeshFilter>();
        MeshCollider meshCollider = island.AddComponent<MeshCollider>();
        meshRenderer.material = material;

        island.transform.position = positionV3 * scale;
        island.transform.parent = parent;
        island.transform.localScale = Vector3.one * scale;

        MapData mapData = islandGenerator.GenerateMapData(position);
        regionMap = mapData.regionMap;

        Texture2D colorTexture = TextureGenerator.TextureFromColourMap(mapData.colourMap, IslandGenerator.mapChunkSize, IslandGenerator.mapChunkSize);
        Texture2D detailTexture = TextureGenerator.TextureFromHeightMap(mapData.detailMap);
        meshRenderer.material.mainTexture = colorTexture;

        meshData = islandGenerator.GenerateTerrainMesh(mapData);
        Mesh mesh = meshData.CreateMesh();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;


        CMR.ConvexDecomposition.Bake(island, CMR.VHACDSession.Create(), null, false, true, false);
        foreach (MeshCollider triggerCollider in island.transform.GetChild(0).GetComponentsInChildren<MeshCollider>())
        {
            triggerCollider.isTrigger = true;
        }

        GameObject coastObstacle = GameObject.Instantiate(islandEditor.coastObstacle, island.transform);
        NavMeshSurface surface = island.AddComponent<NavMeshSurface>();

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
    public readonly float[,] detailMap;
    public readonly int[,] regionMap;

    public MapData(float[,] heightMap, Color[] colourMap, float[,] detailMap, int[,] regionMap)
    {
        this.heightMap = heightMap;
        this.colourMap = colourMap;
        this.detailMap = detailMap;
        this.regionMap = regionMap;
    }
}
