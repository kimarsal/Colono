using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.PlayerSettings;
using static UnityEditor.Progress;
using static UnityEngine.GraphicsBuffer;

public class NPCManager : MonoBehaviour
{
    public IslandScript islandScript;
    public IslandEditor islandEditor;
    public GameObject npcs;
    public List<PeasantScript> peasantList = new List<PeasantScript>();
    private List<ItemScript> itemsToClear = new List<ItemScript>();
    //public int availableAdultPeasants = 0;

    public static float minX = -20;
    public static float maxX = 20;
    public static float minZ = -20;
    public static float maxZ = 20;

    private static Color[] skinColorsCaucassian =
        new Color[] { 
            new Color(255, 205, 148),
            new Color(255, 224, 189),
            new Color(234, 192, 134),
            new Color(255, 227, 159),
            new Color(255, 173, 96)
        };

    private static Color[] hairColors =
        new Color[] {
            new Color(35, 18, 11),
            new Color(61, 35, 20),
            new Color(90, 56, 37),
            new Color(204, 153, 102),
            new Color(44, 22, 8)
        };

    /*void Start()
    {
        for (int i = 0; i < numPeasants; i++)
        {
            GameObject prefab = malePeasantPrefab;
            switch (i % 3)
            {
                case 1:
                    prefab = femalePeasantPrefab; break;
                case 2:
                    prefab = childPeasantPrefab; break;
            }
            Instantiate(prefab, new Vector3(Random.Range(minX, maxX), 0f, Random.Range(minZ, maxZ)), prefab.transform.rotation);
        }

        for (int i = 0; i < numWarriors; i++)
        {
            GameObject prefab = maleWarriorPrefab;
            if (Random.Range(0, 2) == 0) prefab = femaleWarriorPrefab;
            Instantiate(prefab, new Vector3(Random.Range(minX, maxX), 0f, Random.Range(minZ, maxZ)), prefab.transform.rotation);
        }


    }*/

    public void SpawnPeasants()
    {
        for (int i = 0; i < peasantList.Count; i++)
        {
            GameObject prefab = islandEditor.malePeasantPrefab;
            switch (i % 3)
            {
                case 1:
                    prefab = islandEditor.femalePeasantPrefab; break;
                case 2:
                    prefab = islandEditor.childPeasantPrefab; break;
            }
            GameObject npc = Instantiate(prefab, GetRandomPoint(transform.position), prefab.transform.rotation, npcs.transform);
            peasantList.Add(npc.GetComponent<PeasantScript>());
        }
    }

    public void AsignItemToPeasant(PeasantAdultScript peasantScript)
    {
        peasantScript.CancelTask();

        bool otherItemNeedsClearing = false;
        foreach (ItemScript itemScript in itemsToClear)
        {
            if (itemScript.peasantScript == null) //Si no té un NPC vinculat
            {
                otherItemNeedsClearing = true;
                itemScript.peasantScript = peasantScript;
                peasantScript.task = itemScript;
                break;
            }
        }
        if (!otherItemNeedsClearing)
        {
            peasantScript.task = null;
        }
        peasantScript.UpdateTask();
    }

    public void AddItemToClear(ItemScript item)
    {
        itemsToClear.Add(item);
        PeasantAdultScript peasantScript = null;
        float minDistance = 0f;
        for (int i = 0; i < peasantList.Count; i++)
        {
            if(peasantList[i].peasantType == PeasantScript.PeasantType.Adult) //Si és adult
            {
                PeasantAdultScript newPeasantScript = (PeasantAdultScript)peasantList[i];
                if (newPeasantScript.constructionScript == null && newPeasantScript.task == null) //Si no té tasques
                {
                    float distance;
                    if(CheckIfClosestPeasant(peasantScript, minDistance, item.transform.position, newPeasantScript, out distance))
                    {
                        peasantScript = newPeasantScript;
                        minDistance = distance;
                    }
                }
            }
        }
        if(peasantScript != null)
        {
            peasantScript.task = item;
            item.peasantScript = peasantScript;
            peasantScript.UpdateTask();
        }
    }

    public void RemoveItemToClear(ItemScript item)
    {
        itemsToClear.Remove(item);
        PeasantAdultScript peasantScript = item.peasantScript;
        item.peasantScript = null;
        if(peasantScript != null) //Tenia un NPC vinculat
        {
            AsignItemToPeasant(peasantScript);
        }
    }

    public bool CheckIfClosestPeasant(PeasantScript previousPeasantScript, float minDistance, Vector3 destination, PeasantScript newPeasantScript, out float distance)
    {
        distance = 0;
        NavMeshPath path = new NavMeshPath();
        if (newPeasantScript.GetComponent<NavMeshAgent>().CalculatePath(destination, path)) //Si el camí és possible
        {
            distance = Vector3.Distance(newPeasantScript.transform.position, destination);
            if (previousPeasantScript == null || distance < minDistance) //Si és el més proper
            {
                return true;
            }
        }
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
            PeasantScript peasantScript = constructionScript.peasantList[0];
            if (constructionScript.constructionType == ConstructionScript.ConstructionType.Ship)
            {
                peasantScript.npcManager = this;
                peasantScript.transform.parent = npcs.transform;
                peasantScript.transform.localScale = Vector3.one * 0.4f;
                peasantScript.transform.position = constructionScript.entry.position;
            }
            peasantScript.constructionScript = null;
            peasantList.Add(peasantScript);
            constructionScript.peasantList.Remove(peasantScript);

            if (!peasantScript.gameObject.activeInHierarchy) peasantScript.gameObject.SetActive(true);
            else constructionScript.peasantsOnTheirWay--;

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

    public static Color GetRandomSkinColor()
    {
        return normalizeColor(skinColorsCaucassian[Random.Range(0, skinColorsCaucassian.Length)]);
    }

    public static Color GetRandomHairColor()
    {
        return normalizeColor(hairColors[Random.Range(0, hairColors.Length)]);
    }

    private static Color normalizeColor(Color color)
    {
        return new Color(color.r/256, color.g/256, color.b/256, color.a);
    }
}
