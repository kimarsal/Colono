using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine.AI;
using System.Runtime.CompilerServices;
using Random = UnityEngine.Random;
using Unity.VisualScripting;
using static Terrain;
using static BuildingScript;

public class IslandGenerator : MonoBehaviour
{
    public static int mapChunkSize = 101;

    private IslandEditor islandEditor;
    public Material islandMaterial;
    public AnimationCurve meshHeightCurve;
    public Terrain[] regions;

    private float noiseScale = 100;
    private int octaves = 10;
    private float persistance = 0.5f;
    private float lacunarity = 2;
    private float meshHeightMultiplier = 50f;
    private float[,] falloffMap;

    private void Awake()
    {
        islandEditor = GetComponent<IslandEditor>();
    }

    private void Start()
    {
        //falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
    }

    public void LoadIslands(List<IslandScript> islandList)
    {
        IslandManager islandManager = GetComponent<IslandManager>();
        foreach(IslandScript islandScript in islandList)
        {
            islandManager.islandList.Add(GenerateIsland(islandManager.seed, islandScript.offset, islandScript));
        }
    }

    public IslandScript GenerateIsland(int seed, Vector2 offset, IslandScript islandInfo = null)
    {
        // Part 1: Es genera el GameObject i els seus components
        GameObject island = new GameObject("Island");
        island.transform.position = new Vector3(offset.x, 0, offset.y);
        island.transform.parent = transform;
        island.transform.localScale = Vector3.one;
        island.tag = "Island";
        island.isStatic = true;

        MeshRenderer meshRenderer = island.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = island.AddComponent<MeshFilter>();
        MeshCollider meshCollider = island.AddComponent<MeshCollider>();
        meshRenderer.material = islandMaterial;

        // Part 2: Es genera el soroll a partir del seed i l'offset
        MapData mapData = GenerateMapData(seed, offset);
        Texture2D colorTexture = TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize);
        meshRenderer.material.mainTexture = colorTexture;

