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
    public int[] materials = new int[Enum.GetValues(typeof(ResourceScript.MaterialType)).Length];
    public int[] crops = new int[Enum.GetValues(typeof(ResourceScript.CropType)).Length];

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        convexColliders = transform.GetChild(0).gameObject;

        constructions = new GameObject("Constructions");
        constructions.transform.parent = gameObject.transform;
        constructions.transform.localPosition = Vector3.zero;
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

    public void AddMaterial(ResourceScript.MaterialType materialType)
    {
        if (usage < capacity)
        {
            materials[(int)materialType]++;
            usage++;
        }
        else if (gameManager.isInIsland && gameManager.islandScript == this)
        {
            gameManager.shipScript.AddMaterial(materialType);
        }
        gameManager.canvasScript.UpdateMaterial(materialType);
    }

    public void AddCrops(ResourceScript.CropType cropType, int cropAmount)
    {
        int originalAmount = cropAmount;
        if(capacity - usage < originalAmount)
        {
            cropAmount = capacity - usage;
        }

        crops[(int)cropType] += cropAmount;
        usage += cropAmount;

        if(cropAmount < originalAmount && gameManager.isInIsland && gameManager.islandScript == this)
        {
            gameManager.shipScript.AddCrops(cropType, originalAmount - cropAmount);
        }
        gameManager.canvasScript.UpdateCrop(cropType);
    }

    public int GetCropAmount(ResourceScript.CropType cropType)
    {
        int seedAmount = crops[(int)cropType];
        if (gameManager.isInIsland) seedAmount += gameManager.shipScript.crops[(int)cropType];
        return seedAmount;
    }

    public bool UseCrop(ResourceScript.CropType cropType)
    {
        if (crops[(int)cropType] > 0)
        {
            crops[(int)cropType]--;
            usage--;
            return true;
        }
        else if (gameManager.isInIsland && gameManager.shipScript.crops[(int)cropType] > 0)
        {
            gameManager.shipScript.crops[(int)cropType]--;
            gameManager.shipScript.usage--;
            return true;
        }
        return false;
    }
}
