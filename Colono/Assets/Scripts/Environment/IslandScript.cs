using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class IslandScript : MonoBehaviour
{
    public enum ItemType { Tree, Bush, Rock, Flower, Miscellaneous};
    public bool hasBeenDiscovered = false;
    public GameManager gameManagerScript;
    public string islandName;
    public Material selectedMaterial;
    public IslandCellScript islandCellScript;

    private GameObject convexColliders;
    public GameObject trees;
    public GameObject bushes;
    public GameObject rocks;
    public GameObject flowers;
    public GameObject miscellaneous;

    public GameObject cells;
    public GameObject buildings;
    public GameObject zones;

    private List<GameObject> treesList = new List<GameObject>();
    private List<GameObject> bushesList = new List<GameObject>();
    private List<GameObject> rocksList = new List<GameObject>();
    private List<GameObject> flowersList = new List<GameObject>();
    private List<GameObject> miscellaneousList = new List<GameObject>();

    private List<GameObject> zonesList = new List<GameObject>();
    private List<GameObject> buildingsList = new List<GameObject>();

    private Dictionary<Vector2, GameObject> items = new Dictionary<Vector2, GameObject>();

    private void Start()
    {
        gameManagerScript = GameObject.Find("GameManager").GetComponent<GameManager>();
        convexColliders = transform.GetChild(0).gameObject;

        cells = new GameObject("Cells");
        cells.transform.parent = gameObject.transform;
        cells.transform.localPosition = Vector3.zero;

        buildings = new GameObject("Buildings");
        buildings.transform.parent = gameObject.transform;
        buildings.transform.localPosition = Vector3.zero;

        zones = new GameObject("Zones");
        zones.transform.parent = gameObject.transform;
        zones.transform.localPosition = Vector3.zero;
    }

    public void PlayerIsNear()
    {
        gameManagerScript.PlayerIsNearIsland(transform.position, this);
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
        //return !takenCells.Contains(cell);
        return items.ContainsKey(cell);
    }

    public void AddItem(GameObject item, ItemType type, Vector2 cell)
    {
        /*switch (type)
        {
            case ItemType.Tree: treesList.Add(item); break;
            case ItemType.Rock: rocksList.Add(item); break;
            case ItemType.Bush: bushesList.Add(item); break;
            case ItemType.Flower: flowersList.Add(item); break;
            case ItemType.Miscellaneous: miscellaneousList.Add(item); break;
        }
        takenCells.Add(cell);*/
        items.Add(cell, item);
    }

    public void ClearArea(Vector2[] cells)
    {
        foreach(Vector2 cell in cells)
        {
            if (isCellTaken(cell))
            {
                RemoveItemAtCell(cell);
            }
        }
    }

    public void RemoveItemAtCell(Vector2 cell)
    {
        /*switch (type)
        {
            case ItemType.Tree: treesList.Remove(item); break;
            case ItemType.Rock: rocksList.Remove(item); break;
            case ItemType.Bush: bushesList.Remove(item); break;
            case ItemType.Flower: flowersList.Remove(item); break;
            case ItemType.Miscellaneous: miscellaneousList.Remove(item); break;
        }
        takenCells.Remove(cell);*/
        GameObject item = items[cell];
        items.Remove(cell);
        Destroy(item);
    }

    public void RemoveItem(GameObject item, ItemType type, Vector2 cell)
    {
        /*switch (type)
        {
            case ItemType.Tree: treesList.Remove(item); break;
            case ItemType.Rock: rocksList.Remove(item); break;
            case ItemType.Bush: bushesList.Remove(item); break;
            case ItemType.Flower: flowersList.Remove(item); break;
            case ItemType.Miscellaneous: miscellaneousList.Remove(item); break;
        }
        takenCells.Remove(cell);*/
        items.Remove(cell);
        Destroy(item);
    }

    public void AddBuilding(GameObject building)
    {
        buildingsList.Add(building);
    }

    public void RemoveBuilding(GameObject building)
    {
        buildingsList.Remove(building);
    }

    public void AddZone(GameObject zone)
    {
        zonesList.Add(zone);
    }

    public void RemoveZone(GameObject zone)
    {
        zonesList.Remove(zone);
    }

    public GameObject GetZoneByCell(Vector2 cell)
    {
        foreach(GameObject zone in zonesList)
        {
            Vector2[] cells = zone.GetComponent<ZoneScript>().cells;
            if(cell.x >= cells[0].x && cell.x <= cells[cells.Length - 1].x
                && cell.y >= cells[0].y && cell.y <= cells[cells.Length - 1].y)
            {
                return zone;
            }
        }
        return null;
    }

    public GameObject GetConstructionByCell(Vector2 cell, out bool isBuilding)
    {
        isBuilding = false;
        foreach (GameObject zone in zonesList)
        {
            Vector2[] cells = zone.GetComponent<ZoneScript>().cells;
            if (cell.x >= cells[0].x && cell.x <= cells[cells.Length - 1].x
                && cell.y >= cells[0].y && cell.y <= cells[cells.Length - 1].y)
            {
                return zone;
            }
        }
        isBuilding = true;
        foreach (GameObject building in buildingsList)
        {
            Vector2[] cells = building.GetComponent<BuildingScript>().cells;
            if (cell.x >= cells[0].x && cell.x <= cells[cells.Length - 1].x
                && cell.y >= cells[0].y && cell.y <= cells[cells.Length - 1].y)
            {
                return building;
            }
        }
        return null;
    }

}
