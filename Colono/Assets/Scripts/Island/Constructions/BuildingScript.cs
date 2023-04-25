using Newtonsoft.Json;
using UnityEngine;

public abstract class BuildingScript : ConstructionScript
{
    public enum BuildingType { Warehouse, Cabin, Tavern, Mine, Alchemist};
    [JsonProperty] public BuildingType buildingType;
    [JsonProperty] [JsonConverter(typeof(VectorConverter))] public Vector3 position;
    [JsonProperty] public int orientation;
    public int requiredWood;
    public int requiredStone;

    public override string title { get { return buildingType.ToString(); } }
    public override bool canManagePeasants { get { return false; } }

    public virtual void InitializeBuilding(BuildingScript buildingInfo)
    {
        transform.position = buildingInfo.position;
        transform.rotation = Quaternion.Euler(0, 90 * buildingInfo.orientation, 0);
        transform.parent = islandScript.constructionsTransform.transform;
        cells = buildingInfo.cells;
        orientation = buildingInfo.orientation;
    }

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