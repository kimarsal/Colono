using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TavernScript : BuildingScript
{
    public void FeedPeasant(PeasantScript peasantScript)
    {
        StartCoroutine(FeedPeasantCoroutine(peasantScript));
    }

    private IEnumerator FeedPeasantCoroutine(PeasantScript peasantScript)
    {
        peasantScript.gameObject.SetActive(false);
        yield return new WaitForSeconds(5);
        peasantScript.gameObject.SetActive(true);
        peasantScript.hunger = 0;
        peasantScript.tavern = null;
        peasantScript.UpdateTask();
    }
}
