using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeasantChildScript : PeasantScript
{
    public override void UpdateTask()
    {
        if (constructionScript != null) //Si té una construcció com a destí
        {
            SetDestination(constructionScript.entry.position);
        }
        else SetDestination(NPCManager.GetRandomPoint(transform.position));
    }
    public override void ArrivedAtDestination()
    {
        if (constructionScript != null) //Si té una construcció com a destí
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
