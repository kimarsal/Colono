using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using static Terrain;
using static BuildingScript;
using System.Collections;

public class IslandGenerator : MonoBehaviour
{
    public static IslandGenerator Instance { get; private set; }

    public int seed;
    public static int mapChunkSize = 101;
    public Material islandMaterial;
    public AnimationCurve meshHeightCurve;
    public Terrain[] regions;
    private float[,] falloffMap;

    public static float noiseScale = 100;
    public static int octaves = 10;
    public static float persistance = 0.5f;
    public static float lacunarity = 2;
    public static float meshHeightMultiplier = 50f;

    private void Awake()
    {
        Instance = this;
    }

    public float[,] GetFalloffMap()
    {
        if(falloffMap == null)
        {
            falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
        }
        return falloffMap;
    }

    public void LoadIslands(List<IslandScript> islandList)
    {
        foreach(IslandScript islandScript in islandList)
        {
            GenerateIsland(islandScript.offset, islandScript);
        }
    }

    public void GenerateIsland(Vector2 offset, IslandScript islandInfo = null)
    {
        // Part 1: Es genera el GameObject i els seus components
        GameObject island = new GameObject("Island");
        island.transform.parent = GameManager.Instance.islandsTransform;
        island.transform.position = new Vector3(offset.x, 0, offset.y);
        island.transform.localScale = Vector3.one;
        island.tag = "Island";
        island.isStatic = true;

        MeshRenderer meshRenderer = island.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = island.AddComponent<MeshFilter>();
        MeshCollider meshCollider = island.AddComponent<MeshCollider>();
        meshRenderer.material = IslandGenerator.Instance.islandMaterial;

        // Part 2: Es genera el soroll a partir del seed i l'offset
        MapData mapData = GenerateMapData(seed, offset);
        Texture2D colorTexture = TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize);
        meshRenderer.material.mainTexture = colorTexture;

