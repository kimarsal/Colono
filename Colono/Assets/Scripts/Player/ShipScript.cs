using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class ShipScript : ConstructionScript
{
    [JsonIgnore] public GameManager gameManager;
    [JsonIgnore] public override IslandEditor islandEditor { get { return gameManager.islandEditor; } }

    public Vector3 position;
    public int orientation;

    public InventoryScript inventoryScript;
    public int[] animals;
    public List<AnimalScript> animalList;
    public int animalAmount;

    [JsonIgnore] public float distanceToBoardIsland = 2f;

    public void AddDefaultElements()
    {
        for (int i = 0; i < 10; i++)
        {
            PeasantScript.PeasantType peasantType = (PeasantScript.PeasantType)Random.Range(0, 2);
            PeasantScript.PeasantGender peasantGender = (PeasantScript.PeasantGender)Random.Range(0, 2);
            PeasantScript peasantScript = Instantiate(islandEditor.GetNPCPrefab(peasantType, peasantGender), entry.position, Quaternion.identity, entry).GetComponent<PeasantScript>();
            peasantScript.isNative = false;
            
            peasantScript.headType = Random.Range(0, 2);
            peasantScript._SKINCOLOR = islandEditor.GetRandomSkinColor();
            peasantScript._HAIRCOLOR = islandEditor.GetRandomHairColor();
            peasantScript._CLOTH3COLOR = Random.ColorHSV();
            peasantScript._CLOTH4COLOR = Random.ColorHSV();
            peasantScript._OTHERCOLOR = Random.ColorHSV();

            peasantScript.gameObject.SetActive(false);

            peasantList.Add(peasantScript);
        }

        animals = new int[Enum.GetValues(typeof(ResourceScript.AnimalType)).Length];
        animalList = new List<AnimalScript>();
        AddAnimal(ResourceScript.AnimalType.Cow);
        AddAnimal(ResourceScript.AnimalType.Cow);
        AddAnimal(ResourceScript.AnimalType.Pig);
        AddAnimal(ResourceScript.AnimalType.Pig);
        AddAnimal(ResourceScript.AnimalType.Sheep);
        AddAnimal(ResourceScript.AnimalType.Sheep);
        AddAnimal(ResourceScript.AnimalType.Chicken);
        AddAnimal(ResourceScript.AnimalType.Chicken);

        inventoryScript.capacity = 30;
        inventoryScript.AddResource(ResourceScript.ResourceType.Crop, (int)ResourceScript.CropType.Onion, 2);
        inventoryScript.AddResource(ResourceScript.ResourceType.Crop, (int)ResourceScript.CropType.Carrot, 2);
        inventoryScript.AddResource(ResourceScript.ResourceType.Crop, (int)ResourceScript.CropType.Cucumber, 2);
    }

    void Update()
    {
        bool closeToIsland = false;
        Vector3 colliderClosestPoint = new Vector3();
        float minDistance = 10;
        foreach (MeshCollider meshCollider in gameManager.closestIsland.convexColliders.GetComponentsInChildren<MeshCollider>())
        {
            colliderClosestPoint = Physics.ClosestPoint(transform.position, meshCollider, gameManager.closestIsland.transform.position, gameManager.closestIsland.transform.rotation);
            float distanceToClosestPoint = Vector3.Distance(transform.position, colliderClosestPoint);
            if (distanceToClosestPoint < minDistance)
            {
                minDistance = distanceToClosestPoint;
            }
            if (distanceToClosestPoint < distanceToBoardIsland)
            {
                closeToIsland = true;
                break;
            }

        }

        if (closeToIsland)
        {
            GetComponent<ShipScript>().SetClosestPoint(colliderClosestPoint);
            gameManager.PlayerIsNearIsland();
        }
        else
        {
            gameManager.PlayerIsFarFromIsland();
        }

        position = transform.position;
        orientation = (int)transform.rotation.y % 360;
    }

    public override void EditConstruction()
    {
        gameManager.canvasScript.ShowInventoryEditor();
    }

    public void SetClosestPoint(Vector3 colliderClosestPoint)
    {
        NavMeshHit hit;
        if(NavMesh.SamplePosition(colliderClosestPoint, out hit, 15, NavMesh.AllAreas))
        {
            entry.position = hit.position;
        }
    }

    public override PeasantScript RemovePeasant()
    {
        PeasantScript peasantScript = base.RemovePeasant();
        peasantScript.transform.parent = gameManager.closestIsland.npcsTransform;
        peasantScript.islandScript = gameManager.closestIsland;
        return peasantScript;
    }

    private void AddAnimal(ResourceScript.AnimalType animalType)
    {
        AddAnimal(islandEditor.GetAnimalPrefab(animalType).GetComponent<AnimalScript>());
    }

    public void AddAnimal(AnimalScript animalScript)
    {
        animalList.Add(animalScript);
        animals[(int)animalScript.animalType]++;
        animalAmount++;
    }

    public AnimalScript RemoveAnimal(PenScript penScript, ResourceScript.AnimalType animalType)
    {
        for (int i = 0; i < animalList.Count; i++)
        {
            AnimalScript animal = animalList[i];
            if (animal.animalType == animalType)
            {
                animalList.RemoveAt(i);
                animals[(int)animalType]--;
                animalAmount--;
                AnimalScript animalScript = Instantiate(islandEditor.GetAnimalPrefab(animalType),
                    NPCManager.GetRandomPointWithinRange(penScript.minPos, penScript.maxPos),
                    Quaternion.Euler(0, UnityEngine.Random.Range(0, 359), 0),
                    penScript.animalTransform).GetComponent<AnimalScript>();
                animalScript.penScript = penScript;
                animalScript.age = animal.age;
                return animalScript;
            }
        }
        return null;
    }

    public override void FinishUpBusiness()
    {
        return;
    }
}
