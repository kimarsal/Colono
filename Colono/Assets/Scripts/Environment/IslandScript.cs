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
    private List<GameObject> zones;

    private void Start()
    {
        gameManagerScript = GameObject.Find("GameManager").GetComponent<GameManager>();
        convexColliders = transform.GetChild(0).gameObject;
        zones = new List<GameObject>();
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
        zones.Add(zone);
    }

    public void RemoveZone(GameObject zone)
    {
        zones.Remove(zone);
    }

}
