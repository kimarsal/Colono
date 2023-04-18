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
        if (age > 18)
        {
            AgeUpPeasant();
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
        else if (constructionScript != null) //Si t� el vaixell com a dest�
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
        if (constructionScript != null) //Estava de cam� al vaixell
        {
            constructionScript.peasantList.Remove(this);
        }
        if (tavern != null)
        {
            tavern.peasantList.Remove(this);
            if (isInBuilding)
            {
                tavern.peasantsInside--;
                Debug.Log("Peasant " + islandScript.peasantList.IndexOf(this) + " aged up inside a tavern. Number of peasants inside: " + tavern.peasantsInside);
                pos = tavern.entry.position;
            }
        }
        if (cabin != null)
        {
            cabin.peasantList.Remove(this);
            if (isInBuilding && tavern == null)
            {
                cabin.peasantsInside--;
                Debug.Log("Peasant " + islandScript.peasantList.IndexOf(this) + " aged up inside a cabin. Number of peasants inside: " + cabin.peasantsInside);
                pos = cabin.entry.position;
            }
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
