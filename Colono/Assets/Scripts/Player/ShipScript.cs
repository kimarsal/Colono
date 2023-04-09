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

    public Vector3 position;
    public int orientation;
    public IslandScript shipInterior;
    public PenScript shipInteriorPen;

    [JsonIgnore] public Transform nextIslandTransform;
    [JsonIgnore] public Transform enemyShipTransform;
    public override bool canBeRemoved { get { return false; } }
    public override int peasantCount { get { return peasantList.Count + shipInterior.peasantList.Count; } }

    public override EditorScript editorScript { get { return CanvasScript.Instance.shipEditor; } }

    private void Awake()
    {
        Instance = this;
    }

    public void AddDefaultElements()
    {
        for (int i = 0; i < 10; i++)
        {
            PeasantScript.PeasantType peasantType = (PeasantScript.PeasantType)Random.Range(0, 2);//PeasantScript.PeasantType.Adult
            PeasantScript.PeasantGender peasantGender = (PeasantScript.PeasantGender)Random.Range(0, 2);
            PeasantScript peasantScript = Instantiate(ResourceScript.Instance.GetNPCPrefab(peasantType, peasantGender),
            shipInteriorPen.transform.position, Quaternion.identity, shipInterior.npcsTransform);
            
            peasantScript.islandScript = shipInterior;
            peasantScript.isNative = false;
            peasantScript.headType = Random.Range(0, 2);
            peasantScript._SKINCOLOR = ResourceScript.Instance.GetRandomSkinColor();
            peasantScript._HAIRCOLOR = ResourceScript.Instance.GetRandomHairColor();
            peasantScript._CLOTH3COLOR = Random.ColorHSV();
            peasantScript._CLOTH4COLOR = Random.ColorHSV();
            peasantScript._OTHERCOLOR = Random.ColorHSV();

            shipInterior.peasantList.Add(peasantScript);
        }

        int animalTypes = Enum.GetValues(typeof(AnimalType)).Length;
        for (int i = 0; i < 2; i++)
        {
            for(int j = 0; j < 1; j++)
            {
                AnimalScript animalScript = Instantiate(ResourceScript.Instance.GetAnimalPrefab((AnimalType)i),
                    shipInteriorPen.transform.position, Quaternion.identity, shipInteriorPen.animalTransform).GetComponent<AnimalScript>();
                shipInteriorPen.AddAnimal(animalScript);
            }
        }

        shipInterior.inventoryScript.AddCapacityToAllCategories();

        for(int i = 0; i < GetEnumLength(ResourceType.Crop) / 2; i++)
        {
            shipInterior.inventoryScript.AddResource(ResourceType.Crop, i, 3);
        }
        for (int i = 0; i < GetEnumLength(ResourceType.Meat); i++)
        {
            shipInterior.inventoryScript.AddResource(ResourceType.Meat, i, 5);
        }
        shipInterior.inventoryScript.AddResource(ResourceType.Material, (int)MaterialType.Wood, 100);

        ((TavernScript)shipInterior.constructionList[1]).recipeList = new List<Recipe> {
            new Recipe(-1, -1, (int)MeatType.Chicken),
            new Recipe(-1, -1, (int)MeatType.Mutton),
            new Recipe(-1, -1, (int)MeatType.Pork),
            new Recipe(-1, -1, (int)MeatType.Cow),
        };
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
            peasantsOnTheirWay--;
        }
        else
        {
            peasantScript = shipInterior.peasantList[0];
            shipInterior.peasantList.RemoveAt(0);

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
