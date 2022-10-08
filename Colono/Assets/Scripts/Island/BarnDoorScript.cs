using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarnDoorScript : MonoBehaviour
{
    public BarnScript barnScript;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "NPC")
        {
            barnScript.npcHasEntered = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "NPC")
        {
            barnScript.npcIsInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "NPC")
        {
            barnScript.npcIsInside = false;
            barnScript.npcHasExited = true;
        }
    }
}
