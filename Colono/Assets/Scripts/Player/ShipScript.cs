using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class ShipScript : ConstructionScript
{
    private GameManager gameManager;

    public InventoryScript inventoryScript;

    public List<PeasantInfo> peasantInfoList;
    public int[] animals;
    public List<AnimalInfo> animalList;
    public int animalAmount;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        //AddDefaultElements();
    }

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
        AddAnimal(ResourceScript.AnimalType.Calf);
        AddAnimal(ResourceScript.AnimalType.Pig);
        AddAnimal(ResourceScript.AnimalType.Piglet);
        AddAnimal(ResourceScript.AnimalType.Sheep);
        AddAnimal(ResourceScript.AnimalType.Lamb);
        AddAnimal(ResourceScript.AnimalType.Chicken);
        AddAnimal(ResourceScript.AnimalType.Chick);

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

    public PeasantScript RemovePeasant(NPCManager npcManager)
    {
        PeasantInfo peasantInfo = peasantInfoList[0];
        peasantInfoList.RemoveAt(0);
        PeasantScript peasantScript = Instantiate(islandEditor.GetNPCPrefab(peasantInfo.peasantType, peasantInfo.peasantGender),
            entry.position, Quaternion.identity, npcManager.npcs.transform).GetComponent<PeasantScript>();
        peasantScript.transform.localScale = Vector3.one * 0.4f;
        peasantScript.InitializePeasant(peasantInfo);
        peasantScript.npcManager = npcManager;
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
                    penScript.transform).GetComponent<AnimalScript>();
                animalScript.penScript = penScript;
                animalScript.age = animalInfo.age;
                return animalScript;
            }
        }
        return null;
    }

    public override TaskScript GetNextPendingTask()
    {
        return null;
    }

    public override void FinishUpBusiness()
    {
        return;
    }
}
