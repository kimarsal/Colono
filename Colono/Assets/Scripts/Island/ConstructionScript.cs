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
    public Vector2[] cells;
    public int length;
    public int width;
    public Transform center;

    public List<PeasantScript> peasantList = new List<PeasantScript>();
    public int peasantsOnTheirWay;

    public ConstructionDetailsScript constructionDetailsScript;

    public abstract TaskScript GetNextPendingTask();

    public void UpdateConstructionDetails()
    {
        constructionDetailsScript?.UpdatePeasantNum();
    }
}
