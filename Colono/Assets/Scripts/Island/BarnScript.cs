using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarnScript : EnclosureScript
{
    public GameObject animals;
    public GameObject openGate;
    public GameObject closedGate;

    public void ToggleGate()
    {
        openGate.SetActive(!openGate.activeInHierarchy);
        closedGate.SetActive(!closedGate.activeInHierarchy);
    }
}
