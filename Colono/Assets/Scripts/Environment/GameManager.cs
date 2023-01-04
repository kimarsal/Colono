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
    private ConstructionScript constructionScript;

    private bool isPlayerNearIsland = false;
    public bool isInIsland = false;

    public Transform cellsTransform;
    public IslandScript islandScript;

    public bool hasSelectedPeasant;

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
        canvasScript.BoardIsland();

        shipScript.islandScript = islandScript;
    }

    public void ChangeSelectedItemsState(bool toClear)
    {
        islandScript.islandCellScript.ChangeSelectedItemsState(toClear);
        islandScript.islandCellScript.DestroyAllCells();

        canvasScript.UnselectArea();
    }

    public void SelectShip()
    {
        islandScript.islandCellScript.DestroyAllCells();

        SelectConstruction(shipScript);
    }

    public void SelectPeasant(PeasantScript peasantScript)
    {
        hasSelectedPeasant = true;
        islandScript.islandCellScript.DestroyAllCells();

        canvasScript.ShowPeasantDetails(peasantScript);
    }

    public void ChooseBuilding(int type)
    {
        islandScript.islandCellScript.ChooseBuilding((BuildingScript.BuildingType)type);
        canvasScript.ChooseBuilding();
    }

    public void RemoveConstruction()
    {
        islandScript.npcManager.SendAllPeasantsBack(constructionScript);
        islandScript.islandCellScript.RemoveConstruction(constructionScript);
        canvasScript.HideConstructionDetails();
    }

    public void CreateEnclosure(int enclosureType)
    {
        islandScript.islandCellScript.CreateEnclosure((EnclosureScript.EnclosureType) enclosureType);
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
        islandScript.islandCellScript.DestroyAllCells();
        canvasScript.HideConstructionDetails();
    }

    public void LeaveIsland()
    {
        isInIsland = false;
        cameraScript.ResetPlayerCamera();
        canvasScript.LeaveIsland();

        islandScript.npcManager.CancelAllTripsToShip(shipScript);
        islandScript.convexColliders.SetActive(true);
        islandScript.islandCellScript.DestroyAllCells();
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
