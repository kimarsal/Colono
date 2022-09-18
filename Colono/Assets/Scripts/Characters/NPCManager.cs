using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCManager : MonoBehaviour
{
    public IslandEditor islandEditor;
    public GameObject npcs;
    private List<GameObject> peasantsList = new List<GameObject>();

    public int numPeasants = 10;
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
        for (int i = 0; i < numPeasants; i++)
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
            npc.GetComponent<PeasantScript>().npcManager = this;
            peasantsList.Add(npc);
        }
    }

    public void GeneratePeasants(BuildingScript buildingScript)
    {
        for (int i = 0; i < numPeasants; i++)
        {
            GameObject prefab = islandEditor.malePeasantPrefab;
            switch (i % 3)
            {
                case 1:
                    prefab = islandEditor.femalePeasantPrefab; break;
                case 2:
                    prefab = islandEditor.childPeasantPrefab; break;
            }
            GameObject npc = Instantiate(prefab, buildingScript.center.position, prefab.transform.rotation, npcs.transform);
            npc.GetComponent<PeasantScript>().npcManager = this;
            npc.GetComponent<PeasantScript>().settlementScript = buildingScript;
            npc.SetActive(false);
            peasantsList.Add(npc);
        }
    }

    public int GetAvailablePeasants()
    {
        return numPeasants;
    }

    public void SendPeasantToArea(ConstructionScript constructionScript, bool adding)
    {
        numPeasants -= adding ? 1 : -1;
        constructionScript.numPeasants += adding ? 1 : -1;

        for(int i = 0; i < numPeasants; i++)
        {
            PeasantScript peasantScript = peasantsList[i].GetComponent<PeasantScript>();
            if (adding && peasantScript.constructionScript == null)
            {
                peasantScript.constructionScript = constructionScript;
                peasantsList[i].SetActive(true);
                peasantScript.UpdateTask();
                break;
            }
            else if(!adding && peasantScript.constructionScript == constructionScript)
            {
                peasantScript.constructionScript = null;
                peasantScript.UpdateTask();
                break;
            }
        }
    }

    public void SendPeasantsBack(ConstructionScript constructionScript)
    {
        numPeasants += constructionScript.numPeasants;
    }

    public Vector3 GetRandomPoint(Vector3 originPos)
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