        // Part 3: Es genera el mesh a partir del heightMap
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve);
        Mesh mesh = meshData.CreateMesh();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;

        // Part 4: S'afegeixen els col·liders que serviran per calcular la distància entre el vaixell i la costa
        /*CMR.ConvexDecomposition.Bake(island, CMR.VHACDSession.Create(), null, false, true, false);
        foreach (MeshCollider triggerCollider in island.transform.GetChild(0).GetComponentsInChildren<MeshCollider>())
        {
            triggerCollider.isTrigger = true;
        }*/

        // Part 5: Es calcula la navegació
        GameObject coastObstacle = Instantiate(islandEditor.coastObstacle, island.transform);
        NavMeshSurface surface = island.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.Children;
        surface.BuildNavMesh();

        // Part 6: S'afegeix el script
        IslandScript islandScript = island.AddComponent<IslandScript>();
        islandScript.offset = offset;
        islandScript.regionMap = mapData.regionMap;
        islandScript.meshData = meshData;
        //islandScript.convexColliders = island.transform.GetChild(0).gameObject;

        // Part 7: S'afegeixen els fills
        islandScript.itemsTransform = new GameObject("Items").transform;
        islandScript.itemsTransform.parent = island.transform;
        islandScript.itemsTransform.localPosition = Vector3.zero;

        islandScript.constructionsTransform = new GameObject("Constructions").transform;
        islandScript.constructionsTransform.parent = island.transform;
        islandScript.constructionsTransform.localPosition = Vector3.zero;

        islandScript.npcsTransform = new GameObject("NPCs").transform;
        islandScript.npcsTransform.parent = island.transform;
        islandScript.npcsTransform.localPosition = Vector3.zero;

        // Part 8: S'afegeixen els elements de joc
        if(islandInfo == null)
        {
            int row = 0, col = 0;
            while (row < mapChunkSize)
            {
                int itemIndex;
                TerrainType terrainType = regions[islandScript.regionMap[col, row]].type;
                ItemScript itemScript = islandEditor.GetRandomItemPrefab(terrainType, out itemIndex);

                if (itemScript != null)
                {
                    Vector2 itemCell = new Vector2(col, row);
                    Vector3 itemPos = island.transform.position + MeshGenerator.GetCellCenter(itemCell, islandScript.meshData);
                    int orientation = Random.Range(0, 360);
                    itemScript.transform.position = itemPos;
                    itemScript.transform.rotation = Quaternion.Euler(0, orientation, 0);
                    itemScript.transform.parent = islandScript.itemsTransform.transform;
                    itemScript.terrainType = terrainType;
                    itemScript.itemIndex = itemIndex;
                    itemScript.itemCell = itemCell;
                    itemScript.orientation = orientation;
                    islandScript.AddItem(itemScript);
                }

                col += Random.Range(1, 20);
                if (col >= mapChunkSize)
                {
                    row++;
                    col = col - mapChunkSize;
                }
            }

            islandScript.inventoryScript = new InventoryScript();
        }
        else
        {
            foreach(KeyValuePair<Vector2, ItemScript> pair in islandInfo.itemDictionary)
            {
                ItemScript itemScript = islandEditor.GetItemPrefab(pair.Value.terrainType, pair.Value.itemIndex);
                Vector3 itemPos = island.transform.position + MeshGenerator.GetCellCenter(pair.Key, islandScript.meshData);
                itemScript.transform.position = itemPos;
                itemScript.transform.rotation = Quaternion.Euler(0, pair.Value.orientation, 0);
                itemScript.transform.parent = islandScript.itemsTransform.transform;
                itemScript.terrainType = pair.Value.terrainType;
                itemScript.itemIndex = pair.Value.itemIndex;
                itemScript.itemCell = pair.Key;
                itemScript.orientation = pair.Value.orientation;
                islandScript.AddItem(itemScript);
            }

            islandScript.inventoryScript = islandInfo.inventoryScript;

            foreach (ConstructionScript constructionInfo in islandInfo.constructionList)
            {
                if(constructionInfo.constructionType == ConstructionScript.ConstructionType.Enclosure)
                {
                    EnclosureScript enclosureInfo = (EnclosureScript)constructionInfo;
                    EnclosureScript enclosureScript = islandScript.CreateEnclosure(enclosureInfo.enclosureType, constructionInfo.cells, enclosureInfo);

                    islandScript.AddConstruction(enclosureScript);
                }
                else
                {
                    BuildingScript buildingInfo = (BuildingScript)constructionInfo;
                    BuildingScript buildingScript = Instantiate(islandEditor.GetBuilding(buildingInfo.buildingType),
                        buildingInfo.position, Quaternion.Euler(0, 90 * buildingInfo.orientation, 0),
                        islandScript.constructionsTransform.transform).GetComponent<BuildingScript>();
                    buildingScript.cells = buildingInfo.cells;
                    buildingScript.orientation = buildingInfo.orientation;

                    if(buildingScript.buildingType == BuildingType.Tavern)
                    {
                        ((TavernScript)buildingScript).recipeList = ((TavernScript)buildingInfo).recipeList;
                    }

                    islandScript.AddConstruction(buildingScript);
                }
            }

            foreach (PeasantScript peasantInfo in islandInfo.peasantList)
            {
                PeasantScript peasantScript = Instantiate(islandEditor.GetNPCPrefab(peasantInfo.peasantType, peasantInfo.peasantGender),
                    peasantInfo.position, Quaternion.Euler(0, peasantInfo.orientation, 0), islandScript.npcsTransform).GetComponent<PeasantScript>();
                peasantScript.InitializePeasant(peasantInfo);
                islandScript.peasantList.Add(peasantScript);
            }
        }
        
        return islandScript;
    }

    public MapData GenerateMapData(int seed, Vector2 offset)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);
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
public struct Terrain
{
    public enum TerrainType { WaterDeep, WaterShallow, Sand, Field, Hill, Rock, Rock2, Snow}
    public TerrainType type;
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
