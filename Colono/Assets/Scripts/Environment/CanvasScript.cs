using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasScript : MonoBehaviour
{
    public static CanvasScript Instance { get; private set; }
    private enum ButtonState { Idle, ServiceButtons, WorkButtons, ItemButtons, Editing, ConstructionDetails, PeasantDetails, PopUp };
    private ButtonState buttonState = ButtonState.Idle;

    [Header("BottomButtons")]
    [SerializeField] private Animator dockButtonAnimator;
    [SerializeField] private Animator sailButtonAnimator;
    [SerializeField] private Animator tradeButtonAnimator;
    [SerializeField] private Animator fishingButtonAnimator;
    [SerializeField] private TextMeshProUGUI fishingButtonText;

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
    public TradeEditor tradeEditor;

    [Header("Others")]
    public Button dockButton;
    [SerializeField] private NameIslandPopUpScript nameIslandPopUpScript;
    [SerializeField] private Transform inventoryChangeList;
    [SerializeField] private Animator compassAnimator;
    [SerializeField] private PopUpScript mapPopUp;
    [SerializeField] private PopUpScript pauseMenu;
    [SerializeField] private NewCropPopUpScript newCropPopUpScript;
    [SerializeField] private AudioSource soundSource;
    [SerializeField] private AudioClip pageFlipClip;
    [SerializeField] private GameObject gameOverScreen;

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
        dockButton.GetComponentInChildren<TextMeshProUGUI>().text = "Dock onto\n" + (islandScript.islandName == null ? "unknown island" : islandScript.islandName);
        dockButtonAnimator.Play("ShowBottomButton");
    }

    public void PlayerIsFarFromIsland()
    {
        dockButtonAnimator.Play("HideBottomButton");
    }

    public void NameIsland()
    {
        nameIslandPopUpScript.gameObject.SetActive(true);
        nameIslandPopUpScript.ShowPopUp();
    }

    public void Dock()
    {
        compassAnimator.Play("HideCompass");
        dockButtonAnimator.Play("HideBottomButton");
        ShowDefaultButtons();
        UpdateTopButtons();
    }

    public void ShowDefaultButtons()
    {
        sailButtonAnimator.Play("ShowBottomButton");
        itemButtonsAnimator.Play("ShowTabHeader");
        workButtonsAnimator.Play("ShowTabHeader");
        serviceButtonsAnimator.Play("ShowTabHeader");

        buttonState = ButtonState.Idle;

        soundSource.PlayOneShot(pageFlipClip);
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
        soundSource.PlayOneShot(pageFlipClip);
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
        soundSource.PlayOneShot(pageFlipClip);
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
        soundSource.PlayOneShot(pageFlipClip);
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

            soundSource.PlayOneShot(pageFlipClip);
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

            soundSource.PlayOneShot(pageFlipClip);
        }

        buttonState = ButtonState.ConstructionDetails;
    }

    public void HideConstructionDetails()
    {
        constructionDetailsScript.HideDetails();

        ShowDefaultButtons();
    }

    public void InventoryChange(ResourceScript.ResourceType resourceType, int resourceIndex, int amount)
    {
        Instantiate(ResourceScript.Instance.inventoryChange, inventoryChangeList).SetInventoryChange(ResourceScript.Instance.GetResourceSprite(resourceType, resourceIndex), amount);
        
        UpdateInventoryRow(resourceType, resourceIndex);

        if (resourceType == ResourceScript.ResourceType.Material)
        {
            if (resourceIndex == (int)ResourceScript.MaterialType.Gem)
            {
                constructionDetailsScript.UpdateUpgradeButton();
            }
            else
            {
                UpdateTopButtons();
            }
        }
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
            fishingButtonAnimator.Play("ShowFishingButton");
        }
    }

    public void CannotFish()
    {
        if (canPlayerFish)
        {
            canPlayerFish = false;
            if (ShipScript.Instance.fishingScript.enabled)
            {
                ToggleFishing();
            }
            fishingButtonAnimator.Play("HideFishingButton");
        }
    }

    public void ToggleFishing()
    {
        ShipScript.Instance.ToggleFishing();
        fishingButtonText.text = ShipScript.Instance.fishingScript.enabled ? "Stop fishing" : "Start fishing";
    }

    public void Trade()
    {
        if (tradeEditor.gameObject.activeSelf) return;

        tradeEditor.gameObject.SetActive(true);
        tradeEditor.SetEditor(null);
        soundSource.PlayOneShot(pageFlipClip);
    }

    public void OpenMap()
    {
        if (mapPopUp.gameObject.activeSelf || GameManager.Instance.isGameOver) return;

        mapPopUp.gameObject.SetActive(true);
        mapPopUp.ShowPopUp();
    }

    public void TogglePauseMenu()
    {
        if (GameManager.Instance.isGameOver) return;

        if (pauseMenu.gameObject.activeSelf)
        {
            pauseMenu.HidePopUp();
        }
        else
        {
            pauseMenu.gameObject.SetActive(true);
            pauseMenu.ShowPopUp();
        }
        soundSource.PlayOneShot(pageFlipClip);
    }

    public void GameOver()
    {
        gameOverScreen.SetActive(true);
    }
}
