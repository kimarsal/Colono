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
    [JsonIgnore] public virtual IslandEditor islandEditor { get { return islandScript.islandEditor; } }
    [JsonIgnore] public Transform entry;
    [JsonIgnore] public Outline outline;

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
        peasantList.RemoveAt(0);
        if (!peasantScript.gameObject.activeInHierarchy)
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
}