using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public Transform islandsTransform;
    public List<IslandScript> islandList = new List<IslandScript>();

    private IslandGenerator islandGenerator;
    private CameraScript cameraScript;
    public InventoryEditor inventoryEditor;

    private bool isPlayerNearIsland = false;
    public bool isInIsland = false;

    public float distanceToDock = 10f;
    public float distanceToCheckIfCanDock = 50f;
    public float distanceBetweenIslands = 200f;

    public Transform cellsTransform;
    public IslandScript closestIsland;
    private IslandSelectionScript islandSelectionScript;
    public IslandCellScript islandCellScript;
    public PenScript buildingInterior;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        islandCellScript = GetComponent<IslandCellScript>();
        islandCellScript.enabled = false;

        islandSelectionScript = GetComponent<IslandSelectionScript>();
        islandSelectionScript.enabled = false;

        islandGenerator = GetComponent<IslandGenerator>();
        cameraScript = Camera.main.GetComponent<CameraScript>();
        //LoadGame();
        StartGame();

        inventoryEditor.shipInventoryScript = ShipScript.Instance.inventoryScript;
    }

    private void Update()
    {
        Vector3 shipPosition = ShipScript.Instance.transform.position;

        FindClosestIsland(shipPosition);

        FindClosestPointInClosestIsland(shipPosition);
    }

    private void FindClosestIsland(Vector3 shipPosition)
    {
        float minDistanceToIsland = -1;
        Vector3 nextIslandPosition = ShipScript.Instance.nextIslandTransform.position;
        float minDistanceToNextIsland = -1;
        IslandScript c = null;

        foreach (IslandScript islandScript in islandList)
        {
            float distanceToIsland = Vector3.Distance(islandScript.transform.position, shipPosition);
            if (c == null || distanceToIsland < minDistanceToIsland)
            {
                minDistanceToIsland = distanceToIsland;
                c = islandScript;
            }

            float distanceToNextIsland = Vector3.Distance(islandScript.transform.position, nextIslandPosition);
            if (minDistanceToNextIsland == -1 || distanceToNextIsland < minDistanceToNextIsland)
            {
                minDistanceToNextIsland = distanceToNextIsland;
            }
        }

        if (minDistanceToNextIsland > distanceBetweenIslands)
        {
            islandGenerator.GenerateIsland(new Vector2(nextIslandPosition.x, nextIslandPosition.z));
        }

        closestIsland = c;
    }

    private void FindClosestPointInClosestIsland(Vector3 shipPosition)
    {
        if (closestIsland == null || Vector3.Distance(shipPosition, closestIsland.transform.position) > distanceToCheckIfCanDock)
        {
            PlayerIsFarFromIsland();
            return;
        }

        if (ShipScript.Instance.FindEntryPosition(distanceToDock))
        {
            PlayerIsNearIsland();
        }
        else
        {
            PlayerIsFarFromIsland();
        }
    }

    private void PlayerIsNearIsland()
    {
        if (!isPlayerNearIsland)
        {
            isPlayerNearIsland = true;
            CanvasScript.Instance.PlayerIsNearIsland(closestIsland);
        }
    }

    private void PlayerIsFarFromIsland()
    {
        if (isPlayerNearIsland)
        {
            CanvasScript.Instance.PlayerIsFarFromIsland();
            isPlayerNearIsland = false;
        }
    }

    public void DockOntoIsland()
    {
        cameraScript.SetIslandCamera(closestIsland.transform.position);
        inventoryEditor.islandInventoryScript = closestIsland.inventoryScript;
        CanvasScript.Instance.Dock();

        ShipScript.Instance.islandScript = closestIsland;
        islandCellScript.islandScript = closestIsland;
        islandSelectionScript.enabled = true;
    }

    public void DisableIslandCellScript()
    {
        islandCellScript.DestroyAllCells();
        islandCellScript.enabled = false;
        islandSelectionScript.enabled = true;
    }

    public void SelectPeasant(PeasantScript peasantScript)
    {
        CanvasScript.Instance.ShowPeasantDetails(peasantScript);
    }

    public void UnselectPeasant()
    {
        islandSelectionScript.UnselectPeasant();
        CanvasScript.Instance.HidePeasantDetails();
    }

    public void PlantTrees()
    {
        islandCellScript.enabled = true;
        islandCellScript.selectFunction = IslandCellScript.SelectFunction.PlantTrees;
        islandCellScript.selectMode = IslandCellScript.SelectMode.None;
        islandSelectionScript.enabled = false;
    }

    public void ClearItems()
    {
        islandCellScript.enabled = true;
        islandCellScript.selectFunction = IslandCellScript.SelectFunction.ClearItems;
        islandCellScript.selectMode = IslandCellScript.SelectMode.None;
        islandSelectionScript.enabled = false;
    }

    public void CancelItemClearing()
    {
        islandCellScript.enabled = true;
        islandCellScript.selectFunction = IslandCellScript.SelectFunction.CancelItemClearing;
        islandCellScript.selectMode = IslandCellScript.SelectMode.None;
        islandSelectionScript.enabled = false;
    }

    public void ChooseEnclosure(int enclosureType)
    {
        islandCellScript.enabled = true;
        islandCellScript.ChooseEnclosure((EnclosureScript.EnclosureType)enclosureType);
        islandSelectionScript.enabled = false;
    }

    public void ChooseBuilding(int buildingType)
    {
        islandCellScript.enabled = true;
        islandCellScript.ChooseBuilding((BuildingScript.BuildingType)buildingType);
        islandSelectionScript.enabled = false;
    }

    public void RemoveConstruction()
    {
        ConstructionScript constructionScript = islandSelectionScript.selectedConstruction;
        constructionScript.FinishUpBusiness();
        closestIsland.RemoveConstruction(constructionScript);
        CanvasScript.Instance.HideConstructionDetails();
    }

    public void SelectConstruction(ConstructionScript constructionScript)
    {
        islandSelectionScript.selectedConstruction = constructionScript;
        CanvasScript.Instance.ShowConstructionDetails(constructionScript);
    }

    public void UnselectConstrucion()
    {
        islandSelectionScript.UnselectConstruction();
        CanvasScript.Instance.HideConstructionDetails();
    }

    public void Sail()
    {
        cameraScript.ResetPlayerCamera();
        CanvasScript.Instance.Sail();

        islandSelectionScript.enabled = false;
        islandCellScript.enabled = false;

        closestIsland.CancelAllTripsToShip();
        islandSelectionScript.enabled = false;
    }

    public void SaveGame()
    {
        GameInfo gameInfo = new GameInfo();
        gameInfo.seed = islandGenerator.seed;
        gameInfo.shipScript = ShipScript.Instance;
        gameInfo.islandList = islandList;

        string json = JsonConvert.SerializeObject(gameInfo, Formatting.Indented, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });
        File.WriteAllText("gameInfo.json", json);
    }

    public void LoadGame()
    {
        GameInfo gameInfo = JsonConvert.DeserializeObject<GameInfo>(File.ReadAllText("gameInfo.json"), new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });

        ShipScript.Instance.transform.position = gameInfo.shipScript.position;
        ShipScript.Instance.transform.rotation = Quaternion.Euler(0, gameInfo.shipScript.orientation, 0);
        islandGenerator.seed = gameInfo.seed;
        islandGenerator.LoadIslands(gameInfo.islandList);
    }

    private void StartGame()
    {
        islandGenerator.GenerateIsland(new Vector2(0, 40));

        ShipScript.Instance.AddDefaultElements();
    }
}

[System.Serializable]
public class GameInfo
{
    public int seed;
    public ShipScript shipScript;
    public List<IslandScript> islandList = new List<IslandScript>();
}