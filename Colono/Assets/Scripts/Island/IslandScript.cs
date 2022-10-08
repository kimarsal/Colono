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
    public GameObject cells;
    public GameObject constructions;

    private List<GameObject> enclosuresList = new List<GameObject>();
    private List<GameObject> buildingsList = new List<GameObject>();

    private Dictionary<Vector2, GameObject> itemsList = new Dictionary<Vector2, GameObject>();

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

    public void AddBuilding(GameObject building)
    {
        buildingsList.Add(building);
        RebakeNavMesh();
        /*if(building.GetComponent<BuildingScript>().type == BuildingScript.BuildingType.Residence)
        {
            npcManager.GeneratePeasants(building.GetComponent<BuildingScript>());
        }*/
    }

    public void RemoveBuilding(GameObject building)
    {
        buildingsList.Remove(building);
        Destroy(building);
        StartCoroutine(RebakeNavMeshDelayed());
    }

    public void AddEnclosure(GameObject enclosure)
    {
        enclosuresList.Add(enclosure);
        RebakeNavMesh();
    }

    public void RemoveEnclosure(GameObject enclosure)
    {
        enclosuresList.Remove(enclosure);
        Destroy(enclosure);
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

    public GameObject GetEnclosureByCell(Vector2 cell)
    {
        foreach(GameObject enclosure in enclosuresList)
        {
            Vector2[] cells = enclosure.GetComponent<EnclosureScript>().cells;
            if(cell.x >= cells[0].x && cell.x <= cells[cells.Length - 1].x
                && cell.y >= cells[0].y && cell.y <= cells[cells.Length - 1].y)
            {
                return enclosure;
            }
        }
        return null;
    }

    public GameObject GetConstructionByCell(Vector2 cell, out bool isBuilding)
    {
        isBuilding = false;
        foreach (GameObject enclosure in enclosuresList)
        {
            Vector2[] cells = enclosure.GetComponent<EnclosureScript>().cells;
            if (cell.x >= cells[0].x && cell.x <= cells[cells.Length - 1].x
                && cell.y >= cells[0].y && cell.y <= cells[cells.Length - 1].y)
            {
                return enclosure;
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
