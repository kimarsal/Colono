using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CabinScript : BuildingScript
{
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
        peasantScript.UpdateTask();
    }
}
