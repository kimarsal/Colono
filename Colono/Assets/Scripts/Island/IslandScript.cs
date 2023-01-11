using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using static UnityEditor.Experimental.GraphView.Port;

public class IslandScript : MonoBehaviour
{
    public bool hasBeenDiscovered = false;
    public string islandName;
    public MeshData meshData;
    public int[,] regionMap;

    public GameManager gameManager;
    public NPCManager npcManager;
    public InventoryScript inventoryScript;

    public GameObject convexColliders;
    public GameObject items;
    public GameObject constructions;

    private List<ConstructionScript> constructionList = new List<ConstructionScript>();

    private Dictionary<Vector2, ItemScript> itemsList = new Dictionary<Vector2, ItemScript>();

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        convexColliders = transform.GetChild(0).gameObject;
    }

    public bool isCellTaken(Vector2 cell)
    {
        return itemsList.ContainsKey(cell);
    }

    public ItemScript GetItemByCell(Vector2 cell)
    {
        return itemsList[cell];
    }

    public void AddItem(ItemScript item, Vector2 cell)
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
            inventoryScript.capacity += 30;
        }
        constructionList.Add(constructionScript);
        RebakeNavMesh();
    }

    public void RemoveConstruction(ConstructionScript constructionScript)
    {
        if (constructionScript.constructionType == ConstructionScript.ConstructionType.Building && ((BuildingScript)constructionScript).buildingType == BuildingScript.BuildingType.Warehouse)
        {
            inventoryScript.capacity -= 30;
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
        foreach (ConstructionScript construction in constructionList)
        {
            if(construction.constructionType == ConstructionScript.ConstructionType.Building)
            {
                BuildingScript building = (BuildingScript)construction;
                if (building.buildingType == buildingType && building.peasantList.Count < building.maxPeasants)
                {
                    return building;
                }
            }
        }
        return null;
    }

    public bool AddResource(ResourceScript.ResourceType resourceType, int resourceIndex, int amount = 1)
    {
        int remainingAmount = inventoryScript.AddResource(resourceType, resourceIndex, amount);
        if (remainingAmount > 0 && gameManager.isInIsland)
        {
            remainingAmount = gameManager.shipScript.inventoryScript.AddResource(resourceType, resourceIndex, remainingAmount);
        }
        gameManager.canvasScript.UpdateInventoryRow(resourceType, resourceIndex);

        return remainingAmount == 0;
    }

    public int GetResourceAmount(ResourceScript.ResourceType resourceType, int resourceIndex)
    {
        int amount = inventoryScript.GetResourceAmount(resourceType, resourceIndex);
        if (gameManager.isInIsland) amount += gameManager.shipScript.inventoryScript.GetResourceAmount(resourceType, resourceIndex);
        return amount;
    }

    public bool UseResource(ResourceScript.ResourceType resourceType, int resourceIndex, int amount = 1)
    {
        if (GetResourceAmount(resourceType, resourceIndex) < amount) return false;

        int remainingAmount = inventoryScript.RemoveResource(resourceType, resourceIndex, amount);
        if(remainingAmount > 0)
        {
            gameManager.shipScript.inventoryScript.RemoveResource(resourceType, resourceIndex, remainingAmount);
        }
        return true;
    }

}

[System.Serializable]
public class IslandInfo
{
    public Vector2 position;
    public List<ItemInfo> items;
    public List<ConstructionInfo> constructions;
}
