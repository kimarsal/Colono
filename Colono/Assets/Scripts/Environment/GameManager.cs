using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    private ShipScript shipScript;
    public int numPeasants = 10;

    private enum ButtonState { Idle, ItemButtons, BuildingButtons, EnclosureButtons, ConstructionDetails, PeasantDetails };

    private bool isPlayerNearIsland = false;
    public bool isInIsland = false;

    private ButtonState buttonState = ButtonState.Idle;
    public Animator canvasAnimator;
    public Animator itemButtonsAnimator;
    public Animator buildingButtonsAnimator;
    public Animator enclosureButtonsAnimator;
    public Animator constructionDetailsAnimator;
    public Animator peasantDetailsAnimator;

    private ConstructionScript constructionScript;
    public ConstructionDetailsScript constructionDetailsScript;
    public PeasantDetailsScript peasantDetailsScript;

    public Button boardIslandButton;
    public Button cancelClearingButton;
    public Button removeEnclosureButton;
    public Button removeBuildingButton;
    /*public Button plantButton;
    public Button clearPatchButton;*/
    
    public IslandScript islandScript;

    private CameraScript cameraScript;

    private void Start()
    {
        cameraScript = GameObject.Find("MainCamera").GetComponent<CameraScript>();
        shipScript = GameObject.FindGameObjectWithTag("Player").GetComponent<ShipScript>();
        constructionDetailsScript.gameManager = this;
        peasantDetailsScript.gameManager = this;
    }

    public void PlayerIsNearIsland(IslandScript islandScript)
    {
        if (!isPlayerNearIsland)
        {
            isPlayerNearIsland = true;
            boardIslandButton.GetComponentInChildren<TextMeshProUGUI>().text = "Dock onto the island";
            canvasAnimator.Play("GetCloseToIsland");
            this.islandScript = islandScript;
        }
    }

    public void BoardIsland()
    {
        islandScript.PlayerEntered();
        cameraScript.SetIslandCamera(islandScript.transform.position);
        canvasAnimator.Play("BoardIsland");
        buildingButtonsAnimator.Play("ShowBuildingButtons");

        buttonState = ButtonState.Idle;
    }

    public void HideButtons()
    {
        canvasAnimator.Play("HideLeaveIslandButton");
        switch (buttonState)
        {
            case ButtonState.Idle: buildingButtonsAnimator.Play("HideBuildingButtons"); break;
            case ButtonState.ItemButtons: itemButtonsAnimator.Play("HideItemButtons"); break;
            case ButtonState.BuildingButtons: buildingButtonsAnimator.Play("HideAllBuildingButtons"); break;
            case ButtonState.EnclosureButtons: enclosureButtonsAnimator.Play("HideEnclosureButtons"); break;
            case ButtonState.ConstructionDetails: constructionDetailsAnimator.Play("HideDetails"); break;
            case ButtonState.PeasantDetails: peasantDetailsAnimator.Play("HideDetails"); break;
        }

        buttonState = ButtonState.Idle;
    }

    public void ShowButtons()
    {
        canvasAnimator.Play("ShowLeaveIslandButton");
        buildingButtonsAnimator.Play("ShowBuildingButtons");

        buttonState = ButtonState.Idle;
    }

    public void SelectItems(bool areScheduledForClearing)
    {
        cancelClearingButton.gameObject.SetActive(areScheduledForClearing);
        canvasAnimator.Play("ShowLeaveIslandButton");
        itemButtonsAnimator.Play("ShowItemButtons");

        buttonState = ButtonState.ItemButtons;
    }

    public void ChangeSelectedItemsState(bool toClear)
    {
        islandScript.islandCellScript.ChangeSelectedItemsState(toClear);

        UnselectArea();
    }

    public void SelectShip()
    {
        islandScript.islandCellScript.DestroyAllCells();

        removeEnclosureButton.gameObject.SetActive(false);
        removeBuildingButton.gameObject.SetActive(false);

        SetConstructionDetails(shipScript);
    }

    public void SelectPeasant(PeasantScript peasantScript)
    {
        islandScript.islandCellScript.DestroyAllCells();

        SetPeasantDetails(peasantScript);
    }

    public void Build()
    {
        if(buttonState == ButtonState.Idle)
        {
            buildingButtonsAnimator.Play("ShowBuildingOptionsButtons");
            buttonState = ButtonState.BuildingButtons;
        }
        else
        {
            buildingButtonsAnimator.Play("HideBuildingOptionsButtons");
            buttonState = ButtonState.Idle;
        }
    }

    public void ChooseBuilding(int type)
    {
        islandScript.islandCellScript.ChooseBuilding((BuildingScript.BuildingType)type);
        canvasAnimator.Play("HideLeaveIslandButton");
        buildingButtonsAnimator.Play("HideAllBuildingButtons");
    }

    public void SelectBuilding(BuildingScript buildingScript)
    {
        removeEnclosureButton.gameObject.SetActive(false);
        removeBuildingButton.gameObject.SetActive(true);

        SetConstructionDetails(buildingScript);
    }

    public void RemoveBuilding()
    {
        islandScript.npcManager.SendAllPeasantsBack(constructionScript);
        islandScript.islandCellScript.RemoveBuilding();
        canvasAnimator.Play("ShowLeaveIslandButton");
        buildingButtonsAnimator.Play("ShowBuildingButtons");
        constructionDetailsAnimator.Play("HideDetails");

        buttonState = ButtonState.Idle;
    }

    public void SelectArea()
    {
        canvasAnimator.Play("ShowLeaveIslandButton");
        enclosureButtonsAnimator.Play("ShowEnclosureButtons");

        buttonState = ButtonState.EnclosureButtons;
    }

    public void UnselectArea()
    {
        islandScript.islandCellScript.DestroyAllCells();
        if(buttonState==ButtonState.ItemButtons) itemButtonsAnimator.Play("HideItemButtons");
        else enclosureButtonsAnimator.Play("HideEnclosureButtons");
        buildingButtonsAnimator.Play("ShowBuildingButtons");

        buttonState = ButtonState.Idle;
    }

    public void CreateEnclosure(int enclosureType)
    {
        islandScript.islandCellScript.CreateEnclosure((EnclosureScript.EnclosureType) enclosureType);
    }

    public void SelectEnclosure(EnclosureScript enclosureScript)
    {
        removeEnclosureButton.gameObject.SetActive(true);
        removeBuildingButton.gameObject.SetActive(false);

        SetConstructionDetails(enclosureScript);
    }

    public void RemoveEnclosure()
    {
        islandScript.npcManager.SendAllPeasantsBack(constructionScript);
        islandScript.islandCellScript.RemoveEnclosure();
        canvasAnimator.Play("ShowLeaveIslandButton");
        buildingButtonsAnimator.Play("ShowBuildingButtons");
        constructionDetailsAnimator.Play("HideDetails");

        buttonState = ButtonState.Idle;
    }

    public void HideConstructionDetails()
    {
        canvasAnimator.Play("ShowLeaveIslandButton");
        buildingButtonsAnimator.Play("ShowBuildingButtons");
        constructionDetailsAnimator.Play("HideDetails");

        constructionScript.constructionDetailsScript = null;
        islandScript.islandCellScript.DestroyAllCells();

        buttonState = ButtonState.Idle;
    }

    public void HidePeasantDetails()
    {
        canvasAnimator.Play("ShowLeaveIslandButton");
        buildingButtonsAnimator.Play("ShowBuildingButtons");
        peasantDetailsAnimator.Play("HideDetails");

        peasantDetailsScript.peasantScript.peasantDetailsScript = null;

        buttonState = ButtonState.Idle;
    }

    private void SetConstructionDetails(ConstructionScript newConstructionScript)
    {
        constructionScript = newConstructionScript;
        constructionDetailsScript.SetConstructionDetails(newConstructionScript);

        canvasAnimator.Play("HideLeaveIslandButton");
        switch (buttonState)
        {
            case ButtonState.Idle: buildingButtonsAnimator.Play("HideBuildingButtons"); break;
            case ButtonState.ItemButtons: buildingButtonsAnimator.Play("HideItemButtons"); break;
            case ButtonState.BuildingButtons: buildingButtonsAnimator.Play("HideAllBuildingButtons"); break;
            case ButtonState.EnclosureButtons: enclosureButtonsAnimator.Play("HideEnclosureButtons"); break;
            case ButtonState.PeasantDetails: peasantDetailsAnimator.Play("HideDetails"); break;
        }
        constructionDetailsAnimator.Play("ShowDetails");

        buttonState = ButtonState.ConstructionDetails;
    }

    private void SetPeasantDetails(PeasantScript peasantScript)
    {
        peasantDetailsScript.SetPeasantDetails(peasantScript);

        canvasAnimator.Play("HideLeaveIslandButton");
        switch (buttonState)
        {
            case ButtonState.Idle: buildingButtonsAnimator.Play("HideBuildingButtons"); break;
            case ButtonState.ItemButtons: buildingButtonsAnimator.Play("HideItemButtons"); break;
            case ButtonState.BuildingButtons: buildingButtonsAnimator.Play("HideAllBuildingButtons"); break;
            case ButtonState.EnclosureButtons: enclosureButtonsAnimator.Play("HideEnclosureButtons"); break;
            case ButtonState.ConstructionDetails: constructionDetailsAnimator.Play("HideDetails"); break;
        }
        peasantDetailsAnimator.Play("ShowDetails");

        buttonState = ButtonState.ConstructionDetails;
    }

    public void PatchSelected(bool isPatchEmpty)
    {
        /*buildButton.gameObject.SetActive(false);
        chooseBuildingButtons.SetActive(false);

        plantButton.gameObject.SetActive(true);
        if (!isPatchEmpty)
        {
            clearPatchButton.gameObject.SetActive(true);
        }*/
    }

    public void Plant()
    {
        //nearbyIsland.islandCellScript.Plant();
    }

    public void ClearPatch()
    {
        //nearbyIsland.islandCellScript.ClearPatch();
    }

    public void LeaveIsland()
    {
        isInIsland = false;
        cameraScript.ResetPlayerCamera();
        canvasAnimator.Play("LeaveIsland");
        switch (buttonState)
        {
            case ButtonState.Idle: buildingButtonsAnimator.Play("HideBuildingButtons"); break;
            case ButtonState.ItemButtons: buildingButtonsAnimator.Play("HideItemButtons"); break;
            case ButtonState.BuildingButtons: buildingButtonsAnimator.Play("HideAllBuildingButtons"); break;
            case ButtonState.EnclosureButtons: enclosureButtonsAnimator.Play("HideEnclosureButtons"); break;
        }

        islandScript.npcManager.CancelAllTripsToShip(shipScript);
        islandScript.PLayerLeft();
        islandScript.islandCellScript.DestroyAllCells();
    }

    public void PlayerIsFarFromIsland()
    {
        canvasAnimator.Play("GetFarFromIsland");
        isPlayerNearIsland = false;
    }
}
