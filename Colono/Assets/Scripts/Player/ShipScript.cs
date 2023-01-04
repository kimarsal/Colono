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
    public int[] materials = new int[Enum.GetValues(typeof(ResourceScript.MaterialType)).Length];
    public int[] crops = new int[Enum.GetValues(typeof(ResourceScript.CropType)).Length];
    public int[] animals = new int[Enum.GetValues(typeof(ResourceScript.AnimalType)).Length];

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        islandEditor = GameObject.Find("GameManager").GetComponent<IslandEditor>();

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

    public void AddMaterial(ResourceScript.MaterialType materialType)
    {
        if (usage < capacity)
        {
            materials[(int)materialType]++;
            usage++;
        }
    }

    public void AddCrops(ResourceScript.CropType cropType, int cropAmount)
    {
        if (capacity - usage < cropAmount)
        {
            cropAmount = capacity - usage;
        }

        crops[(int)cropType] += cropAmount;
        usage += cropAmount;
    }
}
