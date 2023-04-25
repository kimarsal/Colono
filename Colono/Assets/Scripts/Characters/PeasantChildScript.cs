using Newtonsoft.Json;
using UnityEngine;

[JsonObject(MemberSerialization.OptIn)]
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
            if (constructionScript != null) //Si té el vaixell com a destí
            {
                SetDestination(constructionScript.entry.position);
            }
            else //Si no té destí
            {
                SetDestination(NPCManager.GetRandomPoint(transform.position));
            }
        }
    }

    protected override void ArrivedAtDestination()
    {
        if (age > 18)
        {
            AgeUpPeasant();
            return;
        }

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
        else if (constructionScript != null) //Si té el vaixell com a destí
        {
            constructionScript.PeasantHasArrived(this);
            peasantRowScript?.PeasantArrivedToBuilding(true);
        }
        else
        {
            StartCoroutine(WaitForNextRandomDestination());
        }
    }

    private void AgeUpPeasant()
    {
        Vector3 pos = transform.position;
        if (tavern != null)
        {
            if (isInBuilding)
            {
                tavern.peasantsInside--;
                pos = tavern.entry.position;
            }
            tavern.peasantList.Remove(this);
        }
        if (cabin != null)
        {
            if (tavern == null && isInBuilding)
            {
                cabin.peasantsInside--;
                pos = cabin.entry.position;
            }
            cabin.peasantList.Remove(this);
        }
        if (constructionScript != null) //Estava de camí al vaixell
        {
            constructionScript.peasantList.Remove(this);
        }
        islandScript.peasantList.Remove(this);

        PeasantAdultScript peasantAdultScript = Instantiate(ResourceScript.Instance.GetPeasantPrefab(PeasantType.Adult, peasantGender),
            pos, transform.rotation, transform.parent).GetComponent<PeasantAdultScript>();

        peasantAdultScript.islandScript = islandScript;
        peasantAdultScript.isNative = false;
        peasantAdultScript.headType = Random.Range(0, 2);
        peasantAdultScript._SKINCOLOR = _SKINCOLOR;
        peasantAdultScript._HAIRCOLOR = _HAIRCOLOR;
        peasantAdultScript._CLOTH3COLOR = _CLOTH3COLOR;
        peasantAdultScript._CLOTH4COLOR = _CLOTH4COLOR;
        peasantAdultScript._OTHERCOLOR = _OTHERCOLOR;

        peasantRowScript?.SetPeasant(peasantAdultScript);

        islandScript.peasantList.Add(peasantAdultScript);
        if (constructionScript != null)
        {
            constructionScript.peasantList.Add(peasantAdultScript);
            peasantAdultScript.UpdateTask();
        }
        else islandScript.GetNextPendingTask(peasantAdultScript);

        Destroy(gameObject);
    }
}
