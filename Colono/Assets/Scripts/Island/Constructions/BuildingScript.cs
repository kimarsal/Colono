
using static BuildingScript;

public abstract class BuildingScript : ConstructionScript
{
    public enum BuildingType { Warehouse, Cabin, Tavern, Mine, Alchemist};
    public BuildingType buildingType;
    public int orientation;

    public override bool GetNextPendingTask(PeasantAdultScript peasantAdultScript)
    {
        return false;
    }

    public override PeasantScript RemovePeasant()
    {
        PeasantScript peasantScript = base.RemovePeasant();
        peasantScript.gameObject.SetActive(true);
        return peasantScript;
    }

    public override void FinishUpBusiness()
    {
        return;
    }

    public BuildingInfo GetBuildingInfo()
    {
        BuildingInfo buildingInfo;
        if(buildingType == BuildingType.Tavern)
        {
            buildingInfo = ((TavernScript)this).GetTavernInfo();
        }
        else
        {
            buildingInfo = new BuildingInfo();
        }
        buildingInfo.buildingType = buildingType;
        buildingInfo.position = new SerializableVector3(transform.position);
        buildingInfo.orientation = orientation;
        return buildingInfo;
    }
}

[System.Serializable]
public class BuildingInfo : ConstructionInfo
{
    public BuildingType buildingType;
    public SerializableVector3 position;
    public int orientation;
}