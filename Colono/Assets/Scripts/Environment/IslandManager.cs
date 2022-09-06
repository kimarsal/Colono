using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandManager : MonoBehaviour
{
    private List<GameObject> islands = new List<GameObject>();
    public GameObject player;
    public GameObject islandComponentsPrefab;
    private MapController mapController;
    private IslandEditor islandEditor;
    public float spotDistance = 10f;
    public float islandDistance = 20f;
    public float playerDistance = 30f;

    void Start()
    {
        mapController = player.GetComponent<MapController>();
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
            Island newIsland = GetComponent<IslandGenerator>().GenerateIsland(new Vector2(0,80/*pos.x, pos.z*/));

            IslandCellScript islandCellScript = newIsland.island.AddComponent<IslandCellScript>();
            islandCellScript.meshData = newIsland.meshData;
            islandCellScript.regionMap = newIsland.regionMap;
            IslandScript islandScript = newIsland.island.AddComponent<IslandScript>();
            islandScript.islandCellScript = islandCellScript;
            islandCellScript.islandScript = islandScript;

            mapController.FollowIsland(newIsland.island);
            islands.Add(newIsland.island);

            islandScript.trees = new GameObject("Trees");
            islandScript.trees.transform.parent = newIsland.island.transform;
            islandScript.trees.transform.localPosition = Vector3.zero;

            islandScript.bushes = new GameObject("Bushes");
            islandScript.bushes.transform.parent = newIsland.island.transform;
            islandScript.bushes.transform.localPosition = Vector3.zero;

            islandScript.rocks = new GameObject("Rocks");
            islandScript.rocks.transform.parent = newIsland.island.transform;
            islandScript.rocks.transform.localPosition = Vector3.zero;

            islandScript.flowers = new GameObject("Flowers");
            islandScript.flowers.transform.parent = newIsland.island.transform;
            islandScript.flowers.transform.localPosition = Vector3.zero;

            islandScript.miscellaneous = new GameObject("Miscellaneous");
            islandScript.miscellaneous.transform.parent = newIsland.island.transform;
            islandScript.miscellaneous.transform.localPosition = Vector3.zero;

            int row = 0, col = 0;
            while(row < IslandGenerator.mapChunkSize - 1)
            {
                Vector2 itemCell = new Vector2(col, row);
                Vector3 itemPos = MeshGenerator.GetItemPosition(itemCell, newIsland.meshData);
                Debug.Log(itemCell);
                if (itemPos.y > 0 && islandScript.isCellAvailable(itemCell))
                {
                    IslandScript.ItemType type = (IslandScript.ItemType)UnityEngine.Random.Range(0, System.Enum.GetNames(typeof(IslandScript.ItemType)).Length);
                    Transform parent = null;
                    GameObject prefab = null;

                    switch (type)
                    {
                        case IslandScript.ItemType.Tree: parent = islandScript.trees.transform; prefab = islandEditor.trees[UnityEngine.Random.Range(0, islandEditor.trees.Length)]; break;
                        case IslandScript.ItemType.Bush: parent = islandScript.bushes.transform; prefab = islandEditor.bushes[UnityEngine.Random.Range(0, islandEditor.bushes.Length)]; break;
                        case IslandScript.ItemType.Rock: parent = islandScript.rocks.transform; prefab = islandEditor.rocks[UnityEngine.Random.Range(0, islandEditor.rocks.Length)]; break;
                        case IslandScript.ItemType.Flower: parent = islandScript.flowers.transform; prefab = islandEditor.flowers[UnityEngine.Random.Range(0, islandEditor.flowers.Length)]; break;
                        case IslandScript.ItemType.Miscellaneous: parent = islandScript.miscellaneous.transform; prefab = islandEditor.miscellaneous[UnityEngine.Random.Range(0, islandEditor.miscellaneous.Length)]; break;
                    }

                    GameObject item = GameObject.Instantiate(prefab, newIsland.island.transform.position + itemPos, Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0), parent);
                    islandScript.AddItem(item, type, itemCell);
                }

                int inc = UnityEngine.Random.Range(1, 5);
                col += inc;
                if(col >= IslandGenerator.mapChunkSize - 2)
                {
                    row++;
                    col = IslandGenerator.mapChunkSize - col;
                }
            }

        }
        
    }
}
