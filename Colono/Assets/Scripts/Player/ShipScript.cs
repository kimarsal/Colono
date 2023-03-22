using Newtonsoft.Json;
using System;
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

    public InventoryScript inventoryScript;

    [JsonIgnore] public Transform nextIslandTransform;
    public override bool canBeRemoved { get { return false; } }
    public override int peasantCount { get { return peasantList.Count + shipInterior.peasantList.Count; } }

    public override EditorScript editorScript { get { return CanvasScript.Instance.inventoryEditor; } }

    private void Awake()
    {
        Instance = this;
    }

    public void AddDefaultElements()
    {
        for (int i = 0; i < 15; i++)
        {
            PeasantScript.PeasantType peasantType = (PeasantScript.PeasantType)Random.Range(0, 2);
            PeasantScript.PeasantGender peasantGender = (PeasantScript.PeasantGender)Random.Range(0, 2);
            PeasantScript peasantScript = Instantiate(IslandEditor.Instance.GetNPCPrefab(peasantType, peasantGender),
            shipInteriorPen.transform.position, Quaternion.identity, shipInterior.npcsTransform);
            
            peasantScript.islandScript = shipInterior;
            peasantScript.isNative = false;
            peasantScript.headType = Random.Range(0, 2);
            peasantScript._SKINCOLOR = IslandEditor.Instance.GetRandomSkinColor();
            peasantScript._HAIRCOLOR = IslandEditor.Instance.GetRandomHairColor();
            peasantScript._CLOTH3COLOR = Random.ColorHSV();
            peasantScript._CLOTH4COLOR = Random.ColorHSV();
            peasantScript._OTHERCOLOR = Random.ColorHSV();

            shipInterior.peasantList.Add(peasantScript);
        }

        int animalTypes = Enum.GetValues(typeof(AnimalType)).Length;
        for (int i = 0; i < animalTypes; i++)
        {
            AnimalScript animalScript = Instantiate(IslandEditor.Instance.GetAnimalPrefab((AnimalType)i),
                shipInteriorPen.transform.position, Quaternion.identity, shipInteriorPen.animalTransform).GetComponent<AnimalScript>();
            shipInteriorPen.AddAnimal(animalScript);
        }

        inventoryScript.AddCapacityToAllCategories();
        inventoryScript.AddResource(ResourceType.Crop, (int)CropType.Onion, 2);
        inventoryScript.AddResource(ResourceType.Crop, (int)CropType.Carrot, 2);
        inventoryScript.AddResource(ResourceType.Crop, (int)CropType.Cucumber, 2);
        inventoryScript.AddResource(ResourceType.Material, (int)MaterialType.Wood, 10);
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
        if (peasantList.Count != 0)
        {
            peasantScript = peasantList[0];
            peasantList.RemoveAt(0);
            peasantsOnTheirWay--;
        }
        else
        {
            peasantScript = shipInterior.peasantList[0];
            shipInterior.peasantList.RemoveAt(0);

            peasantScript.transform.parent = GameManager.Instance.closestIsland.npcsTransform;
            peasantScript.navMeshAgent.Warp(entry.position);
            peasantScript.islandScript = GameManager.Instance.closestIsland;
        }

        return peasantScript;
    }

    public override PeasantScript PeasantHasArrived(PeasantScript peasantScript)
    {
        peasantList.Remove(peasantScript);

        peasantScript.transform.parent = shipInterior.npcsTransform;
        peasantScript.navMeshAgent.Warp(shipInteriorPen.transform.position);
        peasantScript.islandScript = shipInterior;
        peasantScript.constructionScript = shipInteriorPen;

        shipInterior.peasantList.Add(peasantScript);

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
            inventoryScript.AddResource(ResourceType.Material, i, boxInventoryScript.GetResourceAmount(ResourceType.Material, i));
        }
        for (int i = 0; i < Enum.GetValues(typeof(CropType)).Length; i++)
        {
            inventoryScript.AddResource(ResourceType.Crop, i, boxInventoryScript.GetResourceAmount(ResourceType.Crop, i));
        }
        for (int i = 0; i < Enum.GetValues(typeof(MeatType)).Length; i++)
        {
            inventoryScript.AddResource(ResourceType.Meat, i, boxInventoryScript.GetResourceAmount(ResourceType.Meat, i));
        }
    }
}
