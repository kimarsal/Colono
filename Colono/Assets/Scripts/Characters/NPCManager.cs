using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCManager : MonoBehaviour
{
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

    void Start()
    {
        /*for (int i = 0; i < numPeasants; i++)
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
        }*/


    }

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

    public void SendPeasantToIsland(ShipScript shipScript, bool adding)
    {
        if (adding) // Enviar al vaixell
        {
            PeasantScript peasantScript = peasantList[0];
            peasantScript.transform.parent = shipScript.npcs.transform;
            shipScript.peasantList.Add(peasantScript);
            peasantList.Remove(peasantScript);
            peasantScript.constructionScript = shipScript;
            peasantScript.UpdateTask();
        }
        else // Enviar a l'illa
        {
            PeasantScript peasantScript = shipScript.peasantList[0];
            peasantScript.transform.parent = npcs.transform;
            peasantScript.transform.localScale = Vector3.one * 0.4f;
            peasantScript.transform.position = shipScript.center.position;
            peasantList.Add(peasantScript);
            shipScript.peasantList.Remove(peasantScript);
            peasantScript.gameObject.SetActive(true);

            if(peasantScript.peasantType == PeasantScript.PeasantType.Adult)
            {
                for (int i = 0; i < itemsToClear.Count; i++)
                {
                    if (itemsToClear[i].peasantScript == null)
                    {
                        itemsToClear[i].peasantScript = (PeasantAdultScript)peasantScript;
                        ((PeasantAdultScript)peasantScript).task = itemsToClear[i];
                        ((PeasantAdultScript)peasantScript).UpdateTask();
                        break;
                    }
                }
            }
        }
    }

    public void AddItemToClear(ItemScript item)
    {
        itemsToClear.Add(item);
        for (int i = 0; i < peasantList.Count; i++)
        {
            PeasantAdultScript peasantScript = peasantList[i].GetComponent<PeasantAdultScript>();
            if (peasantScript != null && peasantScript.constructionScript == null && peasantScript.task == null) //Si és adult i no té tasques
            {
                peasantScript.task = item;
                item.peasantScript = peasantScript;
                peasantScript.UpdateTask();
                break;
            }
        }
    }

    public void RemoveItemToClear(ItemScript item)
    {
        itemsToClear.Remove(item);
        PeasantAdultScript peasantScript = item.peasantScript;
        if(peasantScript != null) //Tenia un NPC vinculat
        {
            bool otherItemNeedsClearing = false;
            foreach(ItemScript itemScript in itemsToClear)
            {
                if(itemScript.peasantScript == null)
                {
                    otherItemNeedsClearing = true;
                    peasantScript.task = item;
                    item.peasantScript = peasantScript;
                    break;
                }
            }
            if (!otherItemNeedsClearing)
            {
                peasantScript.task = null;
            }
            peasantScript.UpdateTask();
        }
    }

    public void SendPeasantToArea(ConstructionScript constructionScript, bool adding)
    {
        if (adding) // Enviar a la construcció
        {
            for (int i = 0; i < peasantList.Count; i++)
            {
                PeasantScript peasantScript = peasantList[i];
                if(constructionScript.constructionType == ConstructionScript.ConstructionType.Ship
                    || peasantScript.peasantType == PeasantScript.PeasantType.Adult) //Si la construcció és el vaixell o és adult
                {
                    peasantScript.constructionScript = constructionScript;
                    peasantScript.transform.parent = constructionScript.peasants.transform;
                    constructionScript.peasantList.Add(peasantScript);
                    peasantList.Remove(peasantScript);

                    if(peasantScript.peasantType == PeasantScript.PeasantType.Adult)
                    {
                        TaskScript task = constructionScript.GetNextPendingTask();
                        if(task != null)
                        {
                            task.peasantScript = (PeasantAdultScript)peasantScript;
                            ((PeasantAdultScript)peasantScript).task = task;
                        }
                    }
                    peasantScript.UpdateTask();
                    break;
                }
            }
        }
        else // Desvincular de la construcció
        {
            PeasantScript peasantScript = constructionScript.peasantList[0];
            peasantScript.constructionScript = null;
            if(peasantScript.peasantType == PeasantScript.PeasantType.Adult)
            {
                ((PeasantAdultScript)peasantScript).task.hasBeenCanceled = true;
            }
            peasantScript.transform.parent = npcs.transform;
            peasantList.Add(peasantScript);
            constructionScript.peasantList.Remove(peasantScript);
            peasantScript.UpdateTask();
        }
    }

    public void SendAllPeasantsBack(ConstructionScript constructionScript)
    {
        for (int i = 0; i < constructionScript.peasantList.Count; i++)
        {
            PeasantScript peasantScript = constructionScript.peasantList[i];
            peasantScript.constructionScript = null;
            peasantScript.transform.parent = npcs.transform;
            peasantList.Add(peasantScript);
            ((PeasantAdultScript)peasantScript).UpdateTask();
        }
    }

    public static Vector3 GetRandomPoint(Vector3 originPos)
    {
        NavMeshHit hit;
        do
        {
            Vector2 randomPos = Random.insideUnitCircle * IslandGenerator.mapChunkSize / 2;
            NavMesh.SamplePosition(originPos + new Vector3(randomPos.x, 0, randomPos.y), out hit, 20, NavMesh.AllAreas);
        }
        while (hit.position.y < 0);
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
