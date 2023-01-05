using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenScript : EnclosureScript
{
    public int[] animals = new int[Enum.GetValues(typeof(ResourceScript.AnimalType)).Length];
    public List<AnimalScript> animalList = new List<AnimalScript>();

    public GameObject openGate;
    public GameObject closedGate;
    public bool npcIsInside = false;
    public bool npcHasEntered = false;
    public bool npcHasExited = false;

    private void Start()
    {
        openGate.GetComponent<PenDoorScript>().penScript = this;
        closedGate.GetComponent<PenDoorScript>().penScript = this;
    }

    void Update()
    {
        if (npcHasEntered)
        {
            openGate.SetActive(true);
            closedGate.SetActive(false);
            npcHasEntered = false;
        }
        else if (npcHasExited)
        {
            if (!npcIsInside)
            {
                openGate.SetActive(false);
                closedGate.SetActive(true);
                npcHasExited = false;
            }
        }
    }

    public override TaskScript GetNextPendingTask()
    {
        return null;
    }

}
