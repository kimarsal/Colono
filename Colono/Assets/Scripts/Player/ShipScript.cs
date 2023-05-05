using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static ResourceScript;
using Random = UnityEngine.Random;

public class ShipScript : ConstructionScript
{
    public static ShipScript Instance { get; private set; }

    [JsonProperty] [JsonConverter(typeof(VectorConverter))] public Vector3 position;
    [JsonProperty] public int orientation;
    [JsonProperty] public IslandScript shipInterior;
    public PenScript shipInteriorPen { get { return (PenScript)shipInterior.constructionList[0]; } }

    public Transform nextIslandTransform;
    public Transform enemyShipTransform;

    public override Sprite sprite { get { return ResourceScript.Instance.shipSprite; } }
    public override bool canBeRemoved { get { return false; } }
    public override int peasantCount { get { return peasantList.Count + shipInterior.peasantList.Count; } }
    public override EditorScript editorScript { get { return CanvasScript.Instance.shipEditor; } }

    public FishingScript fishingScript;

    private void Awake()
    {
        Instance = this;
        fishingScript = GetComponent<FishingScript>();
    }

    public void AddDefaultElements()
    {
        shipInterior.inventoryScript = new InventoryScript();
        shipInterior.inventoryScript.AddCapacityToAllCategories();

        for (int i = 0; i < GetEnumLength(ResourceType.Crop) / 2; i++)
        {
            shipInterior.inventoryScript.AddResource(ResourceType.Crop, i, 1);
        }

        for (int i = 0; i < GetEnumLength(ResourceType.Meat); i++)
        {
            shipInterior.inventoryScript.AddResource(ResourceType.Meat, i, 10);
        }

        shipInterior.inventoryScript.AddResource(ResourceType.Material, (int)MaterialType.Wood, 45);
        shipInterior.inventoryScript.AddResource(ResourceType.Material, (int)MaterialType.Gem, 5);

        ((TavernScript)shipInterior.constructionList[1]).recipeList = new List<Recipe> {
            new Recipe(-1, -1, (int)MeatType.Chicken),
            new Recipe(-1, -1, (int)MeatType.Mutton),
            new Recipe(-1, -1, (int)MeatType.Pork),
            new Recipe(-1, -1, (int)MeatType.Cow),
        };

        for (int i = 0; i < 20; i++)
        {
            PeasantScript.PeasantType peasantType = (PeasantScript.PeasantType)Random.Range(0, 2);
            PeasantScript.PeasantGender peasantGender = (PeasantScript.PeasantGender)Random.Range(0, 2);
            PeasantScript peasantScript = Instantiate(ResourceScript.Instance.GetPeasantPrefab(peasantType, peasantGender),
                shipInteriorPen.transform.position, Quaternion.identity, shipInterior.npcsTransform);
            
            peasantScript.InitializePeasant();

            peasantScript.islandScript = shipInterior;
            shipInterior.peasantList.Add(peasantScript);
        }
        peasantsInside = shipInterior.peasantList.Count;

        int animalTypes = ResourceScript.GetEnumLength(ResourceType.Animal);
        for (int i = 0; i < 2; i++)
        {
            for(int j = 0; j < 10; j++)
            {
                AnimalScript animalScript = Instantiate(ResourceScript.Instance.GetAnimalPrefab((AnimalType)i),
                    shipInteriorPen.transform.position, Quaternion.identity, shipInteriorPen.animalTransform);
                shipInteriorPen.AddAnimal(animalScript);
            }
        }
    }

    public void InitializeShip(ShipScript shipInfo)
    {
        transform.position = shipInfo.position;
        transform.rotation = Quaternion.Euler(0, shipInfo.orientation, 0);
        shipInterior.inventoryScript = shipInfo.shipInterior.inventoryScript;

        ((TavernScript)shipInterior.constructionList[1]).recipeList = ((TavernScript)shipInfo.shipInterior.constructionList[1]).recipeList;

        foreach (PeasantScript peasantInfo in shipInfo.shipInterior.peasantList)
        {
            PeasantScript peasantScript = Instantiate(ResourceScript.Instance.GetPeasantPrefab(peasantInfo.peasantType, peasantInfo.peasantGender),
                peasantInfo.position, Quaternion.Euler(0, peasantInfo.orientation, 0), shipInterior.npcsTransform);
            peasantScript.islandScript = shipInterior;
            peasantScript.InitializePeasant(peasantInfo);
            shipInterior.peasantList.Add(peasantScript);
        }
        peasantsInside = shipInterior.peasantList.Count;

        foreach (AnimalScript animalInfo in shipInfo.shipInteriorPen.animalList)
        {
            AnimalScript animalScript = Instantiate(ResourceScript.Instance.GetAnimalPrefab(animalInfo.animalType),
                shipInteriorPen.transform.position, Quaternion.Euler(0, animalInfo.orientation, 0), shipInteriorPen.animalTransform);
            animalScript.InitializeAnimal(animalInfo);
            shipInteriorPen.AddAnimal(animalScript);
        }
    }

    void Update()
    {
        position = transform.position;
        orientation = (int)transform.rotation.eulerAngles.y % 360;
    }

    public bool FindEntryPosition(float distanceToDock)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, distanceToDock, NavMesh.AllAreas))
        {
            entry.position = hit.position;
            return true;
        }
        return false;
    }

    public override PeasantScript RemovePeasant()
    {
        PeasantScript peasantScript;
        if (peasantList.Count > 0) //Encara hi ha NPCs a l'illa
        {
            peasantScript = peasantList[0];
            peasantList.RemoveAt(0);
        }
        else
        {
            peasantScript = shipInterior.peasantList[0];
            shipInterior.peasantList.RemoveAt(0);
            peasantsInside--;

            peasantScript.transform.parent = islandScript.npcsTransform;
            peasantScript.navMeshAgent.Warp(entry.position);
            peasantScript.islandScript = islandScript;
        }
        peasantScript.constructionScript = null;

        return peasantScript;
    }

    public override PeasantScript PeasantHasArrived(PeasantScript peasantScript)
    {
        peasantList.Remove(peasantScript);
        islandScript.peasantList.Remove(peasantScript);
        shipInterior.peasantList.Add(peasantScript);

        peasantScript.transform.parent = shipInterior.npcsTransform;
        peasantScript.navMeshAgent.Warp(shipInteriorPen.transform.position);
        peasantScript.islandScript = shipInterior;
        peasantScript.constructionScript = shipInteriorPen;

        return base.PeasantHasArrived(peasantScript);
    }

    public override void FinishUpBusiness()
    {
        return;
    }

    public void ToggleFishing()
    {
        fishingScript.enabled = !fishingScript.enabled;
    }

    public void CollectBooty(InventoryScript boxInventoryScript)
    {
        for(int i = 0; i < Enum.GetValues(typeof(MaterialType)).Length; i++)
        {
            shipInterior.inventoryScript.AddResource(ResourceType.Material, i, boxInventoryScript.GetResourceAmount(ResourceType.Material, i));
        }
        for (int i = 0; i < Enum.GetValues(typeof(CropType)).Length; i++)
        {
            shipInterior.inventoryScript.AddResource(ResourceType.Crop, i, boxInventoryScript.GetResourceAmount(ResourceType.Crop, i));
        }
        for (int i = 0; i < Enum.GetValues(typeof(MeatType)).Length; i++)
        {
            shipInterior.inventoryScript.AddResource(ResourceType.Meat, i, boxInventoryScript.GetResourceAmount(ResourceType.Meat, i));
        }
    }
}
