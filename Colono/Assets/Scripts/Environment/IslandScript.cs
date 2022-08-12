using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class IslandScript : MonoBehaviour
{
    public bool hasBeenDiscovered = false;
    public GameManager gameManagerScript;
    public string islandName;
    public Material selectedMaterial;
    public IslandCellScript islandCellScript;
    private GameObject convexColliders;
    public GameObject cells;
    public GameObject buildings;
    public GameObject zones;
    private List<GameObject> zonesList = new List<GameObject>();

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
            for(int i = 0; i < cells.Length; i++)
            {
                if (cells[i] == cell) return zone;
            }
        }
        return null;
    }

}
