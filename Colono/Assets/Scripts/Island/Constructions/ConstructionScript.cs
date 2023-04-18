using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

[JsonObject(MemberSerialization.OptIn)]
public abstract class ConstructionScript : MonoBehaviour
{
    public enum ConstructionType { Ship, Enclosure, Building }
    [JsonProperty] public ConstructionType constructionType;
    [JsonProperty] public Vector2[] cells;
    [JsonProperty] public int length;
    [JsonProperty] public int width;

    [JsonProperty] public int level = 1;
    [JsonProperty] public int maxPeasants;
    [JsonProperty] public List<PeasantScript> peasantList = new List<PeasantScript>();
    [JsonProperty] public int peasantsInside;

    public IslandScript islandScript;
    public ConstructionDetailsScript constructionDetailsScript;
    public Transform entry;
    public Outline outline;

    public virtual string title { get { return constructionType.ToString(); } }
    public virtual bool canManagePeasants { get { return true; } }
    public virtual bool canBeRemoved { get { return true; } }
    public virtual int peasantCount { get { return peasantList.Count; } }

    public abstract EditorScript editorScript { get; }

    public virtual void AddPeasant(PeasantScript peasantScript)
    {
        peasantScript.constructionScript = this;
        peasantList.Add(peasantScript);
        UpdateConstructionDetails();
    }

    public virtual PeasantScript RemovePeasant()
    {
        PeasantScript peasantScript = peasantList[0];
        peasantList.RemoveAt(0);

        if (peasantScript.isInBuilding)
        {
            peasantsInside--;
            Debug.Log("Peasant " + islandScript.peasantList.IndexOf(peasantScript) + " was inside and was removed. Number of peasants inside: " + peasantsInside);

            peasantScript.transform.parent = islandScript.npcsTransform;
            peasantScript.navMeshAgent.Warp(entry.position);
            peasantScript.isInBuilding = false;
            peasantScript.isInConstruction = false;
        }

        return peasantScript;
    }

    public virtual PeasantScript PeasantHasArrived(PeasantScript peasantScript)
    {
        peasantsInside++;
        Debug.Log("Peasant " + islandScript.peasantList.IndexOf(peasantScript) + " has arrived. Number of peasants inside: " + peasantsInside);
        UpdateConstructionDetails();
        return peasantScript;
    }

    public virtual void SendAllPeasantsBack()
    {
        while(peasantList.Count > 0)
        {
            PeasantScript peasantScript = RemovePeasant();
            islandScript.peasantList.Add(peasantScript);
            if (peasantScript.peasantType == PeasantScript.PeasantType.Adult)
            {
                PeasantAdultScript peasantAdultScript = (PeasantAdultScript)peasantScript;
                peasantAdultScript.taskSourceInterface.GetNextPendingTask(peasantAdultScript);
            }
            else peasantScript.UpdateTask();
        }
    }

    public abstract void FinishUpBusiness();

    public void UpdateConstructionDetails()
    {
        constructionDetailsScript?.UpdatePeasantNum();
    }
}