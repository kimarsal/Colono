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

        PeasantScript newPeasantScript = Instantiate(peasantScript.gameObject,
            islandScript.gameManager.buildingInterior.transform.position, Quaternion.identity,
            islandScript.gameManager.buildingInterior.transform).GetComponent<PeasantScript>();
        newPeasantScript.isInBuilding = true;
        //newPeasantScript.InitializePeasant(peasantScript);
        Destroy(peasantScript.gameObject);
        return newPeasantScript;
    }

    public override PeasantScript RemovePeasant()
    {
        PeasantScript peasantScript = base.RemovePeasant();
        if (peasantScript.isInBuilding)
        {
            PeasantScript newPeasantScript = Instantiate(peasantScript.gameObject,
                entry.position, Quaternion.identity,
                islandScript.npcsTransform).GetComponent<PeasantScript>();
            //newPeasantScript.InitializePeasant(peasantScript);
            newPeasantScript.isInBuilding = false;
            Destroy(peasantScript.gameObject);
            return newPeasantScript;
        }
        return peasantScript;
    }

    public override void FinishUpBusiness()
    {
        return;
    }

}