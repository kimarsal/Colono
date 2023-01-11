using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class ConstructionScript : MonoBehaviour
{
    public enum ConstructionType { Ship, Enclosure, Building }
    public ConstructionType constructionType;

    public IslandScript islandScript;
    public IslandEditor islandEditor;
    public Vector2[] cells;
    public int length;
    public int width;
    public Transform entry;
    public Outline outline;

    public int maxPeasants;
    public List<PeasantScript> peasantList = new List<PeasantScript>();
    public int peasantsOnTheirWay;

    public ConstructionDetailsScript constructionDetailsScript;

    public abstract TaskScript GetNextPendingTask();

    public virtual void FinishUpBusiness()
    {
        foreach (PeasantScript peasantScript in peasantList)
        {
            if(peasantScript.peasantType == PeasantScript.PeasantType.Adult)
            {
                ((PeasantAdultScript)peasantScript).CancelTask();
            }
        }
    }

    public void UpdateConstructionDetails()
    {
        constructionDetailsScript?.UpdatePeasantNum();
    }
}

[System.Serializable]
public class ConstructionInfo
{
    public int constructionType;
    public int constructionIndex;
}