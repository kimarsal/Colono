using System.Collections.Generic;
using UnityEngine;

public abstract class ConstructionScript : TaskSourceScript
{
    public enum ConstructionType { Ship, Enclosure, Building }
    public ConstructionType constructionType;

    public IslandScript islandScript;
    public virtual IslandEditor islandEditor { get { return islandScript.islandEditor; } }
    public Vector2[] cells;
    public int length;
    public int width;
    public Transform entry;
    public Outline outline;

    public int maxPeasants;
    public List<PeasantScript> peasantList = new List<PeasantScript>();
    public int peasantsOnTheirWay;

    public ConstructionDetailsScript constructionDetailsScript;

    public abstract void EditConstruction();

    public virtual void AddPeasant(PeasantScript peasantScript)
    {
        peasantScript.constructionScript = this;
        peasantList.Add(peasantScript);
        peasantsOnTheirWay++;
    }

    public virtual PeasantScript RemovePeasant()
    {
        PeasantScript peasantScript = peasantList[0];
        peasantList.Remove(peasantScript);
        if (!peasantScript.gameObject.activeInHierarchy) //Ja havia arribat a l'edifici
        {
            peasantScript.gameObject.SetActive(true);
        }
        else
        {
            peasantsOnTheirWay--;
        }

        return peasantScript;
    }

    public abstract void FinishUpBusiness();

    public void UpdateConstructionDetails()
    {
        constructionDetailsScript?.UpdatePeasantNum();
    }

    public virtual ConstructionInfo GetConstructionInfo()
    {
        ConstructionInfo constructionInfo = null;
        if(constructionType == ConstructionType.Enclosure)
        {
            constructionInfo = ((EnclosureScript)this).GetEnclosureInfo();
        }
        else
        {
            constructionInfo = ((BuildingScript)this).GetBuildingInfo();
        }
        constructionInfo.constructionType = constructionType;
        foreach(PeasantScript peasantScript in peasantList)
        {
            constructionInfo.peasantList.Add(peasantScript.GetPeasantInfo());
        }
        constructionInfo.cells = SerializableVector2.GetSerializableArray(cells);
        constructionInfo.length = length;
        constructionInfo.width = width;
        return constructionInfo;
    }
}

[System.Serializable]
public class ConstructionInfo
{
    public ConstructionScript.ConstructionType constructionType;
    public List<PeasantInfo> peasantList = new List<PeasantInfo>();

    public SerializableVector2[] cells;
    public int length;
    public int width;

}