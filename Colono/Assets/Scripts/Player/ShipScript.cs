using Newtonsoft.Json;
using System;
using UnityEngine;
using UnityEngine.AI;
using static ResourceScript;
using Random = UnityEngine.Random;

public class ShipScript : ConstructionScript
{
    [JsonIgnore] public GameManager gameManager;
    [JsonIgnore] public override IslandEditor islandEditor { get { return gameManager.islandEditor; } }

    public Vector3 position;
    public int orientation;
    public IslandScript shipInterior;
    public PenScript shipInteriorPen;

    public InventoryScript inventoryScript;

    [JsonIgnore] public float distanceToBoardIsland = 2f;
    [JsonIgnore] public override bool canBeRemoved { get { return false; } }

    [JsonIgnore] public override int peasantCount { get { return peasantList.Count + shipInterior.peasantList.Count; } }

    public void AddDefaultElements()
    {
        for (int i = 0; i < 1; i++)
        {
            PeasantScript.PeasantType peasantType = (PeasantScript.PeasantType)Random.Range(0, 2);
            PeasantScript.PeasantGender peasantGender = (PeasantScript.PeasantGender)Random.Range(0, 2);
            PeasantScript peasantScript = Instantiate(islandEditor.GetNPCPrefab(peasantType, peasantGender),
                shipInteriorPen.transform.position, Quaternion.identity, shipInterior.npcsTransform).GetComponent<PeasantScript>();
            
            peasantScript.islandScript = shipInterior;
            peasantScript.isNative = false;
            peasantScript.headType = Random.Range(0, 2);
            peasantScript._SKINCOLOR = islandEditor.GetRandomSkinColor();
            peasantScript._HAIRCOLOR = islandEditor.GetRandomHairColor();
            peasantScript._CLOTH3COLOR = Random.ColorHSV();
            peasantScript._CLOTH4COLOR = Random.ColorHSV();
            peasantScript._OTHERCOLOR = Random.ColorHSV();

            shipInterior.peasantList.Add(peasantScript);
        }

        int animalTypes = Enum.GetValues(typeof(AnimalType)).Length;
        for (int i = 0; i < animalTypes; i++)
        {
            AnimalScript animalScript = Instantiate(islandEditor.GetAnimalPrefab((AnimalType)i),
                shipInteriorPen.transform.position, Quaternion.identity, shipInteriorPen.animalTransform).GetComponent<AnimalScript>();
            shipInteriorPen.AddAnimal(animalScript);
        }

        inventoryScript.capacity = 30;
        inventoryScript.AddResource(ResourceType.Crop, (int)CropType.Onion, 2);
        inventoryScript.AddResource(ResourceType.Crop, (int)CropType.Carrot, 2);
        inventoryScript.AddResource(ResourceType.Crop, (int)CropType.Cucumber, 2);
    }

    void Update()
    {
        position = transform.position;
        orientation = (int)transform.rotation.y % 360;

        if (gameManager.closestIsland == null)
        {
            gameManager.PlayerIsFarFromIsland();
            return;
        }

        float minDistance = 10;

        /*
        bool closeToIsland = false;
        Vector3 colliderClosestPoint = new Vector3();
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

        }*/

        if (Vector3.Distance(transform.position, gameManager.closestIsland.transform.position) < minDistance)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 15, NavMesh.AllAreas))
            {
                entry.position = hit.position;
            }
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
            PeasantScript peasantInShip = shipInterior.peasantList[0];
            shipInterior.peasantList.RemoveAt(0);

            peasantScript = Instantiate(peasantInShip.gameObject, entry.position, Quaternion.identity, gameManager.closestIsland.npcsTransform).GetComponent<PeasantScript>();
            peasantScript.islandScript = gameManager.closestIsland;
            peasantScript.InitializePeasant(peasantInShip);

            Destroy(peasantInShip.gameObject);
        }

        return peasantScript;
    }

    public override PeasantScript PeasantHasArrived(PeasantScript peasantScript)
    {
        peasantList.Remove(peasantScript);

        PeasantScript peasantInShip = Instantiate(peasantScript.gameObject, shipInteriorPen.transform.position,
            Quaternion.identity, shipInterior.npcsTransform).GetComponent<PeasantScript>();
        peasantInShip.islandScript = shipInterior;
        peasantInShip.constructionScript = shipInteriorPen;
        peasantInShip.InitializePeasant(peasantInShip);

        shipInterior.peasantList.Add(peasantInShip);

        Destroy(peasantScript.gameObject);

        return base.PeasantHasArrived(peasantInShip);
    }

    public override void FinishUpBusiness()
    {
        return;
    }
}
