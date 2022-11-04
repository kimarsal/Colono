using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TavernScript : BuildingScript
{
    public List<TavernEditor.Recipe> recipes = new List<TavernEditor.Recipe>();

    private void Start()
    {
        recipes.Add(new TavernEditor.Recipe());
    }

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
        if(peasantScript.peasantType == PeasantScript.PeasantType.Adult && peasantScript.constructionScript != null && peasantScript.exhaustion < 1)
        {
            PeasantAdultScript peasantAdultScript = (PeasantAdultScript)peasantScript;
            if (peasantAdultScript.task == null)
            {
                peasantAdultScript.task = peasantAdultScript.constructionScript.GetNextPendingTask();
                peasantAdultScript.task.peasantScript = peasantAdultScript;
            }
        }
        peasantScript.UpdateTask();
    }
}
