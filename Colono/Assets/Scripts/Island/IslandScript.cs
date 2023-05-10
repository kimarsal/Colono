using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static Terrain;

[JsonObject(MemberSerialization.OptIn)]
public class IslandScript : MonoBehaviour, TaskSourceInterface
{
    [JsonProperty] public string islandName;
    [JsonProperty] [JsonConverter(typeof(VectorConverter))] public Vector2 offset;
    [JsonProperty] public bool hasBeenDiscovered;
    public MeshData meshData;
    public int[,] regionMap;

    public NavMeshSurface navMeshSurface;
    public Transform itemsTransform;
    public Transform constructionsTransform;
    public Transform npcsTransform;

    [JsonProperty] public InventoryScript inventoryScript;
    [JsonProperty] [JsonConverter(typeof(VectorDictionaryConverter))] public Dictionary<Vector2, ItemScript> itemDictionary = new Dictionary<Vector2, ItemScript>();
    public List<ItemScript> itemsToClear = new List<ItemScript>();
    [JsonProperty] public List<ConstructionScript> constructionList = new List<ConstructionScript>();
    [JsonProperty] [JsonConverter(typeof(PeasantListConverter))] public List<PeasantScript> peasantList = new List<PeasantScript>();

    public bool isCellTaken(Vector2 cell)
    {
        return itemDictionary.ContainsKey(cell);
    }

    public ItemScript GetItemByCell(Vector2 cell)
    {
        return itemDictionary[cell];
    }

    public void AddItem(ItemScript itemScript)
    {
        itemScript.islandScript = this;
        itemDictionary.Add(itemScript.cell, itemScript);
    }

    public void RemoveItemAtCell(Vector2 cell)
    {
        itemDictionary.Remove(cell);
    }

    public void PlantTrees(Vector2[] selectedCells)
    {
        foreach (Vector2 cell in selectedCells)
        {
            Vector3 itemPos = transform.position + MeshGenerator.GetCellCenter(cell, meshData);
            int orientation = Random.Range(0, 360);
            TerrainType terrainType = GameManager.Instance.GetComponent<IslandGenerator>().regions[regionMap[(int)cell.x, (int)cell.y]].type;

            TreeSproutScript treeSproutScript = Instantiate(ResourceScript.Instance.treeSprout, itemPos,
                Quaternion.Euler(0, orientation, 0), itemsTransform.transform).GetComponent<TreeSproutScript>();
            treeSproutScript.terrainType = terrainType;
            treeSproutScript.itemIndex = -1;
            treeSproutScript.cell = cell;
            treeSproutScript.orientation = orientation;
            AddItem(treeSproutScript);
        }
    }

    public void AddConstruction(ConstructionScript constructionScript)
    {
        if(constructionScript.constructionType == ConstructionScript.ConstructionType.Building)
        {
            constructionScript.outline = constructionScript.AddComponent<Outline>();
            constructionScript.outline.enabled = false;

            constructionScript.entry.position = NPCManager.GetClosestPointInNavMesh(constructionScript.entry.position);

            if(((BuildingScript)constructionScript).buildingType == BuildingScript.BuildingType.Warehouse)
            {
                inventoryScript.AddCapacityToAllCategories();
            }
            else if (((BuildingScript)constructionScript).buildingType == BuildingScript.BuildingType.Tavern)
            {
                ((TavernScript)constructionScript).recipeList.Add(new Recipe());
            }
        }
        constructionScript.islandScript = this;
        constructionList.Add(constructionScript);

        InvertRegions(constructionScript.cells);
        RebakeNavMesh();
    }

    public EnclosureScript CreateEnclosure(EnclosureScript.EnclosureType enclosureType, Vector2[] selectedCells, EnclosureScript enclosureInfo = null)
    {
        GameObject enclosure = new GameObject(enclosureType.ToString());
        enclosure.transform.parent = constructionsTransform.transform;
        enclosure.transform.localPosition = Vector3.zero;
        EnclosureScript enclosureScript = null;
        switch (enclosureType)
        {
            case EnclosureScript.EnclosureType.Garden: enclosureScript = enclosure.AddComponent<GardenScript>(); break;
            case EnclosureScript.EnclosureType.Pen: enclosureScript = enclosure.AddComponent<PenScript>(); break;
        }
        enclosureScript.enclosureType = enclosureType;
        enclosureScript.cells = selectedCells;
        enclosureScript.InitializeEnclosure(enclosureInfo, this);

        return enclosureScript;
    }

