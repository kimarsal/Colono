using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingScript : ConstructionScript
{
    public enum BuildingType { Warehouse, Cabin, Tavern, Alchemist, Mine};
    public BuildingType buildingType;

    public int orientation;

    public override TaskScript GetNextPendingTask()
    {
        return null;
    }

}
