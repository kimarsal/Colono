using Newtonsoft.Json;
using UnityEngine;

public abstract class BuildingScript : ConstructionScript
{
    public enum BuildingType { Warehouse, Cabin, Tavern, Mine, Alchemist};
    public BuildingType buildingType;
    public Vector3 position;
    public int orientation;

    [JsonIgnore] public override string title { get { return buildingType.ToString(); } }
    [JsonIgnore] public override bool canManagePeasants { get { return false; } }

    public override PeasantScript PeasantHasArrived(PeasantScript peasantScript)
    {
        base.PeasantHasArrived(peasantScript);

        peasantScript.transform.parent = GameManager.Instance.buildingInterior.transform;
        peasantScript.navMeshAgent.Warp(GameManager.Instance.buildingInterior.transform.position);
        peasantScript.isInBuilding = true;
        return peasantScript;
    }

    public override void FinishUpBusiness()
    {
        return;
    }

}