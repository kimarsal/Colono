using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class IslandScript : MonoBehaviour
{
    public string islandName;
    public Vector2 offset;
    public bool hasBeenDiscovered;

    public MeshData meshData;
    public int[,] regionMap;

    private GameManager gameManager;
    public IslandEditor islandEditor;
    public NPCManager npcManager;
    public InventoryScript inventoryScript;

    public GameObject convexColliders;
    public GameObject items;
    public GameObject constructions;

    public Dictionary<Vector2, ItemScript> itemList = new Dictionary<Vector2, ItemScript>();
    public List<ConstructionScript> constructionList = new List<ConstructionScript>();


    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        convexColliders = transform.GetChild(0).gameObject;
    }

    public bool isCellTaken(Vector2 cell)
    {
        return itemList.ContainsKey(cell);
    }

    public ItemScript GetItemByCell(Vector2 cell)
    {
        return itemList[cell];
    }

    public void AddItem(ItemScript item, Vector2 cell)
    {
        itemList.Add(cell, item);
    }

    public void RemoveItemAtCell(Vector2 cell)
    {
        itemList.Remove(cell);
    }

    public void AddConstruction(ConstructionScript constructionScript)
    {
        if(constructionScript.constructionType == ConstructionScript.ConstructionType.Building && ((BuildingScript)constructionScript).buildingType == BuildingScript.BuildingType.Warehouse)
        {
            inventoryScript.capacity += 30;
        }
        constructionScript.islandScript = this;
        constructionScript.islandEditor = islandEditor;
        constructionScript.outline = constructionScript.AddComponent<Outline>();
        constructionScript.outline.enabled = false;
        constructionList.Add(constructionScript);

        InvertRegions(constructionScript.cells);
        RebakeNavMesh();
    }

    public EnclosureScript CreateEnclosure(EnclosureScript.EnclosureType enclosureType, Vector2[] selectedCells)
    {
        GameObject enclosure = new GameObject(enclosureType.ToString());
        enclosure.transform.parent = constructions.transform;
        enclosure.transform.localPosition = Vector3.zero;
        EnclosureScript enclosureScript = null;
        switch (enclosureType)
        {
            case EnclosureScript.EnclosureType.Garden: enclosureScript = enclosure.AddComponent<GardenScript>(); break;
            case EnclosureScript.EnclosureType.Pen: enclosureScript = enclosure.AddComponent<PenScript>(); break;
            case EnclosureScript.EnclosureType.Training: enclosureScript = enclosure.AddComponent<TrainingScript>(); break;
        }
        enclosureScript.constructionType = ConstructionScript.ConstructionType.Enclosure;
        enclosureScript.cells = selectedCells;
        enclosureScript.enclosureType = enclosureType;
        enclosureScript.width = (int)selectedCells[selectedCells.Length - 1].x - (int)selectedCells[0].x + 1;
        enclosureScript.length = (int)selectedCells[selectedCells.Length - 1].y - (int)selectedCells[0].y + 1;

        GameObject fences = new GameObject("Fences");
        fences.transform.parent = enclosure.transform;
        fences.transform.localPosition = Vector3.zero;
        Vector3[] positions;
        Quaternion[] rotations;
        MeshGenerator.GetFencePositions(selectedCells, meshData, out positions, out rotations);
        for (int i = 0; i < positions.Length - 1; i++)
        {
            GameObject fence = Instantiate(islandEditor.fences[UnityEngine.Random.Range(0, islandEditor.fences.Length)], transform.position + positions[i], rotations[i], fences.transform);
            if (i == 0) enclosureScript.minPos = fence.transform.localPosition;
            else if (i == positions.Length - 2) enclosureScript.maxPos = fence.transform.localPosition - new Vector3(0, 0, 1);
        }

        if (enclosureType == EnclosureScript.EnclosureType.Pen)
        {
            ((PenScript)enclosureScript).openGate = Instantiate(islandEditor.gateOpen, transform.position + positions[positions.Length - 1], rotations[rotations.Length - 1], fences.transform);
            ((PenScript)enclosureScript).openGate.SetActive(false);
            ((PenScript)enclosureScript).closedGate = Instantiate(islandEditor.gateClosed, transform.position + positions[positions.Length - 1], rotations[rotations.Length - 1], fences.transform);
        }
        else
        {
            GameObject post = Instantiate(islandEditor.post, transform.position + positions[positions.Length - 1], rotations[rotations.Length - 1], fences.transform);
        }

        BoxCollider boxCollider = enclosure.AddComponent<BoxCollider>();
        boxCollider.center = (enclosureScript.minPos + enclosureScript.maxPos) / 2;
        boxCollider.size = new Vector3(enclosureScript.maxPos.x - enclosureScript.minPos.x, 1, enclosureScript.minPos.z - enclosureScript.maxPos.z);
        boxCollider.isTrigger = true;

        enclosureScript.maxPeasants = (enclosureScript.width - 2) * (enclosureScript.length - 2);
        enclosureScript.minPos += transform.position;
        enclosureScript.maxPos += transform.position;

        enclosureScript.entry = Instantiate(islandEditor.enclosureCenter, transform.position + boxCollider.center, Quaternion.identity, enclosure.transform).transform;

        AddConstruction(enclosureScript);

        return enclosureScript;
    }

    public void RemoveConstruction(ConstructionScript constructionScript)
    {
        if (constructionScript.constructionType == ConstructionScript.ConstructionType.Building && ((BuildingScript)constructionScript).buildingType == BuildingScript.BuildingType.Warehouse)
        {
            inventoryScript.capacity -= 30;
        }
        InvertRegions(constructionScript.cells);
        constructionList.Remove(constructionScript);
        Destroy(constructionScript.gameObject);
        StartCoroutine(RebakeNavMeshDelayed());
    }

    private void InvertRegions(Vector2[] cells)
    {
        int minX = (int)cells[0].x, maxX = (int)cells[cells.Length - 1].x;
        int minY = (int)cells[0].y, maxY = (int)cells[cells.Length - 1].y;

        for (int i = minX; i <= maxX; i++)
        {
            for (int j = minY; j <= maxY; j++)
            {
                regionMap[i, j] = -regionMap[i, j];
            }
        }
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

    public IslandInfo GetIslandInfo()
    {
        IslandInfo islandInfo = new IslandInfo();
        islandInfo.islandName = islandName;
        islandInfo.offset = new SerializableVector2(offset);
        islandInfo.hasBeenDiscovered = hasBeenDiscovered;

        islandInfo.itemList = new List<ItemInfo>();
        foreach(KeyValuePair<Vector2, ItemScript> key in itemList)
        {
            islandInfo.itemList.Add(key.Value.GetItemInfo());
        }

        islandInfo.constructionList = new List<ConstructionInfo>();
        foreach(ConstructionScript constructionScript in constructionList)
        {
            islandInfo.constructionList.Add(constructionScript.GetConstructionInfo());
        }

        islandInfo.peasantList = new List<PeasantInfo>();
        foreach(PeasantScript peasantScript in npcManager.peasantList)
        {
            islandInfo.peasantList.Add(peasantScript.GetPeasantInfo());
        }

        islandInfo.inventoryScript = inventoryScript;

        return islandInfo;
    }
}

[System.Serializable]
public class IslandInfo
{
    public string islandName;
    public SerializableVector2 offset;
    public bool hasBeenDiscovered;
    public List<ItemInfo> itemList;
    public List<ConstructionInfo> constructionList;
    public List<PeasantInfo> peasantList;
    public InventoryScript inventoryScript;
}
