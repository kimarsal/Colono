using System.Collections.Generic;
using UnityEngine;

public class IslandManager : MonoBehaviour
{
    public List<IslandScript> islandList = new List<IslandScript>();
    private Transform player;
    private MapController mapController;
    private IslandGenerator islandGenerator;
    private IslandEditor islandEditor;

    public int seed;
    public float spotDistance = 100f;
    public float islandDistance = 20f;
    public float playerDistance = 30f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        mapController = player.GetComponent<MapController>();
        islandGenerator = GetComponent<IslandGenerator>();
        islandEditor = GetComponent<IslandEditor>();

        //TryToGenerateIsland();
    }

    void Update()
    {
        /*foreach(IslandScript islandScript in islands)
        {
            float distance = Vector3.Distance(islandScript.transform.position, player.transform.position);
            if(!islandScript.gameObject.activeInHierarchy && distance < spotDistance)
            {
                islandScript.gameObject.SetActive(true);
            }
            else if(islandScript.gameObject.activeInHierarchy && distance > spotDistance)
            {
                islandScript.gameObject.SetActive(false);
            }
        }*/
    }

    public void TryToGenerateIsland()
    {
        float angle = (player.transform.rotation.eulerAngles.y % 360) * Mathf.Deg2Rad;
        float x = Mathf.Sin(angle);
        float z = Mathf.Cos(angle);
        Vector3 pos = player.position + new Vector3(x, 0, z) * spotDistance;
        foreach (IslandScript island in islandList)
        {
            if (Vector3.Distance(island.transform.position, pos) < islandDistance)
            {
                return; // Està massa a prop d'una altra illa
            }
        }
        
        IslandScript islandScript = islandGenerator.GenerateIsland(seed, new Vector2(0,80/*pos.x, pos.z*/), islandEditor);
        mapController.closestIsland = islandScript;
        islandList.Add(islandScript);
        
    }
}
