using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeasantChildScript : PeasantScript
{
    public override void UpdateTask()
    {
        if (tavern != null) //Si ha d'anar a menjar
        {
            SetDestination(tavern.entry.position);
        }
        else if (cabin != null) //Si ha d'anar a dormir
        {
            SetDestination(cabin.entry.position);
        }
        else
        {
            if (constructionScript != null) //Si t� el vaixell com a dest�
            {
                SetDestination(constructionScript.entry.position);
            }
            else //Si no t� dest�
            {
                SetDestination(NPCManager.GetRandomPoint(transform.position));
            }
        }
    }

    public override void ArrivedAtDestination()
    {
        if (tavern != null) //Si ha anat a menjar
        {
            tavern.FeedPeasant(this);
        }
        else if (cabin != null) //Si ha anat a dormir
        {
            cabin.RestPeasant(this);
        }
        else if (constructionScript != null) //Si t� el vaixell com a dest�
        {
            transform.parent = ((ShipScript)constructionScript).npcs.transform;
            constructionScript.peasantsOnTheirWay--;
            constructionScript.UpdateConstructionDetails();
            gameObject.SetActive(false);
        }
        else
        {
            StartCoroutine(WaitForNextRandomDestination());
        }
    }
}
