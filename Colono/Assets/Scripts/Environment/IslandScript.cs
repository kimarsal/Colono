using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandScript : MonoBehaviour
{
    public bool hasBeenDiscovered = false;
    public IslandManager islandManagerScript;
    public string islandName;

    private void Start()
    {
        islandManagerScript = GameObject.Find("GameManager").GetComponent<IslandManager>();
    }

    public void PlayerIsNear()
    {
        islandManagerScript.PlayerIsNearIsland(transform.position, islandName, hasBeenDiscovered);
    }

    public void PlayerLeft()
    {
        islandManagerScript.PlayerLeftIsland();
    }

    
}
