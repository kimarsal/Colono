using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class IslandScript : TaskSourceScript
{
    public string islandName;
    public Vector2 offset;
    public bool hasBeenDiscovered;
    public MeshData meshData;
    public int[,] regionMap;

    public GameManager gameManager;
    public IslandEditor islandEditor { get { return gameManager.islandEditor; } }
    public InventoryScript inventoryScript;

    public GameObject convexColliders;
    public Transform itemsTransform;
    public Transform constructionsTransform;
    public Transform npcsTransform;

    private Dictionary<Vector2, ItemScript> itemDictionary = new Dictionary<Vector2, ItemScript>();
    public List<ConstructionScript> constructionList = new List<ConstructionScript>();
    public List<PeasantScript> peasantList = new List<PeasantScript>();


    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        convexColliders = transform.GetChild(0).gameObject;
    }

    public bool isCellTaken(Vector2 cell)
    {
        return itemDictionary.ContainsKey(cell);
    }

    public ItemScript GetItemByCell(Vector2 cell)
    {
        return itemDictionary[cell];
    }

    public void AddItem(ItemScript item, Vector2 cell)
    {
        itemDictionary.Add(cell, item);
    }

    public void RemoveItemAtCell(Vector2 cell)
    {
        itemDictionary.Remove(cell);
    }

    public void AddConstruction(ConstructionScript constructionScript)
    {
        if(constructionScript.constructionType == ConstructionScript.ConstructionType.Building)
        {
            constructionScript.outline = constructionScript.AddComponent<Outline>();
            constructionScript.outline.enabled = false;
            
            if(((BuildingScript)constructionScript).buildingType == BuildingScript.BuildingType.Warehouse)
            {
                inventoryScript.capacity += 30;
            }
        }
        constructionScript.islandScript = this;
        constructionList.Add(constructionScript);

        InvertRegions(constructionScript.cells);
        RebakeNavMesh();
    }

    public EnclosureScript CreateEnclosure(EnclosureScript.EnclosureType enclosureType, Vector2[] selectedCells, EnclosureInfo enclosureInfo = null)
    {
        GameObject enclosure = new GameObject(enclosureType.ToString());
        enclosure.transform.parent = constructionsTransform.transform;
        enclosure.transform.localPosition = Vector3.zero;
        EnclosureScript enclosureScript = null;
        switch (enclosureType)
        {
            case EnclosureScript.EnclosureType.Garden: enclosureScript = enclosure.AddComponent<GardenScript>(); break;
            case EnclosureScript.EnclosureType.Pen: enclosureScript = enclosure.AddComponent<PenScript>(); break;
            case EnclosureScript.EnclosureType.Training: enclosureScript = enclosure.AddComponent<TrainingScript>(); break;
        }
        enclosureScript.enclosureType = enclosureType;
        enclosureScript.cells = selectedCells;
        enclosureScript.InitializeEnclosure(enclosureInfo, this);

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

    public BuildingScript GetAvailableBuilding(BuildingScript.BuildingType buildingType, PeasantScript peasantScript)
    {
        BuildingScript closestBuildingScript = null;
        float minDistance = -1;
        foreach (ConstructionScript construction in constructionList)
        {
            if(construction.constructionType == ConstructionScript.ConstructionType.Building)
            {
                BuildingScript buildingScript = (BuildingScript)construction;
                float distance;
                if (buildingScript.buildingType == buildingType && buildingScript.peasantList.Count < buildingScript.maxPeasants
                    && NPCManager.CheckIfClosest(buildingScript.transform.position, peasantScript.GetComponent<NavMeshAgent>(), minDistance, out distance))
                {
                    closestBuildingScript = buildingScript;
                    minDistance = distance;
                }
            }
        }
        return closestBuildingScript;
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
        foreach(KeyValuePair<Vector2, ItemScript> key in itemDictionary)
        {
            islandInfo.itemList.Add(key.Value.GetItemInfo());
        }

        islandInfo.constructionList = new List<ConstructionInfo>();
        foreach(ConstructionScript constructionScript in constructionList)
        {
            islandInfo.constructionList.Add(constructionScript.GetConstructionInfo());
        }

        islandInfo.peasantList = new List<PeasantInfo>();
        foreach(PeasantScript peasantScript in peasantList)
        {
            islandInfo.peasantList.Add(peasantScript.GetPeasantInfo());
        }

        islandInfo.inventoryScript = inventoryScript;

        return islandInfo;
    }

    /*NPCMANAGER*/

    public void AddItemToClear(ItemScript itemScript)
    {
        taskList.Add(itemScript);

        PeasantAdultScript closestPeasantScript = (PeasantAdultScript)GetClosestPeasant(itemScript.center, true);
        if (closestPeasantScript != null)
        {
            closestPeasantScript.AssignTask(itemScript);
        }
    }

    public PeasantScript GetClosestPeasant(Vector3 position, bool mustBeAvailableAdult)
    {
        PeasantScript closestPeasantScript = null;
        float minDistance = -1;
        for (int i = 0; i < peasantList.Count; i++)
        {
            PeasantScript newPeasantScript = peasantList[i];
            if (!mustBeAvailableAdult //Si pot ser qualsevol
                || (newPeasantScript.peasantType == PeasantScript.PeasantType.Adult //Si és un adult
                && ((PeasantAdultScript)newPeasantScript).CanBeAsignedTask())) //Si és un adult disponible
            {
                float distance;
                if (NPCManager.CheckIfClosest(position, newPeasantScript.GetComponent<NavMeshAgent>(), minDistance, out distance))
                {
                    closestPeasantScript = newPeasantScript;
                    minDistance = distance;
                }
            }
        }
        return closestPeasantScript;
    }

    public void RemoveItemToClear(ItemScript item)
    {
        taskList.Remove(item);
        PeasantAdultScript peasantScript = item.peasantAdultScript;
        if (peasantScript != null) //Tenia un NPC vinculat
        {
            item.peasantAdultScript = null;
            GetNextPendingTask(peasantScript);
        }
    }

    public override bool GetNextPendingTask(PeasantAdultScript peasantAdultScript)
    {
        if (!base.GetNextPendingTask(peasantAdultScript)) return false;

        ItemScript closestItemScript = null;
        float minDistance = -1;
        foreach (ItemScript itemScript in taskList)
        {
            float distance;
            if (itemScript.peasantAdultScript == null //Si no té un NPC vinculat
                && NPCManager.CheckIfClosest(itemScript.transform.position, peasantAdultScript.GetComponent<NavMeshAgent>(), minDistance, out distance))
            {
                closestItemScript = itemScript;
                minDistance = distance;
            }
        }
        peasantAdultScript.AssignTask(closestItemScript);
        return closestItemScript != null;
    }

    public bool ManagePeasants(ConstructionScript constructionScript, bool adding)
    {
        if (adding) // Enviar a la construcció
        {
            PeasantScript peasantScript = GetClosestPeasant(constructionScript.entry.position, constructionScript.constructionType != ConstructionScript.ConstructionType.Ship);

            if (peasantScript != null)
            {
                peasantList.Remove(peasantScript);
                constructionScript.AddPeasant(peasantScript);
                peasantScript.UpdateTask();
                return true;
            }
            return false;
        }
        else // Desvincular de la construcció
        {
            PeasantScript peasantScript = constructionScript.RemovePeasant();
            peasantScript.constructionScript = null;
            peasantList.Add(peasantScript);

            if (peasantScript.peasantType == PeasantScript.PeasantType.Adult)
            {
                GetNextPendingTask((PeasantAdultScript)peasantScript);
            }
            else peasantScript.UpdateTask();
            return true;
        }
    }

    public void SendAllPeasantsBack(ConstructionScript constructionScript)
    {
        for (int i = 0; i < constructionScript.peasantList.Count; i++)
        {
            PeasantScript peasantScript = constructionScript.RemovePeasant();
            peasantList.Add(peasantScript);
            if (peasantScript.peasantType == PeasantScript.PeasantType.Adult)
            {
                GetNextPendingTask((PeasantAdultScript)peasantScript);
            }
            else peasantScript.UpdateTask();
        }
    }

    public void CancelAllTripsToShip(ShipScript shipScript)
    {
        for (int i = shipScript.peasantList.Count - 1; i >= 0; i--)
        {
            PeasantScript peasantScript = shipScript.peasantList[i];
            peasantList.Add(peasantScript);
            shipScript.peasantList.Remove(peasantScript);
            peasantScript.constructionScript = null;
            if (peasantScript.peasantType == PeasantScript.PeasantType.Adult) GetNextPendingTask((PeasantAdultScript)peasantScript);
            else peasantScript.UpdateTask();
        }
        shipScript.peasantsOnTheirWay = 0;
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
