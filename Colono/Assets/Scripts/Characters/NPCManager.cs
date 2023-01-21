using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCManager : MonoBehaviour
{
    public IslandScript islandScript;
    public IslandEditor islandEditor;
    public GameObject npcs;
    public List<PeasantScript> peasantList = new List<PeasantScript>();
    private List<ItemScript> itemsToClear = new List<ItemScript>();
    //public int availableAdultPeasants = 0;

    public void AddItemToClear(ItemScript item)
    {
        itemsToClear.Add(item);
        PeasantAdultScript closestPeasantScript = null;
        float minDistance = 0f;
        for (int i = 0; i < peasantList.Count; i++)
        {
            if (peasantList[i].peasantType == PeasantScript.PeasantType.Adult) //Si és adult
            {
                PeasantAdultScript newPeasantScript = (PeasantAdultScript)peasantList[i];
                if (newPeasantScript.constructionScript == null && newPeasantScript.task == null) //Si no té tasques
                {
                    float distance;
                    if (CheckIfClosestPeasant(closestPeasantScript, minDistance, item.transform.position, newPeasantScript, out distance))
                    {
                        closestPeasantScript = newPeasantScript;
                        minDistance = distance;
                    }
                }
            }
        }
        if (closestPeasantScript != null)
        {
            closestPeasantScript.task = item;
            item.peasantScript = closestPeasantScript;
            closestPeasantScript.UpdateTask();
        }
    }

    public bool CheckIfClosestPeasant(PeasantScript closestPeasantScript, float minDistance, Vector3 destination, PeasantScript newPeasantScript, out float distance)
    {
        NavMeshPath path = new NavMeshPath();
        if (newPeasantScript.GetComponent<NavMeshAgent>().CalculatePath(destination, path)) //Si el camí és possible
        {
            distance = Vector3.Distance(newPeasantScript.transform.position, destination);
            if (closestPeasantScript == null || distance < minDistance) //Si és el més proper
            {
                return true;
            }
        }
        distance = 0;
        return false;
    }

    public void RemoveItemToClear(ItemScript item)
    {
        itemsToClear.Remove(item);
        PeasantAdultScript peasantScript = item.peasantScript;
        item.peasantScript = null;
        if (peasantScript != null) //Tenia un NPC vinculat
        {
            AsignItemToPeasant(peasantScript);
        }
    }

    public void AsignItemToPeasant(PeasantAdultScript peasantScript)
    {
        //peasantScript.CancelTask();
        ItemScript closestItemScript = null;
        float minDistance = 0f;
        foreach (ItemScript itemScript in itemsToClear)
        {
            float distance;
            if (itemScript.peasantScript == null //Si no té un NPC vinculat
                && CheckIfClosestItem(closestItemScript, minDistance, peasantScript.GetComponent<NavMeshAgent>(), itemScript, out distance))
            {
                closestItemScript = itemScript;
                minDistance = distance;
            }
        }
        if (closestItemScript != null)
        {
            closestItemScript.peasantScript = peasantScript;
        }
        peasantScript.task = closestItemScript;
        peasantScript.UpdateTask();
    }

    public bool CheckIfClosestItem(ItemScript closestItemScript, float minDistance, NavMeshAgent navMeshAgent, ItemScript newItemScript, out float distance)
    {
        NavMeshPath path = new NavMeshPath();
        if (navMeshAgent.CalculatePath(newItemScript.transform.position, path)) //Si el camí és possible
        {
            distance = Vector3.Distance(navMeshAgent.transform.position, newItemScript.transform.position);
            if (closestItemScript == null || distance < minDistance) //Si és el més proper
            {
                return true;
            }
        }
        distance = 0;
        return false;
    }

    public bool SendPeasantToArea(ConstructionScript constructionScript, bool adding)
    {
        if (adding) // Enviar a la construcció
        {
            float minDistance = 0f;
            PeasantScript peasantScript = null;
            for (int i = 0; i < peasantList.Count; i++)
            {
                if (constructionScript.constructionType == ConstructionScript.ConstructionType.Ship || peasantList[i].peasantType == PeasantScript.PeasantType.Adult)
                {
                    float distance;
                    if (CheckIfClosestPeasant(peasantScript, minDistance, constructionScript.entry.position, peasantList[i], out distance))
                    {
                        minDistance = distance;
                        peasantScript = peasantList[i];
                    }
                }
            }

            if(peasantScript != null)
            {
                peasantScript.constructionScript = constructionScript;
                constructionScript.peasantList.Add(peasantScript);
                peasantList.Remove(peasantScript);
                constructionScript.peasantsOnTheirWay++;

                if (peasantScript.peasantType == PeasantScript.PeasantType.Adult)
                {
                    TaskScript task = constructionScript.GetNextPendingTask();
                    if (task != null)
                    {
                        task.peasantScript = (PeasantAdultScript)peasantScript;
                        ((PeasantAdultScript)peasantScript).task = task;
                    }
                }
                peasantScript.UpdateTask();
                return true;
            }
            return false;
        }
        else // Desvincular de la construcció
        {
            PeasantScript peasantScript;
            if (constructionScript.constructionType == ConstructionScript.ConstructionType.Ship)
            {
                peasantScript = ((ShipScript)constructionScript).RemovePeasant(this);
            }
            else
            {
                peasantScript = constructionScript.peasantList[0];
                constructionScript.peasantList.Remove(peasantScript);
                if (!peasantScript.gameObject.activeInHierarchy) peasantScript.gameObject.SetActive(true);
                else constructionScript.peasantsOnTheirWay--;
            }
            peasantScript.constructionScript = null;
            peasantList.Add(peasantScript);

            if (peasantScript.peasantType == PeasantScript.PeasantType.Adult)
            {
                AsignItemToPeasant((PeasantAdultScript)peasantScript);
            }
            else peasantScript.UpdateTask();
            return true;
        }
    }

    public void SendAllPeasantsBack(ConstructionScript constructionScript)
    {
        for (int i = 0; i < constructionScript.peasantList.Count; i++)
        {
            PeasantScript peasantScript = constructionScript.peasantList[i];
            peasantScript.constructionScript = null;
            peasantList.Add(peasantScript);
            AsignItemToPeasant((PeasantAdultScript)peasantScript);
        }
    }

    public void CancelAllTripsToShip(ShipScript shipScript)
    {
        for(int i = shipScript.peasantList.Count - 1; i >= 0; i--)
        {
            PeasantScript peasantScript = shipScript.peasantList[i];
            if (peasantScript.gameObject.activeInHierarchy)
            {
                peasantList.Add(peasantScript);
                shipScript.peasantList.Remove(peasantScript);
                peasantScript.constructionScript = null;
                if (peasantScript.peasantType == PeasantScript.PeasantType.Adult) AsignItemToPeasant((PeasantAdultScript)peasantScript);
                peasantScript.UpdateTask();
            }
        }
        shipScript.peasantsOnTheirWay = 0;
    }

    public static Vector3 GetRandomPoint(Vector3 originPos)
    {
        NavMeshHit hit;
        do
        {
            Vector2 randomPos = Random.insideUnitCircle * IslandGenerator.mapChunkSize / 4;
            NavMesh.SamplePosition(originPos + new Vector3(randomPos.x, 0, randomPos.y), out hit, 5, NavMesh.AllAreas);
        }
        while (hit.position == Vector3.positiveInfinity);
        return hit.position;
    }

    public static Vector3 GetRandomPointWithinRange(Vector3 minPos, Vector3 maxPos)
    {
        Vector3 randomPos = new Vector3(Random.Range(minPos.x, maxPos.x), Random.Range(minPos.y, maxPos.y), Random.Range(minPos.z, maxPos.z));
        return GetClosestPointInNavMesh(randomPos);
    }

    public static Vector3 GetClosestPointInNavMesh(Vector3 pos)
    {
        NavMeshHit hit;
        NavMesh.SamplePosition(pos, out hit, 5, NavMesh.AllAreas);
        return hit.position;
    }
}
