using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandManager : MonoBehaviour
{
    public List<IslandScript> islandList = new List<IslandScript>();
    private Transform player;
    private IslandGenerator islandGenerator;
    private GameManager gameManager;

    public int seed;
    private float distanceBetweenActiveIslandsAndPlayer = 200f;
    private float distanceBetweenIslands = 300f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        islandGenerator = GetComponent<IslandGenerator>();
        gameManager = GetComponent<GameManager>();
    }

    void Update()
    {
        if (gameManager.closestIsland == null) return;
        
        float minDistance = Vector3.Distance(gameManager.closestIsland.transform.position, player.position);
        foreach(IslandScript islandScript in islandList)
        {
            float distance = Vector3.Distance(islandScript.transform.position, player.position);
            /*if(!islandScript.gameObject.activeInHierarchy && distance < distanceBetweenActiveIslandsAndPlayer)
            {
                islandScript.gameObject.SetActive(true);
            }
            else if(islandScript.gameObject.activeInHierarchy && distance > distanceBetweenActiveIslandsAndPlayer)
            {
                islandScript.gameObject.SetActive(false);
            }*/
            if(distance < minDistance)
            {
                minDistance = distance;
                gameManager.closestIsland = islandScript;
            }
        }
        if (minDistance > distanceBetweenIslands) TryToGenerateIsland();
    }

    public void TryToGenerateIsland()
    {
        float angle = (player.transform.rotation.eulerAngles.y % 360) * Mathf.Deg2Rad;
        float x = Mathf.Sin(angle);
        float z = Mathf.Cos(angle);
        Vector3 pos = player.position + new Vector3(x, 0, z) * distanceBetweenActiveIslandsAndPlayer;
        foreach (IslandScript island in islandList)
        {
            if (Vector3.Distance(island.transform.position, pos) < distanceBetweenIslands)
            {
                return; // Està massa a prop d'una altra illa
            }
        }

        GenerateIslandCoroutine(new Vector2(pos.x, pos.z));
    }

    public void GenerateIslandCoroutine(Vector2 offset)
    {
        IslandScript islandScript = islandGenerator.GenerateIsland(seed, offset);
        islandList.Add(islandScript);
        gameManager.closestIsland = islandScript;
    }
}
