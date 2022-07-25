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

    private void Start()
    {
        gameManagerScript = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void PlayerIsNear()
    {
        gameManagerScript.PlayerIsNearIsland(transform.position, islandName, hasBeenDiscovered);
    }

    public void PlayerLeft()
    {
        gameManagerScript.PlayerLeftIsland();
    }


}
