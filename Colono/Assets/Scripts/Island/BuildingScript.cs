using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingScript : ConstructionScript
{
    public enum BuildingType { Warehouse, Residence, Mine};
    public BuildingType buildingType;

    public int orientation;

    public void EnableCollider()
    {
        //GetComponent<BoxCollider>().enabled = true;
    }

    public override TaskScript GetNextPendingTask()
    {
        return null;
    }
}
