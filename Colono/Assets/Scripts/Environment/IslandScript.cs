using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
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
    public GameObject constructions;

    private int peasantNum = 10;

    /*private List<GameObject> treesList = new List<GameObject>();
    private List<GameObject> bushesList = new List<GameObject>();
    private List<GameObject> rocksList = new List<GameObject>();
    private List<GameObject> flowersList = new List<GameObject>();
    private List<GameObject> miscellaneousList = new List<GameObject>();*/

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

        constructions = new GameObject("Constructions");
        constructions.transform.parent = gameObject.transform;
        constructions.transform.localPosition = Vector3.zero;
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
        RebakeNavMesh();
    }

    public void RemoveBuilding(GameObject building)
    {
        buildingsList.Remove(building);
        Destroy(building);
        RebakeNavMesh();
    }

    public void AddZone(GameObject zone)
    {
        zonesList.Add(zone);
        RebakeNavMesh();
    }

    public void RemoveZone(GameObject zone)
    {
        zonesList.Remove(zone);
        Destroy(zone);
        RebakeNavMesh();
    }

    public void RebakeNavMesh()
    {
        GetComponent<NavMeshSurface>().UpdateNavMesh(GetComponent<NavMeshSurface>().navMeshData);

        /*NavMeshBuildSource s = new NavMeshBuildSource();
        s.shape = NavMeshBuildSourceShape.Mesh;
        s.sourceObject = GetComponent<MeshFilter>().sharedMesh;
        s.transform = transform.localToWorldMatrix;
        s.size = Vector3.one;

        List<NavMeshBuildSource> list = new List<NavMeshBuildSource>();
        list.Add(s);

        NavMeshBuilder.UpdateNavMeshDataAsync(
            GetComponent<NavMeshSurface>().navMeshData,
            GetComponent<NavMeshSurface>().GetBuildSettings(),
            list,
            new Bounds(bounds.center, bounds.size + Vector3.one * 2));*/

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

    public int GetAvailablePeasants()
    {
        return peasantNum;
    }

    public void SendPeasantToArea(ConstructionScript constructionScript, bool adding)
    {
        peasantNum -= adding ? 1 : -1;
        constructionScript.peasantNum += adding ? 1 : -1;
    }

    public void SendPeasantsBack(ConstructionScript constructionScript)
    {
        peasantNum += constructionScript.peasantNum;
    }

}
