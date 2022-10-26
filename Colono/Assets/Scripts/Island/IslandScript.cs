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

    public GameManager gameManagerScript;
    public IslandCellScript islandCellScript;
    public NPCManager npcManager;

    private GameObject convexColliders;
    public GameObject items;
    public GameObject constructions;

    private List<EnclosureScript> enclosuresList = new List<EnclosureScript>();
    private List<BuildingScript> buildingsList = new List<BuildingScript>();

    private Dictionary<Vector2, GameObject> itemsList = new Dictionary<Vector2, GameObject>();

    public int capacity;
    public int usage;
    public int[] materials = new int[Enum.GetValues(typeof(ResourceScript.MaterialType)).Length];
    public int[] crops = new int[Enum.GetValues(typeof(ResourceScript.CropType)).Length];

    private void Start()
    {
        gameManagerScript = GameObject.Find("GameManager").GetComponent<GameManager>();
        convexColliders = transform.GetChild(0).gameObject;

        constructions = new GameObject("Constructions");
        constructions.transform.parent = gameObject.transform;
        constructions.transform.localPosition = Vector3.zero;
    }

    public void PlayerIsNear()
    {
        gameManagerScript.PlayerIsNearIsland(this);
    }

    public void PlayerEntered()
    {
        convexColliders.SetActive(false);
    }

    public void PLayerLeft()
    {
        convexColliders.SetActive(true);
    }

    public void PlayerIsFar()
    {
        gameManagerScript.PlayerIsFarFromIsland();
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

    /*public void ClearArea(Vector2[] cells)
    {
        foreach(Vector2 cell in cells)
        {
            if (isCellTaken(cell))
            {
                RemoveItemAtCell(cell);
            }
        }
    }*/

    public void RemoveItemAtCell(Vector2 cell)
    {
        itemsList.Remove(cell);
    }

    public void AddBuilding(BuildingScript building)
    {
        if(building.buildingType == BuildingScript.BuildingType.Warehouse)
        {
            capacity += 30;
        }
        buildingsList.Add(building);
        RebakeNavMesh();
    }

    public void RemoveBuilding(BuildingScript building)
    {
        if (building.buildingType == BuildingScript.BuildingType.Warehouse)
        {
            capacity -= 30;
        }
        buildingsList.Remove(building);
        Destroy(building.gameObject);
        StartCoroutine(RebakeNavMeshDelayed());
    }

    public void AddEnclosure(EnclosureScript enclosure)
    {
        enclosuresList.Add(enclosure);
        RebakeNavMesh();
    }

    public void RemoveEnclosure(EnclosureScript enclosure)
    {
        enclosuresList.Remove(enclosure);
        Destroy(enclosure.gameObject);
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

    public EnclosureScript GetEnclosureByCell(Vector2 cell)
    {
        foreach(EnclosureScript enclosure in enclosuresList)
        {
            Vector2[] cells = enclosure.cells;
            if(cell.x >= cells[0].x && cell.x <= cells[cells.Length - 1].x
                && cell.y >= cells[0].y && cell.y <= cells[cells.Length - 1].y)
            {
                return enclosure;
            }
        }
        return null;
    }

    public ConstructionScript GetConstructionByCell(Vector2 cell)
    {
        foreach (EnclosureScript enclosure in enclosuresList)
        {
            Vector2[] cells = enclosure.cells;
            if (cell.x >= cells[0].x && cell.x <= cells[cells.Length - 1].x
                && cell.y >= cells[0].y && cell.y <= cells[cells.Length - 1].y)
            {
                return enclosure;
            }
        }
        foreach (BuildingScript building in buildingsList)
        {
            Vector2[] cells = building.cells;
            if (cell.x >= cells[0].x && cell.x <= cells[cells.Length - 1].x
                && cell.y >= cells[0].y && cell.y <= cells[cells.Length - 1].y)
            {
                return building;
            }
        }
        return null;
    }

    public BuildingScript GetAvailableBuilding(BuildingScript.BuildingType buildingType)
    {
        foreach (BuildingScript building in buildingsList)
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
        else if (gameManagerScript.isInIsland && gameManagerScript.islandScript == this)
        {
            gameManagerScript.shipScript.AddMaterial(materialType);
        }
        gameManagerScript.UpdateMaterial(materialType);
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

        if(cropAmount < originalAmount && gameManagerScript.isInIsland && gameManagerScript.islandScript == this)
        {
            gameManagerScript.shipScript.AddCrops(cropType, originalAmount - cropAmount);
        }
        gameManagerScript.UpdateCrop(cropType);
    }

    public int GetCropAmount(ResourceScript.CropType cropType)
    {
        int seedAmount = crops[(int)cropType];
        if (gameManagerScript.isInIsland) seedAmount += gameManagerScript.shipScript.crops[(int)cropType];
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
        else if (gameManagerScript.isInIsland && gameManagerScript.shipScript.crops[(int)cropType] > 0)
        {
            gameManagerScript.shipScript.crops[(int)cropType]--;
            gameManagerScript.shipScript.usage--;
            return true;
        }
        return false;
    }
}
