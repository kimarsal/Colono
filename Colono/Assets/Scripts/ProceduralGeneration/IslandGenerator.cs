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
    public static int mapChunkSize = 241;

    public Material islandMaterial;
    public AnimationCurve meshHeightCurve;
    public Terrain[] regions;

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

    public void LoadIslands(List<IslandInfo> islandList)
    {
        IslandManager islandManager = GetComponent<IslandManager>();
        foreach(IslandInfo islandInfo in islandList)
        {
            islandManager.islandList.Add(GenerateIsland(islandManager.seed, islandInfo.offset.UnityVector, GetComponent<IslandEditor>(), islandInfo));
        }
    }

    public IslandScript GenerateIsland(int seed, Vector2 offset, IslandEditor islandEditor, IslandInfo islandInfo = null)
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
        islandScript.offset = offset;
        islandScript.regionMap = mapData.regionMap;
        islandScript.meshData = meshData;

        islandScript.npcManager = island.AddComponent<NPCManager>();
        islandScript.npcManager.islandEditor = islandEditor;
        islandScript.npcManager.islandScript = islandScript;

        islandScript.islandEditor = islandEditor;

        // Part 7: S'afegeixen els fills
        islandScript.items = new GameObject("Items");
        islandScript.items.transform.parent = island.transform;
        islandScript.items.transform.localPosition = Vector3.zero;

        GameObject npcs = new GameObject("NPCs");
        npcs.transform.parent = island.transform;
        npcs.transform.localPosition = Vector3.zero;
        islandScript.npcManager.npcs = npcs;
        
        islandScript.constructions = new GameObject("Constructions");
        islandScript.constructions.transform.parent = island.transform;
        islandScript.constructions.transform.localPosition = Vector3.zero;

        // Part 8: S'afegeixen els elements de joc
        if(islandInfo == null)
        {
            int row = 0, col = 0;
            while (row < mapChunkSize)
            {
                int itemIndex;
                Vector2 itemCell = new Vector2(col, row);
                TerrainType terrainType = regions[islandScript.regionMap[col, row]].type;
                GameObject itemPrefab = islandEditor.GetRandomItemPrefab(terrainType, out itemIndex);

                if (itemPrefab != null)
                {
                    Vector3 itemPos = island.transform.position + MeshGenerator.GetCellCenter(itemCell, islandScript.meshData);
                    int orientation = Random.Range(0, 360);
                    ItemScript itemScript = Instantiate(itemPrefab, itemPos, Quaternion.Euler(0, orientation, 0), islandScript.items.transform).GetComponent<ItemScript>();
                    itemScript.islandScript = islandScript;
                    itemScript.terrainType = terrainType;
                    itemScript.itemIndex = itemIndex;
                    itemScript.itemCell = itemCell;
                    itemScript.orientation = orientation;
                    islandScript.AddItem(itemScript, itemCell);
                }

                col += Random.Range(1, 20);
                if (col >= mapChunkSize)
                {
                    row++;
                    col = col - mapChunkSize;
                }
            }

            islandScript.inventoryScript = new InventoryScript();
            islandScript.inventoryScript.capacity = 30;
        }
        else
        {
            foreach(ItemInfo itemInfo in islandInfo.itemList)
            {
                GameObject itemPrefab = islandEditor.GetItemPrefab(itemInfo.terrainType, itemInfo.itemIndex);
                Vector3 itemPos = island.transform.position + MeshGenerator.GetCellCenter(itemInfo.cell.UnityVector, islandScript.meshData);
                ItemScript itemScript = Instantiate(itemPrefab, itemPos, Quaternion.Euler(0, itemInfo.orientation, 0), islandScript.items.transform).GetComponent<ItemScript>();
                itemScript.islandScript = islandScript;
                itemScript.terrainType = itemInfo.terrainType;
                itemScript.itemIndex = itemInfo.itemIndex;
                itemScript.itemCell = itemInfo.cell.UnityVector;
                itemScript.orientation = itemInfo.orientation;
                islandScript.AddItem(itemScript, itemInfo.cell.UnityVector);
            }

            islandScript.inventoryScript = islandInfo.inventoryScript;

            foreach (ConstructionInfo constructionInfo in islandInfo.constructionList)
            {
                if(constructionInfo.constructionType == ConstructionScript.ConstructionType.Enclosure)
                {
                    EnclosureInfo enclosureInfo = (EnclosureInfo)constructionInfo;
                    EnclosureScript enclosureScript = islandScript.CreateEnclosure(enclosureInfo.enclosureType, SerializableVector2.GetSerializableArray(constructionInfo.cells));
                    switch (enclosureScript.enclosureType)
                    {
                        case EnclosureScript.EnclosureType.Garden: ((GardenScript)enclosureScript).InitializeGarden((GardenInfo)enclosureInfo); break;
                        case EnclosureScript.EnclosureType.Pen: ((PenScript)enclosureScript).InitializePen((PenInfo)enclosureInfo); break;
                        case EnclosureScript.EnclosureType.Training: break;
                    }
                }
                else
                {
                    BuildingInfo buildingInfo = (BuildingInfo)constructionInfo;
                    BuildingScript buildingScript = Instantiate(islandEditor.GetBuilding(buildingInfo.buildingType),
                        buildingInfo.position.UnityVector, Quaternion.Euler(0, 90 * buildingInfo.orientation, 0),
                        islandScript.constructions.transform).GetComponent<BuildingScript>();
                    buildingScript.cells = SerializableVector2.GetSerializableArray(buildingInfo.cells);
                    buildingScript.orientation = buildingInfo.orientation;

                    if(buildingScript.buildingType == BuildingType.Tavern)
                    {
                        ((TavernScript)buildingScript).recipeList = ((TavernInfo)buildingInfo).recipeList;
                    }

                    islandScript.AddConstruction(buildingScript);
                }
            }

            foreach (PeasantInfo peasantInfo in islandInfo.peasantList)
            {
                GameObject peasantPrefab = islandEditor.GetNPCPrefab(peasantInfo.peasantType, peasantInfo.peasantGender);
                PeasantScript peasantScript = Instantiate(peasantPrefab, peasantInfo.position.UnityVector,
                    Quaternion.Euler(0, peasantInfo.orientation, 0), npcs.transform).GetComponent<PeasantScript>();
                peasantScript.transform.localScale = Vector3.one * 0.4f;
                peasantScript.InitializePeasant(peasantInfo);
                peasantScript.npcManager = islandScript.npcManager;
                islandScript.npcManager.peasantList.Add(peasantScript);
                peasantScript.UpdateTask();
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
    public enum TerrainType { WaterDeep, WaterShallow, Sand, Grass, Grass2, Rock, Rock2, Snow}
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
