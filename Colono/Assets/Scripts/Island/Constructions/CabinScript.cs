using System;
using System.Collections;
using UnityEngine;

public class CabinScript : BuildingScript
{
    public override void EditConstruction()
    {
        throw new NotImplementedException();
    }

    public void RestPeasant(PeasantScript peasantScript)
    {
        StartCoroutine(RestPeasantCoroutine(peasantScript));
    }

    private IEnumerator RestPeasantCoroutine(PeasantScript peasantScript)
    {
        peasantScript.gameObject.SetActive(false);
        yield return new WaitForSeconds(10);
        peasantScript.gameObject.SetActive(true);

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