using System.Collections.Generic;
using UnityEngine;

public class IslandManager : MonoBehaviour
{
    public List<IslandScript> islandList = new List<IslandScript>();
    private Transform player;
    private IslandGenerator islandGenerator;
    private GameManager gameManager;
    private IslandEditor islandEditor;

    public int seed;
    public float spotDistance = 100f;
    public float islandDistance = 20f;
    public float playerDistance = 30f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        islandGenerator = GetComponent<IslandGenerator>();
        gameManager = GetComponent<GameManager>();
    }

    void Update()
    {
        float minDistance = Vector3.Distance(gameManager.closestIsland.transform.position, player.position);
        foreach(IslandScript islandScript in islandList)
        {
            float distance = Vector3.Distance(islandScript.transform.position, player.position);
            /*if(!islandScript.gameObject.activeInHierarchy && distance < spotDistance)
            {
                islandScript.gameObject.SetActive(true);
            }
            else if(islandScript.gameObject.activeInHierarchy && distance > spotDistance)
            {
                islandScript.gameObject.SetActive(false);
            }*/
            if(distance < minDistance)
            {
                minDistance = distance;
                gameManager.closestIsland = islandScript;
            }
        }
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
        
        IslandScript islandScript = islandGenerator.GenerateIsland(seed, new Vector2(0,80/*pos.x, pos.z*/));
        gameManager.closestIsland = islandScript;
        islandList.Add(islandScript);
        
    }
}
