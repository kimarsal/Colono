using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ShipScript : ConstructionScript
{
    public GameObject npcs;
    private IslandEditor islandEditor;
    private GameManager gameManager;

    public int capacity;
    public int usage;
    public int[][] resources;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        islandEditor = GameObject.Find("GameManager").GetComponent<IslandEditor>();

        resources = new int[Enum.GetValues(typeof(ResourceScript.ResourceType)).Length][];
        resources[0] = new int[Enum.GetValues(typeof(ResourceScript.MaterialType)).Length];
        resources[1] = new int[Enum.GetValues(typeof(ResourceScript.CropType)).Length];
        resources[2] = new int[Enum.GetValues(typeof(ResourceScript.MeatType)).Length];
        resources[3] = new int[Enum.GetValues(typeof(ResourceScript.AnimalType)).Length];

        resources[(int)ResourceScript.ResourceType.Crop][(int)ResourceScript.CropType.Onion] = 2;
        resources[(int)ResourceScript.ResourceType.Crop][(int)ResourceScript.CropType.Carrot] = 2;
        resources[(int)ResourceScript.ResourceType.Crop][(int)ResourceScript.CropType.Eggplant] = 2;
        resources[(int)ResourceScript.ResourceType.Animal][(int)ResourceScript.AnimalType.Cow] = 2;
        resources[(int)ResourceScript.ResourceType.Animal][(int)ResourceScript.AnimalType.Pig] = 2;
        usage = 10;

        for (int i = 0; i < 10; i++)
        {
            GameObject prefab = islandEditor.malePeasantPrefab;
            switch (i % 3)
            {
                case 1:
                    prefab = islandEditor.femalePeasantPrefab; break;
                case 2:
                    prefab = islandEditor.childPeasantPrefab; break;
            }
            GameObject peasant = Instantiate(prefab, entry.position, prefab.transform.rotation, npcs.transform);
            PeasantScript peasantScript = peasant.GetComponent<PeasantScript>();
            peasantScript.gameManager = gameManager;
            peasantScript.InitializePeasant();
            peasant.SetActive(false);
            peasantList.Add(peasantScript);
        }
    }

    void Update()
    {
        if (gameManager.CanSelect() && constructionDetailsScript == null)
        {
            RaycastHit raycastHit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out raycastHit, 100f))
            {
                if (raycastHit.transform.gameObject.CompareTag("Player"))
                {
                    outline.enabled = true;
                    if (Input.GetMouseButtonDown(0))
                    {
                        gameManager.SelectShip();
                    }
                }
                else
                {
                    outline.enabled = false;
                }
            }
        }
    }

    public void SetClosestPoint(Vector3 colliderClosestPoint)
    {
        NavMeshHit hit;
        if(NavMesh.SamplePosition(colliderClosestPoint, out hit, 15, NavMesh.AllAreas))
        {
            entry.position = hit.position;
        }
    }

    public override TaskScript GetNextPendingTask()
    {
        return null;
    }

    public void AddResource(ResourceScript.ResourceType resourceType, int resourceIndex, int amount)
    {
        if (capacity - usage < amount)
        {
            amount = capacity - usage;
        }

        resources[(int)resourceType][resourceIndex] += amount;
        usage += amount;
    }
}
