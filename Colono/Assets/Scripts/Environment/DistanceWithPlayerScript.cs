using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceWithPlayerScript : MonoBehaviour
{
    private IslandScript islandScript;

    private void Start()
    {
        islandScript = transform.parent.gameObject.GetComponent<IslandScript>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            islandScript.PlayerIsNear();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            islandScript.PlayerLeft();
        }
    }
}
