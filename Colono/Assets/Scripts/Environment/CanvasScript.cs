using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasScript : MonoBehaviour
{
    public static CanvasScript Instance { get; private set; }
    private enum ButtonState { Idle, ItemButtons, BuildingButtons, EnclosureButtons, Editing, ConstructionDetails, PeasantDetails, PopUp };
    private ButtonState buttonState = ButtonState.Idle;

    [Header("BottomButtons")]
    [SerializeField] private Animator fishButtonAnimator;
    [SerializeField] private Animator tradeButtonAnimator;
    [SerializeField] private Animator surrenderButtonAnimator;
    [SerializeField] private Animator dockButtonAnimator;
    [SerializeField] private Animator sailButtonAnimator;

    [Header("Tabs")]
    [SerializeField] private Animator itemButtonsAnimator;
    [SerializeField] private Animator buildingButtonsAnimator;
    [SerializeField] private Animator enclosureButtonsAnimator;

    [Header("Details")]
    [SerializeField] private ConstructionDetailsScript constructionDetailsScript;
    [SerializeField] private Animator peasantDetailsAnimator;

    [Header("Editors")]
    [SerializeField] private PeasantDetailsScript peasantDetailsScript;
    public GardenEditor gardenEditor;
    public PenEditor penEditor;
    public TavernEditor tavernEditor;
    public InventoryEditor inventoryEditor;
    [SerializeField] private ShopEditor shopEditor;

    [Header("Buttons")]
    public Button dockButton;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        constructionDetailsScript.gameObject.SetActive(true);
    }

    public void PlayerIsNearIsland(IslandScript islandScript)
    {
        //TODO: Change button text to include island name
        dockButton.GetComponentInChildren<TextMeshProUGUI>().text = "Dock onto the island";
        dockButtonAnimator.Play("ShowBottomButton");
    }

    public void PlayerIsFarFromIsland()
    {
        dockButtonAnimator.Play("HideBottomButton");
    }

    public void DockOntoIsland()
    {
        dockButtonAnimator.Play("HideBottomButton");
        sailButtonAnimator.Play("ShowBottomButton");

        itemButtonsAnimator.Play("ShowTabHeader");
        enclosureButtonsAnimator.Play("ShowTabHeader");
        buildingButtonsAnimator.Play("ShowTabHeader");

        buttonState = ButtonState.Idle;
    }

    public void ShowDefaultButtons()
    {
        switch (buttonState)
        {
            case ButtonState.ConstructionDetails: constructionDetailsScript.HideDetails(); break;
            case ButtonState.PeasantDetails: peasantDetailsAnimator.Play("HideDetails"); break;
        }

        sailButtonAnimator.Play("ShowBottomButton");
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
        sailButtonAnimator.Play("HideBottomButton");
        switch (buttonState)
        {
            case ButtonState.ConstructionDetails: constructionDetailsScript.HideDetails(); break;
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

    public void HideItemButtons()
    {
        itemButtonsAnimator.Play("HideWholeTab");
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
        sailButtonAnimator.Play("HideBottomButton");
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
        sailButtonAnimator.Play("HideBottomButton");
        buildingButtonsAnimator.Play("HideWholeTab");
        buttonState = ButtonState.Editing;
    }


    public void ShowPeasantDetails(PeasantScript peasantScript)
    {
        peasantDetailsScript.SetPeasantDetails(peasantScript);

        HideTopButtons();
        if (buttonState == ButtonState.ConstructionDetails)
        {
            constructionDetailsScript.HideDetails();
        }
        else if(buttonState != ButtonState.PeasantDetails)
        {
            sailButtonAnimator.Play("HideBottomButton");
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
            sailButtonAnimator.Play("HideBottomButton");
            constructionDetailsScript.ShowDetails();
        }

        buttonState = ButtonState.ConstructionDetails;
    }

    public void HideConstructionDetails()
    {
        constructionDetailsScript.HideDetails();

        ShowDefaultButtons();
    }

    public void UpdateInventoryRow(ResourceScript.ResourceType resourceType, int resourceIndex)
    {
        inventoryEditor.UpdateInventoryRow(resourceType, resourceIndex);
    }

    public void UpdatePenRow(ResourceScript.AnimalType animalType)
    {
        penEditor.UpdatePenRow(animalType);
    }

    public void Sail()
    {
        HideButtons();

        dockButtonAnimator.Play("ShowBottomButton");
        sailButtonAnimator.Play("HideBottomButton");
    }

}
