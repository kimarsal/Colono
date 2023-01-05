using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class IslandScript : MonoBehaviour
{
    public enum ItemType { Tree, Bush, Rock, Flower, Miscellaneous};
    public bool hasBeenDiscovered = false;
    public string islandName;

    public GameManager gameManager;
    public IslandCellScript islandCellScript;
    public NPCManager npcManager;

    public GameObject convexColliders;
    public GameObject items;
    public GameObject constructions;

    private List<ConstructionScript> constructionList = new List<ConstructionScript>();

    private Dictionary<Vector2, GameObject> itemsList = new Dictionary<Vector2, GameObject>();

    public int capacity;
    public int usage;
    public int[][] resources;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        convexColliders = transform.GetChild(0).gameObject;

        constructions = new GameObject("Constructions");
        constructions.transform.parent = gameObject.transform;
        constructions.transform.localPosition = Vector3.zero;

        resources = new int[Enum.GetValues(typeof(ResourceScript.ResourceType)).Length][];
        resources[0] = new int[Enum.GetValues(typeof(ResourceScript.MaterialType)).Length];
        resources[1] = new int[Enum.GetValues(typeof(ResourceScript.CropType)).Length];
        resources[2] = new int[Enum.GetValues(typeof(ResourceScript.MeatType)).Length];
        resources[3] = new int[Enum.GetValues(typeof(ResourceScript.AnimalType)).Length];
    }

    public bool isCellTaken(Vector2 cell)
    {
        return itemsList.ContainsKey(cell);
    }

    public ItemScript GetItemByCell(Vector2 cell)
    {
        return itemsList[cell].GetComponent<ItemScript>();
    }

    public void AddItem(GameObject item, Vector2 cell)
    {
        itemsList.Add(cell, item);
    }

    public void RemoveItemAtCell(Vector2 cell)
    {
        itemsList.Remove(cell);
    }

    public void AddConstruction(ConstructionScript constructionScript)
    {
        if(constructionScript.constructionType == ConstructionScript.ConstructionType.Building && ((BuildingScript)constructionScript).buildingType == BuildingScript.BuildingType.Warehouse)
        {
            capacity += 30;
        }
        constructionList.Add(constructionScript);
        RebakeNavMesh();
    }

    public void RemoveConstruction(ConstructionScript constructionScript)
    {
        if (constructionScript.constructionType == ConstructionScript.ConstructionType.Building && ((BuildingScript)constructionScript).buildingType == BuildingScript.BuildingType.Warehouse)
        {
            capacity -= 30;
        }
        constructionList.Remove(constructionScript);
        Destroy(constructionScript.gameObject);
        StartCoroutine(RebakeNavMeshDelayed());
    }

    private IEnumerator RebakeNavMeshDelayed()
    {
        yield return new WaitForSeconds(0.1f);
        RebakeNavMesh();
    }

    public void RebakeNavMesh()
    {
        GetComponent<NavMeshSurface>().UpdateNavMesh(GetComponent<NavMeshSurface>().navMeshData);

    }

    public ConstructionScript GetConstructionByCell(Vector2 cell)
    {
        foreach (ConstructionScript constructionScript in constructionList)
        {
            Vector2[] cells = constructionScript.cells;
            if (cell.x >= cells[0].x && cell.x <= cells[cells.Length - 1].x
                && cell.y >= cells[0].y && cell.y <= cells[cells.Length - 1].y)
            {
                return constructionScript;
            }
        }
        return null;
    }

    public BuildingScript GetAvailableBuilding(BuildingScript.BuildingType buildingType)
    {
        foreach (BuildingScript building in constructionList)
        {
            if (building.buildingType == buildingType && building.peasantList.Count < building.maxPeasants)
            {
                return building;
            }
        }
        return null;
    }

    public void AddResource(ResourceScript.ResourceType resourceType, int resourceIndex, int amount = 1)
    {
        int originalAmount = amount;
        if(capacity - usage < originalAmount)
        {
            amount = capacity - usage;
        }

        resources[(int)resourceType][resourceIndex] += amount;
        usage += amount;

        if(amount < originalAmount && gameManager.isInIsland && gameManager.islandScript == this)
        {
            gameManager.shipScript.AddResource(resourceType, resourceIndex, originalAmount - amount);
        }
        gameManager.canvasScript.UpdateInventoryRow(resourceType, resourceIndex);
    }

    public int GetCropAmount(ResourceScript.CropType cropType)
    {
        int seedAmount = resources[(int)ResourceScript.ResourceType.Crop][(int)cropType];
        if (gameManager.isInIsland) seedAmount += gameManager.shipScript.resources[(int)ResourceScript.ResourceType.Crop][(int)cropType];
        return seedAmount;
    }

    public bool UseResource(ResourceScript.ResourceType resourceType, int resourceIndex)
    {
        if (resources[(int)resourceType][resourceIndex] > 0)
        {
            resources[(int)resourceType][resourceIndex]--;
            usage--;
            return true;
        }
        else if (gameManager.isInIsland && gameManager.shipScript.resources[(int)resourceType][resourceIndex] > 0)
        {
            gameManager.shipScript.resources[(int)resourceType][resourceIndex]--;
            gameManager.shipScript.usage--;
            return true;
        }
        return false;
    }
}
