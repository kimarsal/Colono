using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class EnclosureScript : ConstructionScript
{
    public enum EnclosureType { Garden, Barn, Training };
    public EnclosureType enclosureType;

    public Vector3 minPos;
    public Vector3 maxPos;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("NPC"))
        {
            peasantsOnTheirWay--;
            UpdateConstructionDetails();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("NPC"))
        {
            peasantsOnTheirWay++;
            UpdateConstructionDetails();
        }
    }
}
