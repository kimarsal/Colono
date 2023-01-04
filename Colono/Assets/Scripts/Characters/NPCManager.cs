using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.Progress;

public class NPCManager : MonoBehaviour
{
    public IslandScript islandScript;
    public IslandEditor islandEditor;
    public GameObject npcs;
    public List<PeasantScript> peasantList = new List<PeasantScript>();
    private List<ItemScript> itemsToClear = new List<ItemScript>();

    public int numWarriors = 0;
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
                PeasantAdultScript peasantAdultScript = (PeasantAdultScript)peasantList[i];
                float distance = Vector3.Distance(peasantAdultScript.transform.position, item.transform.position);
                if (peasantAdultScript.constructionScript == null && peasantAdultScript.task == null) //Si no té tasques
                {
                    if(peasantScript == null || distance < minDistance)
                    {
                        peasantScript = peasantAdultScript;
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

    public void SendPeasantToArea(ConstructionScript constructionScript, bool adding)
    {
        if (adding) // Enviar a la construcció
        {
            PeasantScript peasantScript = null;
            float minDistance = 0f;
            for (int i = 0; i < peasantList.Count; i++)
            {
                PeasantScript p = peasantList[i];
                if(constructionScript.constructionType == ConstructionScript.ConstructionType.Ship
                    || p.peasantType == PeasantScript.PeasantType.Adult) //Si la construcció és el vaixell o el personatge és adult
                {
                    float distance = Vector3.Distance(p.transform.position, constructionScript.entry.position);
                    if(peasantScript == null || distance < minDistance)
                    {
                        peasantScript = p;
                        minDistance = distance;
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
            }
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
            ((PeasantAdultScript)peasantScript).UpdateTask();
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
        NavMeshHit hit;
        Vector3 randomPos = new Vector3(Random.Range(minPos.x, maxPos.x), Random.Range(minPos.y, maxPos.y), Random.Range(minPos.z, maxPos.z));
        NavMesh.SamplePosition(randomPos, out hit, 5, NavMesh.AllAreas);
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
