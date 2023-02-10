using UnityEngine;

public abstract class BuildingScript : ConstructionScript
{
    public enum BuildingType { Warehouse, Cabin, Tavern, Mine, Alchemist};
    public BuildingType buildingType;
    public Vector3 position;
    public int orientation;

    public override void FinishUpBusiness()
    {
        return;
    }

}