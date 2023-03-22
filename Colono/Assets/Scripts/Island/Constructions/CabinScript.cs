using System;
using System.Collections;
using UnityEngine;

public class CabinScript : BuildingScript
{
    public override EditorScript editorScript { get { return null; } }

    public override void AddPeasant(PeasantScript peasantScript)
    {
        peasantList.Add(peasantScript);
        peasantsOnTheirWay++;
        UpdateConstructionDetails();
    }

    public override PeasantScript PeasantHasArrived(PeasantScript peasantScript)
    {
        PeasantScript newPeasantScript = base.PeasantHasArrived(peasantScript);
        StartCoroutine(RestPeasantCoroutine(newPeasantScript));
        return null;
    }

    private IEnumerator RestPeasantCoroutine(PeasantScript peasantScript)
    {
        yield return new WaitForSeconds(5);

        peasantScript.transform.parent = islandScript.npcsTransform;
        peasantScript.navMeshAgent.Warp(entry.position);
        peasantScript.exhaustion = 0;
        peasantScript.cabin = null;

        if (peasantScript.peasantType == PeasantScript.PeasantType.Adult)
        {
            PeasantAdultScript peasantAdultScript = (PeasantAdultScript)peasantScript;
            peasantAdultScript.taskSourceInterface.GetNextPendingTask(peasantAdultScript);
        }
        else peasantScript.UpdateTask();
    }

    public override PeasantScript RemovePeasant()
    {
        PeasantScript peasantScript = base.RemovePeasant();
        peasantScript.cabin = null;
        return peasantScript;
    }
}