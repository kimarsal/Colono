using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ShipScript : ConstructionScript
{
    public GameObject npcs;
    private IslandEditor islandEditor;
    private GameManager gameManagerScript;
    public List<GameObject> peasantsList = new List<GameObject>();

    void Start()
    {
        gameManagerScript = GameObject.Find("GameManager").GetComponent<GameManager>();
        islandEditor = GameObject.Find("GameManager").GetComponent<IslandEditor>();

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
            GameObject peasant = Instantiate(prefab, center.position, prefab.transform.rotation, npcs.transform);
            peasant.GetComponent<PeasantScript>().InitializePeasant();
            peasant.SetActive(false);
            peasantsList.Add(peasant);
        }
    }

    void Update()
    {
        if (gameManagerScript.isInIsland && Input.GetMouseButtonDown(0))
        {
            RaycastHit raycastHit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out raycastHit, 100f))
            {
                if (raycastHit.transform.gameObject.CompareTag("Player"))
                {
                    gameManagerScript.SelectShip();
                }
            }
        }
    }

    public void SetClosestPoint(Vector3 colliderClosestPoint)
    {
        NavMeshHit hit;
        NavMesh.SamplePosition(colliderClosestPoint, out hit, 10, NavMesh.AllAreas);
        center.position = hit.position;
    }
}