    public void RemoveConstruction(ConstructionScript constructionScript)
    {
        constructionScript.SendAllPeasantsBack();
        constructionScript.FinishUpBusiness();

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
        navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);

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

    public void AddResource(ResourceScript.ResourceType resourceType, int resourceIndex, int amount = 1)
    {
        int remainingAmount = amount;
        if (resourceType == ResourceScript.ResourceType.Crop)
        {
            if (!GameManager.Instance.CheckIfCropIsNew(resourceIndex))
            {
                foreach (GardenScript gardenScript in constructionList)
                {
                    if (gardenScript == null) continue;

                    remainingAmount = gardenScript.UseNewCrops((ResourceScript.CropType)resourceIndex, remainingAmount);

                    if (remainingAmount == 0) break;
                }
            }
        }

        remainingAmount = inventoryScript.AddResource(resourceType, resourceIndex, remainingAmount);
        if (remainingAmount > 0 && GameManager.Instance.isInIsland && GameManager.Instance.closestIsland == this)
        {
            ShipScript.Instance.shipInterior.inventoryScript.AddResource(resourceType, resourceIndex, remainingAmount);
        }

        if (!(GameManager.Instance.isInIsland && GameManager.Instance.closestIsland == this)) return;

        CanvasScript.Instance.ShowInventoryChange(resourceType, resourceIndex, amount);

        CanvasScript.Instance.UpdateInventoryRow(resourceType, resourceIndex);

        if (resourceType == ResourceScript.ResourceType.Material)
        {
            if (resourceIndex == (int)ResourceScript.MaterialType.Gem)
            {
                CanvasScript.Instance.constructionDetailsScript.UpdateUpgradeButton();
            }
            else
            {
                CanvasScript.Instance.UpdateTopButtons();
            }
        }
    }

    public int GetResourceAmount(ResourceScript.ResourceType resourceType, int resourceIndex)
    {
        int amount = inventoryScript.GetResourceAmount(resourceType, resourceIndex);
        if (GameManager.Instance.isInIsland) amount += ShipScript.Instance.shipInterior.inventoryScript.GetResourceAmount(resourceType, resourceIndex);
        return amount;
    }

    public bool UseResource(ResourceScript.ResourceType resourceType, int resourceIndex, int amount = 1)
    {
        if (GetResourceAmount(resourceType, resourceIndex) < amount) return false;

        int remainingAmount = inventoryScript.RemoveResource(resourceType, resourceIndex, amount);
        if(remainingAmount > 0)
        {
            ShipScript.Instance.shipInterior.inventoryScript.RemoveResource(resourceType, resourceIndex, remainingAmount);
        }

        CanvasScript.Instance.ShowInventoryChange(resourceType, resourceIndex, -amount);

        return true;
    }

    /*NPCMANAGER*/

    public void AddItemToClear(ItemScript itemScript)
    {
        itemsToClear.Add(itemScript);

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
                && ((PeasantAdultScript)newPeasantScript).task == null)) //Si és un adult disponible
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
        itemsToClear.Remove(item);
        PeasantAdultScript peasantScript = item.peasantAdultScript;
        if (peasantScript != null) //Tenia un NPC vinculat
        {
            peasantScript.task = null;
            item.peasantAdultScript = null;
            GetNextPendingTask(peasantScript);
        }
    }

    public bool GetNextPendingTask(PeasantAdultScript peasantAdultScript)
    {
        if (!peasantAdultScript.CanBeAsignedTask())
        {
            peasantAdultScript.UpdateTask();
            return false;
        }

        ItemScript closestItemScript = null;
        float minDistance = -1;
        foreach (ItemScript itemScript in itemsToClear)
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
                if(peasantScript.peasantType == PeasantScript.PeasantType.Adult)
                {
                    ((PeasantAdultScript)peasantScript).CancelTask();
                }
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
            peasantList.Add(peasantScript);

            if (peasantScript.peasantType == PeasantScript.PeasantType.Adult)
            {
                GetNextPendingTask((PeasantAdultScript)peasantScript);
            }
            else peasantScript.UpdateTask();
            return true;
        }
    }

}