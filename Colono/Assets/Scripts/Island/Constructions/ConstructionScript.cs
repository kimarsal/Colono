using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public abstract class ConstructionScript : MonoBehaviour
{
    public enum ConstructionType { Ship, Enclosure, Building }
    public ConstructionType constructionType;
    public Vector2[] cells;
    public int length;
    public int width;

    public int maxPeasants;
    public List<PeasantScript> peasantList = new List<PeasantScript>();
    public int peasantsOnTheirWay;

    [JsonIgnore] public IslandScript islandScript;
    [JsonIgnore] public ConstructionDetailsScript constructionDetailsScript;
    [JsonIgnore] public Transform entry;
    [JsonIgnore] public Outline outline;

    [JsonIgnore] public virtual string title { get { return constructionType.ToString(); } }
    [JsonIgnore] public virtual bool canManagePeasants { get { return true; } }
    [JsonIgnore] public virtual bool canBeRemoved { get { return true; } }
    [JsonIgnore] public virtual int peasantCount { get { return peasantList.Count; } }

    public abstract EditorScript editorScript { get; }

    public virtual void AddPeasant(PeasantScript peasantScript)
    {
        peasantScript.constructionScript = this;
        peasantList.Add(peasantScript);
        peasantsOnTheirWay++;
    }

    public virtual PeasantScript RemovePeasant()
    {
        PeasantScript peasantScript = peasantList[0];
        peasantList.RemoveAt(0);
        if(!peasantScript.isInBuilding) peasantsOnTheirWay--;

        return peasantScript;
    }

    public virtual PeasantScript PeasantHasArrived(PeasantScript peasantScript)
    {
        peasantsOnTheirWay--;
        UpdateConstructionDetails();
        return peasantScript;
    }

    public abstract void FinishUpBusiness();

    public void UpdateConstructionDetails()
    {
        constructionDetailsScript?.UpdatePeasantNum();
    }
}