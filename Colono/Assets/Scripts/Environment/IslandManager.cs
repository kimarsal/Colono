using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IslandManager : MonoBehaviour
{
    private List<GameObject> islands = new List<GameObject>();
    public GameObject player;
    public GameObject islandComponentsPrefab;
    private MapController mapController;
    private IslandGenerator islandGenerator;
    private IslandEditor islandEditor;
    public float spotDistance = 10f;
    public float islandDistance = 20f;
    public float playerDistance = 30f;

    void Start()
    {
        mapController = player.GetComponent<MapController>();
        islandGenerator = GetComponent<IslandGenerator>();
        islandEditor = GameObject.Find("GameManager").GetComponent<IslandEditor>();

        GenerateIsland(player.transform);
    }

    void Update()
    {
        /*foreach(GameObject island in islands)
        {
            if(Vector3.Distance(island.transform.position, player.transform.position) < playerDistance)
            {
                island.SetActive(true);
            }
            else
            {
                island.SetActive(false);
            }
        }*/
    }

    public void GenerateIsland(Transform player)
    {
        float angle = (player.transform.rotation.eulerAngles.y % 360) * Mathf.Deg2Rad;
        float x = Mathf.Sin(angle);
        float z = Mathf.Cos(angle);
        Vector3 pos = player.position + new Vector3(x, 0, z) * spotDistance;
        bool ok = true;
        /*foreach (GameObject island in islands)
        {
            if (Vector3.Distance(island.transform.position, pos) < islandDistance)
            {
                ok = false;
                break;
            }
        }*/
        if (ok)
        {
            //GameObject newIsland = Instantiate(islandPrefab, pos, islandPrefab.transform.rotation);
            Island newIsland = islandGenerator.GenerateIsland(new Vector2(0,80/*pos.x, pos.z*/));

            IslandCellScript islandCellScript = newIsland.island.AddComponent<IslandCellScript>();
            islandCellScript.meshData = newIsland.meshData;
            islandCellScript.regionMap = newIsland.regionMap;
            IslandScript islandScript = newIsland.island.AddComponent<IslandScript>();
            islandScript.islandCellScript = islandCellScript;
            islandCellScript.islandScript = islandScript;

            newIsland.island.GetComponent<NavMeshSurface>().BuildNavMesh();
            islandScript.npcManager = newIsland.island.AddComponent<NPCManager>();
            islandScript.npcManager.npcs = new GameObject("NPCs");
            islandScript.npcManager.npcs.transform.parent = newIsland.island.transform;
            islandScript.npcManager.npcs.transform.localPosition = Vector3.zero;
            islandScript.npcManager.islandEditor = islandEditor;
            //islandScript.npcManager.SpawnPeasants();

            mapController.FollowIsland(newIsland.island);
            islands.Add(newIsland.island);

            islandScript.items = new GameObject("Items");
            islandScript.items.transform.parent = newIsland.island.transform;
            islandScript.items.transform.localPosition = Vector3.zero;

            int row = 0, col = 0;
            while(row < IslandGenerator.mapChunkSize - 1)
            {
                Vector2 itemCell = new Vector2(col, row);
                Vector3 itemPos = MeshGenerator.GetCellCenter(itemCell, newIsland.meshData);
                /*if (itemPos.y > 0 && !islandScript.isCellTaken(itemCell))*/
                GameObject prefab = null;

                try
                {
                    switch (islandGenerator.regions[newIsland.regionMap[col, row]].name)
                    {
                        case "Sand": prefab = islandEditor.beachItems[UnityEngine.Random.Range(0, islandEditor.beachItems.Length)]; break;
                        case "Grass": prefab = islandEditor.fieldItems[UnityEngine.Random.Range(0, islandEditor.fieldItems.Length)]; break;
                        case "Grass 2": prefab = islandEditor.hillItems[UnityEngine.Random.Range(0, islandEditor.hillItems.Length)]; break;
                        case "Rock": prefab = islandEditor.mountainItems[UnityEngine.Random.Range(0, islandEditor.mountainItems.Length)]; break;
                    }
                }
                catch(Exception e)
                {
                    Debug.Log(e);
                }

                if(prefab != null)
                {
                    GameObject item = GameObject.Instantiate(prefab, newIsland.island.transform.position + itemPos, Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0), islandScript.items.transform);
                    islandScript.AddItem(item, itemCell);
                }

                int inc = UnityEngine.Random.Range(1, 20);
                col += inc;
                if(col > IslandGenerator.mapChunkSize - 1)
                {
                    row++;
                    col = col - IslandGenerator.mapChunkSize;
                }
            }

        }
        
    }
}
