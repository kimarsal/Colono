using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeasantChildScript : PeasantScript
{
    public override void UpdateTask()
    {
        if (constructionScript != null) //Si t� una construcci� com a dest�
        {
            SetDestination(constructionScript.entry.position);
        }
        else SetDestination(NPCManager.GetRandomPoint(transform.position));
    }
    public override void ArrivedAtDestination()
    {
        if (constructionScript != null) //Si t� una construcci� com a dest�
        {
            if(constructionScript.constructionType == ConstructionScript.ConstructionType.Ship)
            {
                transform.parent = ((ShipScript)constructionScript).npcs.transform;
            }
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