        // Part 3: Es genera el mesh a partir del heightMap
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, IslandGenerator.Instance.meshHeightCurve);
        Mesh mesh = meshData.CreateMesh();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;

        // Part 4: S'afegeix el script
        IslandScript islandScript = island.AddComponent<IslandScript>();
        islandScript.offset = offset;
        islandScript.regionMap = mapData.regionMap;
        islandScript.meshData = meshData;

        // Part 6: Es calcula la navegació
        GameObject coastObstacle = Instantiate(IslandEditor.Instance.GetCoastObstacle());
        coastObstacle.transform.parent = island.transform;
        coastObstacle.transform.localPosition = new Vector3(0, -3f, 0);

        islandScript.navMeshSurface = island.AddComponent<NavMeshSurface>();
        islandScript.navMeshSurface.collectObjects = CollectObjects.Children;
        //StartCoroutine(BuildNavMeshAsync(islandScript));
        islandScript.navMeshSurface.BuildNavMesh();

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
        if (islandInfo == null)
        {
            int row = 0, col = 0;
            while (row < mapChunkSize)
            {
                int itemIndex;
                TerrainType terrainType = regions[islandScript.regionMap[col, row]].type;
                ItemScript itemScript = IslandEditor.Instance.GetRandomItemPrefab(terrainType, out itemIndex);

                if (itemScript != null)
                {
                    Vector2 itemCell = new Vector2(col, row);
                    Vector3 itemPos = islandScript.transform.position + MeshGenerator.GetCellCenter(itemCell, islandScript.meshData);
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
            foreach (KeyValuePair<Vector2, ItemScript> pair in islandInfo.itemDictionary)
            {
                ItemScript itemScript = IslandEditor.Instance.GetItemPrefab(pair.Value.terrainType, pair.Value.itemIndex);
                Vector3 itemPos = islandScript.transform.position + MeshGenerator.GetCellCenter(pair.Key, islandScript.meshData);
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
                if (constructionInfo.constructionType == ConstructionScript.ConstructionType.Enclosure)
                {
                    EnclosureScript enclosureInfo = (EnclosureScript)constructionInfo;
                    EnclosureScript enclosureScript = islandScript.CreateEnclosure(enclosureInfo.enclosureType, constructionInfo.cells, enclosureInfo);

                    islandScript.AddConstruction(enclosureScript);
                }
                else
                {
                    BuildingScript buildingInfo = (BuildingScript)constructionInfo;
                    BuildingScript buildingScript = IslandEditor.Instance.GetBuilding(buildingInfo.buildingType);
                    buildingScript.transform.position = buildingInfo.position;
                    buildingScript.transform.rotation = Quaternion.Euler(0, 90 * buildingInfo.orientation, 0);
                    buildingScript.transform.parent = islandScript.constructionsTransform.transform;
                    buildingScript.cells = buildingInfo.cells;
                    buildingScript.orientation = buildingInfo.orientation;

                    if (buildingScript.buildingType == BuildingType.Tavern)
                    {
                        ((TavernScript)buildingScript).recipeList = ((TavernScript)buildingInfo).recipeList;
                    }

                    islandScript.AddConstruction(buildingScript);
                }
            }

            foreach (PeasantScript peasantInfo in islandInfo.peasantList)
            {
                PeasantScript peasantScript = Instantiate(IslandEditor.Instance.GetNPCPrefab(peasantInfo.peasantType, peasantInfo.peasantGender),
                    peasantInfo.position, Quaternion.Euler(0, peasantInfo.orientation, 0), islandScript.npcsTransform);
                peasantScript.InitializePeasant(peasantInfo);
                islandScript.peasantList.Add(peasantScript);
            }
        }

        GameManager.Instance.islandList.Add(islandScript);
    }

    private IEnumerator BuildNavMesh(IslandScript islandScript)
    {
        /*islandScript.navMeshSurface = islandScript.gameObject.AddComponent<NavMeshSurface>();
        islandScript.navMeshSurface.BuildNavMesh();
        yield return null;*/

        var buildSources = islandScript.navMeshSurface.CollectSources();

        var islandBounds = islandScript.navMeshSurface.CalculateWorldBounds(buildSources);

        var buildSettings = islandScript.navMeshSurface.GetBuildSettings();

        yield return null;

        islandScript.navMeshSurface.BuildNavMesh();

        yield return null;
    }

    private IEnumerator BuildNavMeshAsync(IslandScript islandScript)
    {
        // Get the list of all "sources" around us.  This is basically little gridded subsquares of our terrains.
        List<NavMeshBuildSource> buildSources = new List<NavMeshBuildSource>();

        // Set up a boundary area for the build sources collector to look at;
        Bounds islandBounds = new Bounds(islandScript.transform.position, new Vector3(mapChunkSize, meshHeightMultiplier, mapChunkSize));

        // This actually collects them
        NavMeshBuilder.CollectSources(islandBounds, -1, NavMeshCollectGeometry.PhysicsColliders, 0, new List<NavMeshBuildMarkup>(), buildSources);

        yield return null;

        // Get the settings for each of our agent "sizes" (humanoid, giant humanoid)
        NavMeshBuildSettings buildSettings = NavMesh.GetSettingsByIndex(-1);

        // Make a new mesh data object.
        NavMeshData navMeshData = new NavMeshData();

        // "Update" it from scratch.
        AsyncOperation buildOp = NavMeshBuilder.UpdateNavMeshDataAsync(navMeshData, buildSettings, buildSources, islandBounds);

        while (!buildOp.isDone) yield return null;

        NavMesh.AddNavMeshData(navMeshData);

        islandScript.navMeshSurface.navMeshData = navMeshData;

        //islandScript.RebakeNavMesh();

        yield return null;
    }

    public MapData GenerateMapData(int seed, Vector2 offset)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);
        Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
        int[,] regionMap = new int[mapChunkSize, mapChunkSize];
        float[,] falloffMap = IslandGenerator.Instance.GetFalloffMap();

        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < IslandGenerator.Instance.regions.Length; i++)
                {
                    if (currentHeight >= IslandGenerator.Instance.regions[i].height)
                    {
                        colourMap[y * mapChunkSize + x] = IslandGenerator.Instance.regions[i].colour;
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
