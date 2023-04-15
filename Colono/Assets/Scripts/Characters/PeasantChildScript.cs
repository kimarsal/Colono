using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeasantChildScript : PeasantScript
{

    public override void UpdateTask()
    {
        StopCharacter();
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

    protected override void ArrivedAtDestination()
    {
        if (tavern != null) //Si ha anat a menjar
        {
            tavern.PeasantHasArrived(this);
            peasantRowScript?.PeasantArrivedToBuilding();
        }
        else if (cabin != null) //Si ha anat a dormir
        {
            cabin.PeasantHasArrived(this);
            peasantRowScript?.PeasantArrivedToBuilding();
        }
        else if (constructionScript != null) //Si t� el vaixell com a dest�
        {
            constructionScript.PeasantHasArrived(this);
            peasantRowScript?.PeasantArrivedToBuilding();
        }
        else
        {
            StartCoroutine(WaitForNextRandomDestination());
        }
    }
}
