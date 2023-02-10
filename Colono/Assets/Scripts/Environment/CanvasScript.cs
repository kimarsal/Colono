using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasScript : MonoBehaviour
{
    public enum ButtonState { Idle, ItemButtons, BuildingButtons, EnclosureButtons, Editing, ConstructionDetails, PeasantDetails, PopUp };
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
    public PenEditor penEditor;
    public TavernEditor tavernEditor;
    public InventoryEditor inventoryEditor;

    [Header("Buttons")]
    public Button boardIslandButton;

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
        itemButtonsAnimator.Play("ShowTabHeader");
        enclosureButtonsAnimator.Play("ShowTabHeader");
        buildingButtonsAnimator.Play("ShowTabHeader");

        buttonState = ButtonState.Idle;
    }

    public void ShowDefaultButtons()
    {
        switch (buttonState)
        {
            case ButtonState.ConstructionDetails: constructionDetailsAnimator.Play("HideDetails"); break;
            case ButtonState.PeasantDetails: peasantDetailsAnimator.Play("HideDetails"); break;
        }

        canvasAnimator.Play("ShowLeaveIslandButton");
        itemButtonsAnimator.Play("ShowTabHeader");
        enclosureButtonsAnimator.Play("ShowTabHeader");
        buildingButtonsAnimator.Play("ShowTabHeader");

        buttonState = ButtonState.Idle;
    }

    public void HideTopButtons()
    {
        switch (buttonState)
        {
            case ButtonState.Idle:
                itemButtonsAnimator.Play("HideTabHeader");
                enclosureButtonsAnimator.Play("HideTabHeader");
                buildingButtonsAnimator.Play("HideTabHeader");
                break;
            case ButtonState.ItemButtons:
                itemButtonsAnimator.Play("HideWholeTab");
                enclosureButtonsAnimator.Play("HideTabHeader");
                buildingButtonsAnimator.Play("HideTabHeader");
                break;
            case ButtonState.EnclosureButtons:
                itemButtonsAnimator.Play("HideWholeTab");
                enclosureButtonsAnimator.Play("HideWholeTab");
                buildingButtonsAnimator.Play("HideTabHeader");
                break;
            case ButtonState.BuildingButtons:
                itemButtonsAnimator.Play("HideTabHeader");
                enclosureButtonsAnimator.Play("HideTabHeader");
                buildingButtonsAnimator.Play("HideWholeTab");
                break;
        }
    }

    public void HideButtons()
    {
        HideTopButtons();
        canvasAnimator.Play("HideLeaveIslandButton");
        switch (buttonState)
        {
            case ButtonState.ConstructionDetails: constructionDetailsAnimator.Play("HideDetails"); break;
            case ButtonState.PeasantDetails: peasantDetailsAnimator.Play("HideDetails"); break;
        }

        buttonState = ButtonState.Idle;
    }

    public void OpenCloseItemButtons()
    {
        if (buttonState == ButtonState.Idle)
        {
            itemButtonsAnimator.Play("ShowTabContent");
            enclosureButtonsAnimator.Play("HideTabHeader");
            buildingButtonsAnimator.Play("HideTabHeader");
            buttonState = ButtonState.ItemButtons;
        }
        else
        {
            itemButtonsAnimator.Play("HideTabContent");
            enclosureButtonsAnimator.Play("ShowTabHeader");
            buildingButtonsAnimator.Play("ShowTabHeader");
            buttonState = ButtonState.Idle;
        }
    }

    public void OpenCloseEnclosureButtons()
    {
        if (buttonState == ButtonState.Idle)
        {
            itemButtonsAnimator.Play("HideTabHeader");
            enclosureButtonsAnimator.Play("ShowTabContent");
            buildingButtonsAnimator.Play("HideTabHeader");
            buttonState = ButtonState.EnclosureButtons;
        }
        else
        {
            itemButtonsAnimator.Play("ShowTabHeader");
            enclosureButtonsAnimator.Play("HideTabContent");
            buildingButtonsAnimator.Play("ShowTabHeader");
            buttonState = ButtonState.Idle;
        }
    }

    public void ChooseEnclosure()
    {
        canvasAnimator.Play("HideLeaveIslandButton");
        enclosureButtonsAnimator.Play("HideWholeTab");
        buttonState = ButtonState.Editing;
    }

    public void OpenCloseBuildingButtons()
    {
        if (buttonState == ButtonState.Idle)
        {
            itemButtonsAnimator.Play("HideTabHeader");
            enclosureButtonsAnimator.Play("HideTabHeader");
            buildingButtonsAnimator.Play("ShowTabContent");
            buttonState = ButtonState.BuildingButtons;
        }
        else
        {
            itemButtonsAnimator.Play("ShowTabHeader");
            enclosureButtonsAnimator.Play("ShowTabHeader");
            buildingButtonsAnimator.Play("HideTabContent");
            buttonState = ButtonState.Idle;
        }
    }

    public void ChooseBuilding()
    {
        canvasAnimator.Play("HideLeaveIslandButton");
        buildingButtonsAnimator.Play("HideWholeTab");
        buttonState = ButtonState.Editing;
    }


    public void ShowPeasantDetails(PeasantScript peasantScript)
    {
        peasantDetailsScript.SetPeasantDetails(peasantScript);

        HideTopButtons();
        if (buttonState == ButtonState.ConstructionDetails)
        {
            constructionDetailsAnimator.Play("HideDetails");
        }
        else if(buttonState != ButtonState.PeasantDetails)
        {
            canvasAnimator.Play("HideLeaveIslandButton");
            peasantDetailsAnimator.Play("ShowDetails");
        }

        buttonState = ButtonState.PeasantDetails;
    }

    public void HidePeasantDetails()
    {
        peasantDetailsAnimator.Play("HideDetails");

        ShowDefaultButtons();
    }

    public void ShowConstructionDetails(ConstructionScript constructionScript)
    {
        constructionDetailsScript.SetConstructionDetails(constructionScript);

        HideTopButtons();
        if (buttonState == ButtonState.PeasantDetails)
        {
            peasantDetailsAnimator.Play("HideDetails");
        }
        if(buttonState != ButtonState.ConstructionDetails)
        {
            canvasAnimator.Play("HideLeaveIslandButton");
            constructionDetailsAnimator.Play("ShowDetails");
        }

        buttonState = ButtonState.ConstructionDetails;
    }

    public void HideConstructionDetails()
    {
        constructionDetailsAnimator.Play("HideDetails");

        ShowDefaultButtons();
    }

    public void ShowInventoryEditor()
    {
        constructionDetailsAnimator.Play("HideDetails");

        inventoryEditor.gameObject.SetActive(true);
        inventoryEditor.SetGrid();
        inventoryEditorAnimator.Play("ShowPopUp");

        buttonState = ButtonState.PopUp;
    }

    public void HideInventoryEditor()
    {
        inventoryEditorAnimator.Play("HidePopUp");
        constructionDetailsAnimator.Play("ShowDetails");

        buttonState = ButtonState.ConstructionDetails;
        //ShowButtons();
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

    public void ShowPenEditor()
    {
        constructionDetailsAnimator.Play("HideDetails");

        penEditor.gameObject.SetActive(true);
        penEditor.penScript = (PenScript)constructionDetailsScript.constructionScript;
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

    public void UpdateInventoryRow(ResourceScript.ResourceType resourceType, int resourceIndex)
    {
        inventoryEditor.UpdateInventoryRow(resourceType, resourceIndex);
    }

    public void UpdatePenRow(ResourceScript.AnimalType animalType)
    {
        penEditor.UpdatePenRow(animalType);
    }

    public void LeaveIsland()
    {
        HideButtons();

        canvasAnimator.Play("LeaveIsland");
    }

}
