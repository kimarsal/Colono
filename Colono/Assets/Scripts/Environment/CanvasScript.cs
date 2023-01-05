using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasScript : MonoBehaviour
{
    public enum ButtonState { Idle, ItemButtons, BuildingButtons, EnclosureButtons, ConstructionDetails, PeasantDetails, PopUp };
    [Header("Animations")]
    public ButtonState buttonState = ButtonState.Idle;
    private Animator canvasAnimator;
    public Animator itemButtonsAnimator;
    public Animator buildingButtonsAnimator;
    public Animator enclosureButtonsAnimator;
    public Animator constructionDetailsAnimator;
    public Animator peasantDetailsAnimator;
    public Animator gardenEditorAnimator;
    public Animator penEditorAnimator;
    public Animator tavernEditorAnimator;
    public Animator inventoryEditorAnimator;

    [Header("Editors")]
    public ConstructionDetailsScript constructionDetailsScript;
    public PeasantDetailsScript peasantDetailsScript;
    public GardenEditor gardenEditor;
    public InventoryEditor penEditor;
    public TavernEditor tavernEditor;
    public InventoryEditor inventoryEditor;

    [Header("Buttons")]
    public Button boardIslandButton;
    public Button cancelClearingButton;

    private void Start()
    {
        canvasAnimator = GetComponent<Animator>();
    }

    public void PlayerIsNearIsland(IslandScript islandScript)
    {
        boardIslandButton.GetComponentInChildren<TextMeshProUGUI>().text = "Dock onto the island";
        canvasAnimator.Play("GetCloseToIsland");
    }

    public void PlayerIsFarFromIsland()
    {
        canvasAnimator.Play("GetFarFromIsland");
    }

    public void BoardIsland()
    {
        canvasAnimator.Play("BoardIsland");
        buildingButtonsAnimator.Play("ShowBuildingButtons");

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

    public void SelectArea()
    {
        canvasAnimator.Play("ShowLeaveIslandButton");
        enclosureButtonsAnimator.Play("ShowEnclosureButtons");

        buttonState = ButtonState.EnclosureButtons;
    }

    public void UnselectArea()
    {
        if (buttonState == ButtonState.ItemButtons) itemButtonsAnimator.Play("HideItemButtons");
        else enclosureButtonsAnimator.Play("HideEnclosureButtons");
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

    public void Build()
    {
        if (buttonState == ButtonState.Idle)
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

    public void ChooseBuilding()
    {
        canvasAnimator.Play("HideLeaveIslandButton");
        buildingButtonsAnimator.Play("HideAllBuildingButtons");
    }

    public void ShowConstructionDetails(ConstructionScript constructionScript)
    {
        constructionDetailsScript.SetConstructionDetails(constructionScript);

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

    public void HideConstructionDetails()
    {
        constructionDetailsAnimator.Play("HideDetails");
        buildingButtonsAnimator.Play("ShowBuildingButtons");
        canvasAnimator.Play("ShowLeaveIslandButton");

        buttonState = ButtonState.Idle;
    }

    public void ShowInventoryEditor()
    {
        constructionDetailsAnimator.Play("HideDetails");

        inventoryEditor.gameObject.SetActive(true);
        inventoryEditor.islandScript = constructionDetailsScript.constructionScript.islandScript;
        inventoryEditor.penScript = null;
        inventoryEditor.SetGrid();
        inventoryEditorAnimator.Play("ShowPopUp");

        buttonState = ButtonState.PopUp;
    }

    public void HideInventoryEditor()
    {
        inventoryEditorAnimator.Play("HidePopUp");
        ShowButtons();
    }

    public void ShowGardenEditor()
    {
        constructionDetailsAnimator.Play("HideDetails");

        gardenEditor.gameObject.SetActive(true);
        gardenEditor.gardenScript = (GardenScript)constructionDetailsScript.constructionScript;
        gardenEditor.SetGrid();
        gardenEditorAnimator.Play("ShowPopUp");

        buttonState = ButtonState.PopUp;
    }

    public void HideGardenEditor()
    {
        gardenEditorAnimator.Play("HidePopUp");
        constructionDetailsAnimator.Play("ShowDetails");

        buttonState = ButtonState.ConstructionDetails;
    }

    public void ShowPenEditor(PenScript penScript)
    {
        constructionDetailsAnimator.Play("HideDetails");

        penEditor.gameObject.SetActive(true);
        penEditor.islandScript = constructionDetailsScript.constructionScript.islandScript;
        penEditor.penScript = penScript;
        penEditor.SetGrid();
        penEditorAnimator.Play("ShowPopUp");

        buttonState = ButtonState.PopUp;
    }

    public void HidePenEditor()
    {
        penEditorAnimator.Play("HidePopUp");
        constructionDetailsAnimator.Play("ShowDetails");

        buttonState = ButtonState.ConstructionDetails;
    }

    public void ShowTavernEditor()
    {
        constructionDetailsAnimator.Play("HideDetails");

        tavernEditor.gameObject.SetActive(true);
        tavernEditor.tavernScript = (TavernScript)constructionDetailsScript.constructionScript;
        tavernEditor.SetList();
        tavernEditorAnimator.Play("ShowPopUp");

        buttonState = ButtonState.PopUp;
    }

    public void HideTavernEditor()
    {
        tavernEditorAnimator.Play("HidePopUp");
        constructionDetailsAnimator.Play("ShowDetails");

        buttonState = ButtonState.ConstructionDetails;
    }

    public void ShowPeasantDetails(PeasantScript peasantScript)
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

    public void HidePeasantDetails()
    {
        canvasAnimator.Play("ShowLeaveIslandButton");
        buildingButtonsAnimator.Play("ShowBuildingButtons");
        peasantDetailsAnimator.Play("HideDetails");

        peasantDetailsScript.peasantScript.peasantDetailsScript = null;

        buttonState = ButtonState.Idle;
    }

    public void UpdateInventoryRow(ResourceScript.ResourceType resourceType, int resourceIndex)
    {
        inventoryEditor.UpdateInventoryRow(ResourceScript.ResourceType.Material, resourceIndex);
    }

    public void LeaveIsland()
    {
        canvasAnimator.Play("LeaveIsland");
        switch (buttonState)
        {
            case ButtonState.Idle: buildingButtonsAnimator.Play("HideBuildingButtons"); break;
            case ButtonState.ItemButtons: buildingButtonsAnimator.Play("HideItemButtons"); break;
            case ButtonState.BuildingButtons: buildingButtonsAnimator.Play("HideAllBuildingButtons"); break;
            case ButtonState.EnclosureButtons: enclosureButtonsAnimator.Play("HideEnclosureButtons"); break;
        }
    }

}
