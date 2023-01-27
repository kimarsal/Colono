using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class ShipScript : ConstructionScript
{
    public GameManager gameManager;
    public override IslandEditor islandEditor { get { return gameManager.islandEditor; } }
    public InventoryScript inventoryScript;

    public List<PeasantInfo> peasantInfoList;
    public int[] animals;
    public List<AnimalInfo> animalList;
    public int animalAmount;

    public float distanceToBoardIsland = 2f;

    public void AddDefaultElements()
    {
        peasantInfoList = new List<PeasantInfo>();
        for (int i = 0; i < 10; i++)
        {
            PeasantInfo peasantInfo = new PeasantInfo();
            peasantInfo.peasantType = Random.Range(0, 2) == 0 ? PeasantScript.PeasantType.Adult : PeasantScript.PeasantType.Child;
            peasantInfo.peasantGender = Random.Range(0, 2) == 0 ? PeasantScript.PeasantGender.Male : PeasantScript.PeasantGender.Female;
            peasantInfo.isNative = false;

            peasantInfo.headType = Random.Range(0, 2);
            peasantInfo._SKINCOLOR = new SerializableColor(islandEditor.GetRandomSkinColor());
            peasantInfo._HAIRCOLOR = new SerializableColor(islandEditor.GetRandomHairColor());
            peasantInfo._CLOTH3COLOR = new SerializableColor(Random.ColorHSV());
            peasantInfo._CLOTH4COLOR = new SerializableColor(Random.ColorHSV());
            peasantInfo._OTHERCOLOR = new SerializableColor(Random.ColorHSV());

            peasantInfo.position = new SerializableVector3(Vector3.zero);
            peasantInfo.age = 10;
            
            AddPeasant(peasantInfo);
        }

        animals = new int[Enum.GetValues(typeof(ResourceScript.AnimalType)).Length];
        animalList = new List<AnimalInfo>();
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

    public void AddPeasant(PeasantInfo peasantInfo)
    {
        peasantInfoList.Add(peasantInfo);
    }

    public override PeasantScript RemovePeasant()
    {
        PeasantInfo peasantInfo = peasantInfoList[0];
        peasantInfoList.RemoveAt(0);
        PeasantScript peasantScript = Instantiate(islandEditor.GetNPCPrefab(peasantInfo.peasantType, peasantInfo.peasantGender),
            entry.position, Quaternion.identity, gameManager.closestIsland.npcsTransform).GetComponent<PeasantScript>();
        peasantScript.transform.localScale = Vector3.one * 0.4f;
        peasantScript.InitializePeasant(peasantInfo);
        peasantScript.islandScript = gameManager.closestIsland;
        return peasantScript;
    }

    private void AddAnimal(ResourceScript.AnimalType animalType)
    {
        AddAnimal(islandEditor.GetAnimalPrefab(animalType).GetComponent<AnimalScript>().GetAnimalInfo());
    }

    public void AddAnimal(AnimalInfo animalInfo)
    {
        animalList.Add(animalInfo);
        animals[(int)animalInfo.animalType]++;
        animalAmount++;
    }

    public AnimalScript RemoveAnimal(PenScript penScript, ResourceScript.AnimalType animalType)
    {
        for (int i = 0; i < animalList.Count; i++)
        {
            AnimalInfo animalInfo = animalList[i];
            if (animalInfo.animalType == animalType)
            {
                animalList.RemoveAt(i);
                animals[(int)animalType]--;
                animalAmount--;
                AnimalScript animalScript = Instantiate(islandEditor.GetAnimalPrefab(animalType),
                    NPCManager.GetRandomPointWithinRange(penScript.minPos, penScript.maxPos),
                    Quaternion.Euler(0, UnityEngine.Random.Range(0, 359), 0),
                    penScript.animalTransform).GetComponent<AnimalScript>();
                animalScript.penScript = penScript;
                animalScript.age = animalInfo.age;
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
