using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    public ShipScript shipScript;
    public int numPeasants = 10;
    public Transform cellsTransform;

    public enum ButtonState { Idle, ItemButtons, BuildingButtons, EnclosureButtons, ConstructionDetails, PeasantDetails, PopUp };

    private bool isPlayerNearIsland = false;
    public bool isInIsland = false;

    public ButtonState buttonState = ButtonState.Idle;
    public Animator canvasAnimator;
    public Animator itemButtonsAnimator;
    public Animator buildingButtonsAnimator;
    public Animator enclosureButtonsAnimator;
    public Animator constructionDetailsAnimator;
    public Animator peasantDetailsAnimator;
    public Animator gardenEditorAnimator;
    public Animator tavernEditorAnimator;
    public Animator manageInventoryAnimator;

    private ConstructionScript constructionScript;
    public ConstructionDetailsScript constructionDetailsScript;
    public PeasantDetailsScript peasantDetailsScript;
    public GardenEditor gardenEditorScript;
    public TavernEditor tavernEditorScript;
    public ManageInventoryScript manageInventoryScript;

    public Button boardIslandButton;
    public Button cancelClearingButton;
    public Button removeEnclosureButton;
    public Button removeBuildingButton;
    
    public IslandScript islandScript;
    private CameraScript cameraScript;

    public bool hasSelectedPeasant;

    private void Start()
    {
        cellsTransform = transform.GetChild(0);
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
        switch (buttonState)
        {
            case ButtonState.ConstructionDetails: constructionDetailsAnimator.Play("HideDetails"); break;
            case ButtonState.PeasantDetails: peasantDetailsAnimator.Play("HideDetails"); break;
        }

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
        hasSelectedPeasant = true;
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

    public void ShowGardenEditor()
    {
        constructionDetailsAnimator.Play("HideDetails");

        gardenEditorScript.gameObject.SetActive(true);
        gardenEditorScript.gardenScript = (GardenScript)constructionScript;
        gardenEditorScript.SetGrid();
        gardenEditorAnimator.Play("ShowPopUp");

        buttonState = ButtonState.PopUp;
    }

    public void HideGardenEditor()
    {
        gardenEditorAnimator.Play("HidePopUp");
        constructionDetailsAnimator.Play("ShowDetails");

        buttonState = ButtonState.ConstructionDetails;
    }

    public void ShowTavernEditor()
    {
        constructionDetailsAnimator.Play("HideDetails");

        tavernEditorScript.gameObject.SetActive(true);
        tavernEditorScript.tavernScript = (TavernScript)constructionScript;
        tavernEditorScript.SetList();
        tavernEditorAnimator.Play("ShowPopUp");

        buttonState = ButtonState.PopUp;
    }

    public void HideTavernEditor()
    {
        tavernEditorAnimator.Play("HidePopUp");
        constructionDetailsAnimator.Play("ShowDetails");

        buttonState = ButtonState.ConstructionDetails;
    }

    public void ShowManageInventory()
    {
        constructionScript.constructionDetailsScript = null;
        constructionDetailsAnimator.Play("HideDetails");

        manageInventoryScript.gameObject.SetActive(true);
        manageInventoryScript.islandScript = islandScript;
        manageInventoryScript.SetGrid();
        manageInventoryAnimator.Play("ShowPopUp");

        buttonState = ButtonState.PopUp;
    }

    public void UpdateMaterial(ResourceScript.MaterialType materialType)
    {
        manageInventoryScript.UpdateMaterial(materialType);
    }

    public void UpdateCrop(ResourceScript.CropType cropType)
    {
        manageInventoryScript.UpdateCrop(cropType);
    }

    public void HideManageInventory()
    {
        manageInventoryAnimator.Play("HidePopUp");
        ShowButtons();
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
