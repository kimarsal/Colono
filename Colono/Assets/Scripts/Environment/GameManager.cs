using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    public ShipScript shipScript;
    public CameraScript cameraScript;
    public CanvasScript canvasScript;
    public InventoryEditor inventoryEditor;
    private ConstructionScript constructionScript;

    private bool isPlayerNearIsland = false;
    public bool isInIsland = false;

    public Transform cellsTransform;
    public IslandScript islandScript;
    private IslandCellScript islandCellScript;

    public bool hasSelectedPeasant;

    private void Start()
    {
        islandCellScript = GetComponent<IslandCellScript>();
        islandCellScript.enabled = false;
    }

    public void PlayerIsNearIsland(IslandScript islandScript)
    {
        if (!isPlayerNearIsland)
        {
            isPlayerNearIsland = true;
            canvasScript.PlayerIsNearIsland(islandScript);
            this.islandScript = islandScript;
        }
    }

    public void BoardIsland()
    {
        islandScript.convexColliders.SetActive(false);
        cameraScript.SetIslandCamera(islandScript.transform.position);
        inventoryEditor.islandInventoryScript = islandScript.inventoryScript;
        canvasScript.BoardIsland();

        shipScript.islandScript = islandScript;

        islandCellScript.enabled = true;
        islandCellScript.islandScript = islandScript;
    }

    public void ChangeSelectedItemsState(bool toClear)
    {
        islandCellScript.ChangeSelectedItemsState(toClear);
        islandCellScript.DestroyAllCells();

        canvasScript.UnselectArea();
    }

    public void SelectShip()
    {
        islandCellScript.DestroyAllCells();

        SelectConstruction(shipScript);
    }

    public void SelectPeasant(PeasantScript peasantScript)
    {
        hasSelectedPeasant = true;
        islandCellScript.DestroyAllCells();

        canvasScript.ShowPeasantDetails(peasantScript);
    }

    public void ChooseBuilding(int type)
    {
        islandCellScript.ChooseBuilding((BuildingScript.BuildingType)type);
        canvasScript.ChooseBuilding();
    }

    public void RemoveConstruction()
    {
        constructionScript.FinishUpBusiness();
        islandScript.npcManager.SendAllPeasantsBack(constructionScript);
        islandCellScript.RemoveConstruction(constructionScript);
        canvasScript.HideConstructionDetails();
    }

    public void CreateEnclosure(int enclosureType)
    {
        islandCellScript.CreateEnclosure((EnclosureScript.EnclosureType) enclosureType);
    }

    public void SelectConstruction(ConstructionScript newConstructionScript)
    {
        if(constructionScript != null)
        {
            constructionScript.outline.enabled = false;
        }
        constructionScript = newConstructionScript;
        constructionScript.outline.enabled = true;
        canvasScript.ShowConstructionDetails(constructionScript);
    }

    public void UnselectConstrucion()
    {
        constructionScript.constructionDetailsScript = null;
        constructionScript.outline.enabled = false;
        constructionScript = null;
        islandCellScript.DestroyAllCells();
        canvasScript.HideConstructionDetails();
    }

    public void LeaveIsland()
    {
        cameraScript.ResetPlayerCamera();
        canvasScript.LeaveIsland();

        islandScript.npcManager.CancelAllTripsToShip(shipScript);
        islandScript.convexColliders.SetActive(true);
        islandCellScript.DestroyAllCells();
        islandCellScript.enabled = false;
    }

    public void PlayerIsFarFromIsland()
    {
        if (isPlayerNearIsland)
        {
            canvasScript.PlayerIsFarFromIsland();
            isPlayerNearIsland = false;
        }
    }

    public bool CanSelect()
    {
        return isInIsland && canvasScript.buttonState != CanvasScript.ButtonState.PopUp;
    }

}
