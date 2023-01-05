using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenDoorScript : MonoBehaviour
{
    public PenScript penScript;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "NPC")
        {
            penScript.npcHasEntered = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "NPC")
        {
            penScript.npcIsInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "NPC")
        {
            penScript.npcIsInside = false;
            penScript.npcHasExited = true;
        }
    }
}
