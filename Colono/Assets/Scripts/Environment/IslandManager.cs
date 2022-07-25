using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandManager : MonoBehaviour
{
    private List<GameObject> islands = new List<GameObject>();
    public GameObject player;
    public GameObject islandComponentsPrefab;
    private MapController mapController;
    public float spotDistance = 10f;
    public float islandDistance = 20f;
    public float playerDistance = 30f;

    void Start()
    {
        mapController = player.GetComponent<MapController>();

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

            newIsland.island.AddComponent<IslandScript>();
            IslandCellScript script = newIsland.island.AddComponent<IslandCellScript>();
            script.meshData = newIsland.meshData;
            script.regionMap = newIsland.regionMap;
            mapController.FollowIsland(newIsland.island);
            islands.Add(newIsland.island);
        }
        
    }
}
