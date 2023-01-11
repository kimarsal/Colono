using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine.AI;
using System.Runtime.CompilerServices;
using Random = UnityEngine.Random;
using Unity.VisualScripting;

public class IslandGenerator : MonoBehaviour
{
    public static int mapChunkSize = 241;

    public int seed;
    public Vector2 offset;
    public Material islandMaterial;
    public AnimationCurve meshHeightCurve;
    public TerrainType[] regions;

    private float noiseScale = 100;
    private int octaves = 10;
    private float persistance = 0.5f;
    private float lacunarity = 2;
    private float meshHeightMultiplier = 50f;
    private float[,] falloffMap;

    private void Start()
    {
        //falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
    }

    public IslandScript GenerateIsland(Vector2 position, IslandEditor islandEditor, IslandInfo islandInfo = null)
    {
        // Part 1: Es genera el GameObject i els seus components
        GameObject island = new GameObject("Island");
        island.transform.position = new Vector3(position.x, 0, position.y);
        island.transform.parent = transform;
        island.transform.localScale = Vector3.one;
        island.tag = "Island";
        island.isStatic = true;

        MeshRenderer meshRenderer = island.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = island.AddComponent<MeshFilter>();
        MeshCollider meshCollider = island.AddComponent<MeshCollider>();
        meshRenderer.material = islandMaterial;

        // Part 2: Es pinta l'objecte amb el colourMap
        MapData mapData = GenerateMapData(position);
        Texture2D colorTexture = TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize);
        meshRenderer.material.mainTexture = colorTexture;

        // Part 3: Es genera el mesh a partir del heightMap
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve);
        Mesh mesh = meshData.CreateMesh();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;

        // Part 4: S'afegeixen els col·liders que serviran per calcular la distància entre el vaixell i la costa
        CMR.ConvexDecomposition.Bake(island, CMR.VHACDSession.Create(), null, false, true, false);
        foreach (MeshCollider triggerCollider in island.transform.GetChild(0).GetComponentsInChildren<MeshCollider>())
        {
            triggerCollider.isTrigger = true;
        }

        // Part 5: Es calcula la navegació
        GameObject coastObstacle = Instantiate(islandEditor.coastObstacle, island.transform);
        NavMeshSurface surface = island.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.Children;
        surface.BuildNavMesh();

        // Part 6: S'afegeixen els scripts
        IslandScript islandScript = island.AddComponent<IslandScript>();
        islandScript.regionMap = mapData.regionMap;
        islandScript.meshData = meshData;

        islandScript.npcManager = island.AddComponent<NPCManager>();
        islandScript.npcManager.islandEditor = islandEditor;
        islandScript.npcManager.islandScript = islandScript;

        islandScript.inventoryScript = transform.AddComponent<InventoryScript>();

        // Part 7: S'afegeixen els fills
        islandScript.items = new GameObject("Items");
        islandScript.items.transform.parent = island.transform;
        islandScript.items.transform.localPosition = Vector3.zero;

        GameObject npcs = new GameObject("NPCs");
        npcs.transform.parent = island.transform;
        npcs.transform.localPosition = Vector3.zero;
        islandScript.npcManager.npcs = npcs;
        
        islandScript.constructions = new GameObject("Constructions");
        islandScript.constructions.transform.parent = gameObject.transform;
        islandScript.constructions.transform.localPosition = Vector3.zero;

        // Part 8: S'afegeixen els elements de joc
        int row = 0, col = 0;
        while (row < mapChunkSize - 1)
        {
            Vector2 itemCell = new Vector2(col, row);
            Vector3 itemPos = Vector3.zero;

            try
            {
                itemPos = island.transform.position + MeshGenerator.GetCellCenter(itemCell, islandScript.meshData);
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
                Debug.Log(itemCell);
            }

            GameObject prefab = null;

            switch (regions[islandScript.regionMap[col, row]].name)
            {
                case "Grass": prefab = islandEditor.fieldItems[Random.Range(0, islandEditor.fieldItems.Length)]; break;
                case "Grass 2": prefab = islandEditor.hillItems[Random.Range(0, islandEditor.hillItems.Length)]; break;
            }

            if (prefab != null)
            {
                int orientation = Random.Range(0, 360);
                ItemScript itemScript = Instantiate(prefab, itemPos, Quaternion.Euler(0, orientation, 0), islandScript.items.transform).GetComponent<ItemScript>();
                itemScript.islandScript = islandScript;
                itemScript.itemCell = itemCell;
                islandScript.AddItem(itemScript, itemCell);
            }

            col += Random.Range(1, 20);
            if (col >= mapChunkSize)
            {
                row++;
                col = col - mapChunkSize;
            }
        }

        return islandScript;
    }

    public MapData GenerateMapData(Vector2 centre)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, centre + offset);
        Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
        int[,] regionMap = new int[mapChunkSize, mapChunkSize];

        if(falloffMap == null) falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);

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

        return new MapData(noiseMap, colourMap, regionMap);
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
    public readonly int[,] regionMap;

    public MapData(float[,] heightMap, Color[] colourMap, int[,] regionMap)
    {
        this.heightMap = heightMap;
        this.colourMap = colourMap;
        this.regionMap = regionMap;
    }
}
