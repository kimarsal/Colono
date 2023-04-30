using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasScript : MonoBehaviour
{
    public static CanvasScript Instance { get; private set; }
    private enum ButtonState { Idle, ServiceButtons, WorkButtons, ItemButtons, Editing, ConstructionDetails, PeasantDetails, PopUp };
    private ButtonState buttonState = ButtonState.Idle;

    [Header("BottomButtons")]
    [SerializeField] private Animator fishButtonAnimator;
    [SerializeField] private Animator tradeButtonAnimator;
    [SerializeField] private Animator dockButtonAnimator;
    [SerializeField] private Animator sailButtonAnimator;

    [Header("Tabs")]
    [SerializeField] private Animator serviceButtonsAnimator;
    [SerializeField] private Animator workButtonsAnimator;
    [SerializeField] private Animator itemButtonsAnimator;
    [SerializeField] private TopButtonScript[] topButtons;

    [Header("Details")]
    public ConstructionDetailsScript constructionDetailsScript;
    [SerializeField] private Animator peasantDetailsAnimator;

    [Header("Editors")]
    [SerializeField] private PeasantDetailsScript peasantDetailsScript;
    public ShipEditor shipEditor;
    public GardenEditor gardenEditor;
    public PenEditor penEditor;
    public TavernEditor tavernEditor;
    public CabinEditor cabinEditor;
    public InventoryEditor inventoryEditor;
    public MineEditor mineEditor;
    [SerializeField] private TradeEditor tradeEditor;

    [Header("Others")]
    [SerializeField] private Animator compassAnimator;
    [SerializeField] private NewCropPopUpScript newCropPopUpScript;
    public Button dockButton;

    [SerializeField] private PopUpScript pauseMenu;

    private bool canPlayerTrade = false;
    private bool canPlayerFish = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        constructionDetailsScript.gameObject.SetActive(true);
        compassAnimator.Play("ShowCompass");
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

    public void Dock()
    {
        compassAnimator.Play("HideCompass");
        dockButtonAnimator.Play("HideBottomButton");
        ShowDefaultButtons();
    }

    public void ShowDefaultButtons()
    {
        sailButtonAnimator.Play("ShowBottomButton");
        itemButtonsAnimator.Play("ShowTabHeader");
        workButtonsAnimator.Play("ShowTabHeader");
        serviceButtonsAnimator.Play("ShowTabHeader");

        buttonState = ButtonState.Idle;
    }

    public void HideTopButtons()
    {
        switch (buttonState)
        {
            case ButtonState.Idle:
                serviceButtonsAnimator.Play("HideTabHeader");
                workButtonsAnimator.Play("HideTabHeader");
                itemButtonsAnimator.Play("HideTabHeader");
                break;
            case ButtonState.ServiceButtons:
                serviceButtonsAnimator.Play("HideWholeTab");
                break;
            case ButtonState.WorkButtons:
                workButtonsAnimator.Play("HideWholeTab");
                break;
            case ButtonState.ItemButtons:
                itemButtonsAnimator.Play("HideWholeTab");
                break;
        }
    }

    public void ToggleServiceButtons()
    {
        if (buttonState == ButtonState.Idle)
        {
            serviceButtonsAnimator.Play("ShowTabContent");
            workButtonsAnimator.Play("HideTabHeader");
            itemButtonsAnimator.Play("HideTabHeader");
            buttonState = ButtonState.ServiceButtons;
        }
        else
        {
            serviceButtonsAnimator.Play("HideTabContent");
            workButtonsAnimator.Play("ShowTabHeader");
            itemButtonsAnimator.Play("ShowTabHeader");
            buttonState = ButtonState.Idle;
        }
    }

    public void ToggleWorkButtons()
    {
        if (buttonState == ButtonState.Idle)
        {
            serviceButtonsAnimator.Play("HideTabHeader");
            workButtonsAnimator.Play("ShowTabContent");
            itemButtonsAnimator.Play("HideTabHeader");
            buttonState = ButtonState.WorkButtons;
        }
        else
        {
            serviceButtonsAnimator.Play("ShowTabHeader");
            workButtonsAnimator.Play("HideTabContent");
            itemButtonsAnimator.Play("ShowTabHeader");
            buttonState = ButtonState.Idle;
        }
    }

    public void ToggleItemButtons()
    {
        if (buttonState == ButtonState.Idle)
        {
            serviceButtonsAnimator.Play("HideTabHeader");
            workButtonsAnimator.Play("HideTabHeader");
            itemButtonsAnimator.Play("ShowTabContent");
            buttonState = ButtonState.ItemButtons;
        }
        else
        {
            serviceButtonsAnimator.Play("ShowTabHeader");
            workButtonsAnimator.Play("ShowTabHeader");
            itemButtonsAnimator.Play("HideTabContent");
            buttonState = ButtonState.Idle;
        }
    }

    public void ChooseTopButton()
    {
        HideTopButtons();
        sailButtonAnimator.Play("HideBottomButton");
        buttonState = ButtonState.Editing;
    }


    public void ShowPeasantDetails(PeasantScript peasantScript)
    {
        peasantDetailsScript.SetPeasantDetails(peasantScript);

        if (buttonState != ButtonState.PeasantDetails)
        {
            if (buttonState == ButtonState.ConstructionDetails)
            {
                constructionDetailsScript.HideDetails();
            }
            else
            {
                HideTopButtons();
                sailButtonAnimator.Play("HideBottomButton");
            }

            peasantDetailsAnimator.Play("ShowDetails");
            buttonState = ButtonState.PeasantDetails;
        }
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

    public void CropIsDiscovered(int cropType)
    {
        newCropPopUpScript.gameObject.SetActive(true);
        newCropPopUpScript.ShowNewCrop(cropType);
    }

    public void UpdateInventoryRow(ResourceScript.ResourceType resourceType, int resourceIndex)
    {
        inventoryEditor.UpdateInventoryRow(resourceType, resourceIndex);
    }

    public void UpdateTopButtons()
    {
        foreach(TopButtonScript topButtonScript in topButtons)
        {
            topButtonScript.UpdateAvailability();
        }
    }

    public void UpdatePenRow(ResourceScript.AnimalType animalType)
    {
        penEditor.UpdatePenRow(animalType);
    }

    public void Sail()
    {
        compassAnimator.Play("ShowCompass");
        HideTopButtons();
        sailButtonAnimator.Play("HideBottomButton");
        dockButtonAnimator.Play("ShowBottomButton");

        buttonState = ButtonState.Idle;
    }

    public void CanTrade()
    {
        if (!canPlayerTrade)
        {
            canPlayerTrade = true;
            tradeButtonAnimator.Play("ShowBottomButton");
        }
    }

    public void CannotTrade()
    {
        if (canPlayerTrade)
        {
            canPlayerTrade = false;
            tradeButtonAnimator.Play("HideBottomButton");
        }
    }

    public void CanFish()
    {
        if (!canPlayerFish && !ShipScript.Instance.fishingScript.enabled)
        {
            canPlayerFish = true;
            fishButtonAnimator.Play("ShowBottomButton");
        }
    }

    public void CannotFish()
    {
        if (canPlayerFish)
        {
            canPlayerFish = false;
            if (ShipScript.Instance.fishingScript.enabled)
            {
                ShipScript.Instance.EndFishingSession();
            }
            else
            {
                fishButtonAnimator.Play("HideBottomButton");
            }
        }
    }

    public void Trade()
    {
        tradeEditor.gameObject.SetActive(true);
        tradeEditor.SetEditor(null);
    }

    public void TogglePauseMenu()
    {
        if (pauseMenu.gameObject.activeSelf)
        {
            pauseMenu.HidePopUp();
        }
        else
        {
            pauseMenu.gameObject.SetActive(true);
            pauseMenu.ShowPopUp();
        }
    }
}
