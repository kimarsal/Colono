using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ShipScript : ConstructionScript
{
    public Transform npcsTransform;
    public Transform animalsTransform;
    private IslandEditor islandEditor;
    private GameManager gameManager;

    public InventoryScript inventoryScript;

    public int[] animals;
    public List<AnimalScript> animalList = new List<AnimalScript>();
    public int animalAmount;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        islandEditor = GameObject.Find("GameManager").GetComponent<IslandEditor>();
        inventoryScript = GetComponent<InventoryScript>();

        inventoryScript.AddResource(ResourceScript.ResourceType.Crop, (int)ResourceScript.CropType.Onion, 2);
        inventoryScript.AddResource(ResourceScript.ResourceType.Crop, (int)ResourceScript.CropType.Carrot, 2);
        inventoryScript.AddResource(ResourceScript.ResourceType.Crop, (int)ResourceScript.CropType.Cucumber, 2);

        inventoryScript.AddResource(ResourceScript.ResourceType.Animal, (int)ResourceScript.AnimalType.Cow, 2);

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
            GameObject peasant = Instantiate(prefab, transform.position, prefab.transform.rotation, npcsTransform);
            PeasantScript peasantScript = peasant.GetComponent<PeasantScript>();
            peasantScript.gameManager = gameManager;
            peasantScript.InitializePeasant();
            peasant.SetActive(false);
            peasantList.Add(peasantScript);
        }

        animals = new int[Enum.GetValues(typeof(ResourceScript.AnimalType)).Length];
        AddAnimal(ResourceScript.AnimalType.Cow);
        AddAnimal(ResourceScript.AnimalType.Calf);
        AddAnimal(ResourceScript.AnimalType.Pig);
        AddAnimal(ResourceScript.AnimalType.Piglet);
        AddAnimal(ResourceScript.AnimalType.Sheep);
        AddAnimal(ResourceScript.AnimalType.Lamb);
        AddAnimal(ResourceScript.AnimalType.Chicken);
        AddAnimal(ResourceScript.AnimalType.Chick);
    }

    private void AddAnimal(ResourceScript.AnimalType animalType)
    {
        GameObject prefab = islandEditor.animals[(int)animalType];
        AnimalScript animalScript = Instantiate(prefab, transform.position, prefab.transform.rotation, animalsTransform).GetComponent<AnimalScript>();
        animalScript.gameObject.SetActive(false);
        animalList.Add(animalScript);
        animals[(int)animalType]++;
        animalAmount++;
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



    public void AddAnimal(AnimalScript animalScript)
    {
        animalList.Add(animalScript);
        animals[(int)animalScript.animalType]++;
        animalAmount++;

        animalScript.gameObject.SetActive(false);
        animalScript.transform.parent = animalsTransform;
    }

    public AnimalScript RemoveAnimal(ResourceScript.AnimalType animalType)
    {
        for (int i = 0; i < animalList.Count; i++)
        {
            AnimalScript animalScript = animalList[i];
            if (animalScript.animalType == animalType)
            {
                animalList.RemoveAt(i);
                animals[(int)animalType]--;
                animalAmount--;
                return animalScript;
            }
        }
        return null;
    }

}
