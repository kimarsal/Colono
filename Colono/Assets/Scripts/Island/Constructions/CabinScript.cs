using System;
using System.Collections;
using UnityEngine;

public class CabinScript : BuildingScript
{
    public override EditorScript editorScript { get { return null; } }

    public override PeasantScript PeasantHasArrived(PeasantScript peasantScript)
    {
        PeasantScript newPeasantScript = base.PeasantHasArrived(peasantScript);
        StartCoroutine(RestPeasantCoroutine(newPeasantScript));
        return null;
    }

    private IEnumerator RestPeasantCoroutine(PeasantScript peasantScript)
    {
        yield return new WaitForSeconds(5);

        PeasantScript newPeasantScript = Instantiate(peasantScript.gameObject,
                entry.position, Quaternion.identity,
                islandScript.npcsTransform).GetComponent<PeasantScript>();
        newPeasantScript.InitializePeasant(peasantScript);
        Destroy(peasantScript.gameObject);

        newPeasantScript.exhaustion = 0;
        newPeasantScript.cabin = null;
        if (newPeasantScript.peasantType == PeasantScript.PeasantType.Adult)
        {
            PeasantAdultScript peasantAdultScript = (PeasantAdultScript)newPeasantScript;
            peasantAdultScript.taskSourceInterface.GetNextPendingTask(peasantAdultScript);
        }
        else newPeasantScript.UpdateTask();
    }

    public override PeasantScript RemovePeasant()
    {
        PeasantScript peasantScript = base.RemovePeasant();
        peasantScript.cabin = null;
        return peasantScript;
    }
}