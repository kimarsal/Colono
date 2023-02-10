using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System;
using System.Net.Sockets;
using UnityEngine.UI;
using static Terrain;
using static UnityEngine.Rendering.DebugUI.Table;
using System.Runtime.CompilerServices;

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
    public IslandCellScript islandCellScript;
    private IslandManager islandManager;

    public bool canSelectIslandElements;
    private PeasantScript hoveredPeasant;
    private PeasantScript selectedPeasant;
    private ConstructionScript hoveredConstruction;
    private ConstructionScript selectedConstruction;

    private void Start()
    {
        islandEditor = GetComponent<IslandEditor>();
        islandCellScript = GetComponent<IslandCellScript>();
        islandCellScript.gameManager = this;
        islandCellScript.enabled = false;

        islandManager = GetComponent<IslandManager>();
        shipScript.gameManager = this;
        //LoadGame();
        StartGame();

        inventoryEditor.shipInventoryScript = shipScript.inventoryScript;
    }

    private void Update()
    {
        if (canSelectIslandElements)
        {
            RaycastHit raycastHit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out raycastHit, 100f))
            {
                return;
            }

            if (raycastHit.transform.gameObject.CompareTag("NPC"))
            {
                PeasantScript peasantScript = raycastHit.transform.GetComponent<PeasantScript>();
                if(peasantScript != hoveredPeasant)
                {
                    if (hoveredPeasant != null && hoveredPeasant != selectedPeasant) hoveredPeasant.outline.enabled = false;
                    hoveredPeasant = peasantScript;
                    peasantScript.outline.enabled = true;
                }
                if (Input.GetMouseButtonDown(0))
                {
                    SelectPeasant(peasantScript);
                }
            }
            else
            {
                if (hoveredPeasant != null && hoveredPeasant != selectedPeasant)
                {
                    hoveredPeasant.outline.enabled = false;
                    hoveredPeasant = null;
                }
            }

            if (raycastHit.transform.gameObject.CompareTag("Construction"))
            {
                ConstructionScript constructionScript = raycastHit.transform.GetComponentInParent<ConstructionScript>();
                if (constructionScript != hoveredConstruction)
                {
                    if (hoveredConstruction != null && hoveredConstruction != selectedConstruction) hoveredConstruction.outline.enabled = false;
                    hoveredConstruction = constructionScript;
                    constructionScript.outline.enabled = true;
                }
                if (Input.GetMouseButtonDown(0))
                {
                    SelectConstruction(constructionScript);
                }
            }
            else if (raycastHit.transform.gameObject.CompareTag("Player"))
            {
                ConstructionScript constructionScript = raycastHit.transform.GetComponent<ConstructionScript>();
                if (constructionScript != hoveredConstruction)
                {
                    if (hoveredConstruction != null && hoveredConstruction != selectedConstruction) hoveredConstruction.outline.enabled = false;
                    hoveredConstruction = constructionScript;
                    constructionScript.outline.enabled = true;
                }
                if (Input.GetMouseButtonDown(0))
                {
                    SelectConstruction(constructionScript);
                }
            }
            else
            {
                if (hoveredConstruction != null && hoveredConstruction != selectedConstruction)
                {
                    hoveredConstruction.outline.enabled = false;
                    hoveredConstruction = null;
                }
            }
        }
    }

    public void PlayerIsNearIsland()
    {
        if (!isPlayerNearIsland)
        {
            isPlayerNearIsland = true;
            canvasScript.PlayerIsNearIsland(closestIsland);
        }
    }

    public void BoardIsland()
    {
        closestIsland.convexColliders.SetActive(false);
        cameraScript.SetIslandCamera(closestIsland.transform.position);
        inventoryEditor.islandInventoryScript = closestIsland.inventoryScript;
        canvasScript.BoardIsland();

        shipScript.islandScript = closestIsland;
        islandCellScript.islandScript = closestIsland;
        canSelectIslandElements = true;
    }

    public void DisableIslandCellScript()
    {
        islandCellScript.enabled = false;
        canSelectIslandElements = true;
    }

    public void SelectShip()
    {
        SelectConstruction(shipScript);
    }

    public void SelectPeasant(PeasantScript peasantScript)
    {
        if (selectedConstruction != null)
        {
            selectedConstruction.constructionDetailsScript = null;
            selectedConstruction.outline.enabled = false;
            selectedConstruction.outline.OutlineWidth = 2;
        }
        if (selectedPeasant != null)
        {
            selectedPeasant.peasantDetailsScript = null;
            selectedPeasant.outline.enabled = false;
            selectedPeasant.outline.OutlineWidth = 2;
        }
        selectedPeasant = peasantScript;
        selectedPeasant.outline.enabled = true;
        selectedPeasant.outline.OutlineWidth = 8;

        canvasScript.ShowPeasantDetails(peasantScript);
    }

    public void UnselectPeasant()
    {
        selectedPeasant.peasantDetailsScript = null;
        selectedPeasant.outline.enabled = false;
        selectedPeasant.outline.OutlineWidth = 2;
        selectedPeasant = null;
        canvasScript.HidePeasantDetails();
    }

    public void PlantTrees()
    {
        canvasScript.itemButtonsAnimator.Play("HideWholeTab");
        islandCellScript.enabled = true;
        islandCellScript.selectFunction = IslandCellScript.SelectFunction.PlantTrees;
        islandCellScript.selectMode = IslandCellScript.SelectMode.None;
        canSelectIslandElements = false;
    }

    public void ClearItems()
    {
        canvasScript.itemButtonsAnimator.Play("HideWholeTab");
        islandCellScript.enabled = true;
        islandCellScript.selectFunction = IslandCellScript.SelectFunction.ClearItems;
        islandCellScript.selectMode = IslandCellScript.SelectMode.None;
        canSelectIslandElements = false;
    }

    public void CancelItemClearing()
    {
        canvasScript.itemButtonsAnimator.Play("HideWholeTab");
        islandCellScript.enabled = true;
        islandCellScript.selectFunction = IslandCellScript.SelectFunction.CancelItemClearing;
        islandCellScript.selectMode = IslandCellScript.SelectMode.None;
        canSelectIslandElements = false;
    }

    public void ChooseBuilding(int buildingType)
    {
        islandCellScript.enabled = true;
        islandCellScript.ChooseBuilding((BuildingScript.BuildingType)buildingType);
        canvasScript.ChooseBuilding();
        canSelectIslandElements = false;
    }

    public void RemoveConstruction()
    {
        selectedConstruction.FinishUpBusiness();
        closestIsland.SendAllPeasantsBack(selectedConstruction);
        closestIsland.RemoveConstruction(selectedConstruction);
        canvasScript.HideConstructionDetails();
    }

    public void ChooseEnclosure(int enclosureType)
    {
        islandCellScript.enabled = true;
        islandCellScript.ChooseEnclosure((EnclosureScript.EnclosureType)enclosureType);
        canvasScript.ChooseEnclosure();
        canSelectIslandElements = false;
    }

    public void SelectConstruction(ConstructionScript newConstructionScript)
    {
        if (selectedPeasant != null)
        {
            selectedPeasant.peasantDetailsScript = null;
            selectedPeasant.outline.enabled = false;
            selectedPeasant.outline.OutlineWidth = 2;
        }
        if (selectedConstruction != null)
        {
            selectedConstruction.constructionDetailsScript = null;
            selectedConstruction.outline.enabled = false;
            selectedConstruction.outline.OutlineWidth = 2;
        }
        selectedConstruction = newConstructionScript;
        selectedConstruction.outline.enabled = true;
        selectedConstruction.outline.OutlineWidth = 8;

        canvasScript.ShowConstructionDetails(selectedConstruction);
    }

    public void UnselectConstrucion()
    {
        selectedConstruction.constructionDetailsScript = null;
        selectedConstruction.outline.enabled = false;
        selectedConstruction.outline.OutlineWidth = 2;
        selectedConstruction = null;
        canvasScript.HideConstructionDetails();
    }

    public void LeaveIsland()
    {
        cameraScript.ResetPlayerCamera();
        canvasScript.LeaveIsland();

        closestIsland.CancelAllTripsToShip(shipScript);
        closestIsland.convexColliders.SetActive(true);
        islandCellScript.DestroyAllCells();
        islandCellScript.enabled = false;
        canSelectIslandElements = false;
    }

    public void PlayerIsFarFromIsland()
    {
        if (isPlayerNearIsland)
        {
            canvasScript.PlayerIsFarFromIsland();
            isPlayerNearIsland = false;
        }
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
        islandManager.TryToGenerateIsland();
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