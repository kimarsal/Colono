using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class GameManager : MonoBehaviour
{
    public ShipScript shipScript;
    public CameraScript cameraScript;
    public CanvasScript canvasScript;
    public InventoryEditor inventoryEditor;

    private bool isPlayerNearIsland = false;
    public bool isInIsland = false;

    public Transform cellsTransform;
    public IslandScript closestIsland;
    public IslandEditor islandEditor;
    private IslandSelectionScript islandSelectionScript;
    public IslandCellScript islandCellScript;
    private IslandManager islandManager;
    public PenScript buildingInterior;

    private void Start()
    {
        islandEditor = GetComponent<IslandEditor>();

        islandCellScript = GetComponent<IslandCellScript>();
        islandCellScript.gameManager = this;
        islandCellScript.enabled = false;

        islandSelectionScript = GetComponent<IslandSelectionScript>();
        islandSelectionScript.enabled = false;

        islandManager = GetComponent<IslandManager>();
        //LoadGame();
        StartGame();

        inventoryEditor.shipInventoryScript = shipScript.inventoryScript;
    }

    public void PlayerIsNearIsland()
    {
        if (!isPlayerNearIsland)
        {
            isPlayerNearIsland = true;
            canvasScript.PlayerIsNearIsland(closestIsland);
        }
    }

    public void PlayerIsFarFromIsland()
    {
        if (isPlayerNearIsland)
        {
            canvasScript.PlayerIsFarFromIsland();
            isPlayerNearIsland = false;
        }
    }

    public void BoardIsland()
    {
        //closestIsland.convexColliders.SetActive(false);
        cameraScript.SetIslandCamera(closestIsland.transform.position);
        inventoryEditor.islandInventoryScript = closestIsland.inventoryScript;
        canvasScript.BoardIsland();

        shipScript.islandScript = closestIsland;
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
        canvasScript.ShowPeasantDetails(peasantScript);
    }

    public void UnselectPeasant()
    {
        islandSelectionScript.UnselectPeasant();
        canvasScript.HidePeasantDetails();
    }

    public void PlantTrees()
    {
        canvasScript.itemButtonsAnimator.Play("HideWholeTab");
        islandCellScript.enabled = true;
        islandCellScript.selectFunction = IslandCellScript.SelectFunction.PlantTrees;
        islandCellScript.selectMode = IslandCellScript.SelectMode.None;
        islandSelectionScript.enabled = false;
    }

    public void ClearItems()
    {
        canvasScript.itemButtonsAnimator.Play("HideWholeTab");
        islandCellScript.enabled = true;
        islandCellScript.selectFunction = IslandCellScript.SelectFunction.ClearItems;
        islandCellScript.selectMode = IslandCellScript.SelectMode.None;
        islandSelectionScript.enabled = false;
    }

    public void CancelItemClearing()
    {
        canvasScript.itemButtonsAnimator.Play("HideWholeTab");
        islandCellScript.enabled = true;
        islandCellScript.selectFunction = IslandCellScript.SelectFunction.CancelItemClearing;
        islandCellScript.selectMode = IslandCellScript.SelectMode.None;
        islandSelectionScript.enabled = false;
    }

    public void ChooseEnclosure(int enclosureType)
    {
        islandCellScript.enabled = true;
        islandCellScript.ChooseEnclosure((EnclosureScript.EnclosureType)enclosureType);
        canvasScript.ChooseEnclosure();
        islandSelectionScript.enabled = false;
    }

    public void ChooseBuilding(int buildingType)
    {
        islandCellScript.enabled = true;
        islandCellScript.ChooseBuilding((BuildingScript.BuildingType)buildingType);
        canvasScript.ChooseBuilding();
        islandSelectionScript.enabled = false;
    }

    public void RemoveConstruction()
    {
        ConstructionScript constructionScript = islandSelectionScript.selectedConstruction;
        constructionScript.FinishUpBusiness();
        closestIsland.RemoveConstruction(constructionScript);
        canvasScript.HideConstructionDetails();
    }

    public void SelectConstruction(ConstructionScript constructionScript)
    {
        islandSelectionScript.selectedConstruction = constructionScript;
        canvasScript.ShowConstructionDetails(constructionScript);
    }

    public void UnselectConstrucion()
    {
        islandSelectionScript.UnselectConstruction();
        canvasScript.HideConstructionDetails();
    }

    public void LeaveIsland()
    {
        cameraScript.ResetPlayerCamera();
        canvasScript.LeaveIsland();

        islandSelectionScript.enabled = false;
        islandCellScript.enabled = false;

        closestIsland.CancelAllTripsToShip(shipScript);
        //closestIsland.convexColliders.SetActive(true);
        islandSelectionScript.enabled = false;
    }

    public void SaveGame()
    {
        IslandManager islandManager = GetComponent<IslandManager>();

        GameInfo gameInfo = new GameInfo();
        gameInfo.seed = islandManager.seed;
        gameInfo.shipScript = shipScript;
        gameInfo.islandList = islandManager.islandList;

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

        shipScript.transform.position = gameInfo.shipScript.position;
        shipScript.transform.rotation = Quaternion.Euler(0, gameInfo.shipScript.orientation, 0);
        islandManager.seed = gameInfo.seed;
        GetComponent<IslandGenerator>().LoadIslands(gameInfo.islandList);
    }

    private void StartGame()
    {
        islandManager.GenerateIslandCoroutine(new Vector2(0, 20));

        shipScript.AddDefaultElements();
    }
}

[System.Serializable]
public class GameInfo
{
    public int seed;
    public ShipScript shipScript;
    public List<IslandScript> islandList = new List<IslandScript>();
}