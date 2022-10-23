using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarnScript : EnclosureScript
{
    public GameObject animals;
    public GameObject openGate;
    public GameObject closedGate;

    public bool npcIsInside = false;
    public bool npcHasEntered = false;
    public bool npcHasExited = false;

    private void Start()
    {
        openGate.GetComponent<BarnDoorScript>().barnScript = this;
        closedGate.GetComponent<BarnDoorScript>().barnScript = this;
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
