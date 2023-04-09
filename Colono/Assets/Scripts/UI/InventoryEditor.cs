using UnityEngine;
using TMPro;
using static ResourceScript;
using UnityEngine.UI;

public class InventoryEditor : EditorScript
{
    public InventoryScript shipInventoryScript;
    public InventoryScript islandInventoryScript;

    [SerializeField] private TextMeshProUGUI shipInventoryText;
    [SerializeField] private TextMeshProUGUI islandInventoryText;
    [SerializeField] private Button upgradeShipInventoryCategoryButton;
    [SerializeField] private Button upgradeIslandInventoryCategoryButton;

    [SerializeField] private Transform rows;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private InventoryRowScript[][] inventoryRows;
    [SerializeField] private InventoryRowScript inventoryRowPrefab;

    [SerializeField] private Button[] tabButtons;
    private int selectedTab;

    private void SetRow(ResourceType resourceType, int resourceIndex)
    {
        InventoryRowScript gridRowScript = Instantiate(inventoryRowPrefab, rows);
        gridRowScript.inventoryEditor = this;
        gridRowScript.resourceType = resourceType;
        gridRowScript.resourceIndex = resourceIndex;
        gridRowScript.resourceImage.sprite = ResourceScript.Instance.GetResourceSprite(resourceType, resourceIndex);
        gridRowScript.shipResources = shipInventoryScript.GetResourceAmount(resourceType, resourceIndex);
        gridRowScript.islandResources = islandInventoryScript.GetResourceAmount(resourceType, resourceIndex);
        gridRowScript.UpdateValues();
        inventoryRows[(int)resourceType][resourceIndex] = gridRowScript;
    }

    public override void SetEditor(ConstructionScript constructionScript)
    {
        islandInventoryScript = constructionScript.islandScript.inventoryScript;

        foreach (Transform row in rows)
        {
            Destroy(row.gameObject);
        }

        inventoryRows = new InventoryRowScript[System.Enum.GetValues(typeof(ResourceType)).Length - 1][];

        inventoryRows[(int)ResourceType.Material] = new InventoryRowScript[System.Enum.GetValues(typeof(MaterialType)).Length];
        for (int i = 0; i < inventoryRows[(int)ResourceType.Material].Length; i++)
        {
            SetRow(ResourceType.Material, i);
        }

        inventoryRows[(int)ResourceType.Crop] = new InventoryRowScript[System.Enum.GetValues(typeof(CropType)).Length];
        for (int i = 0; i < inventoryRows[(int)ResourceType.Crop].Length; i++)
        {
            SetRow(ResourceType.Crop, i);
        }

        inventoryRows[(int)ResourceType.Meat] = new InventoryRowScript[System.Enum.GetValues(typeof(MeatType)).Length];
        for (int i = 0; i < inventoryRows[(int)ResourceType.Meat].Length; i++)
        {
            SetRow(ResourceType.Meat, i);
        }

        SelectTab(0);

    }

    public void SelectTab(int tabIndex)
    {
        selectedTab = tabIndex;

        for(int i = 0; i < inventoryRows.Length; i++)
        {
            tabButtons[i].interactable = tabIndex != i;

            foreach(InventoryRowScript row in inventoryRows[i])
            {
                bool shouldBeActive = tabIndex == i;
                if (shouldBeActive && tabIndex == 1) shouldBeActive = GameManager.Instance.discoveredCrops[i]; 
                row.gameObject.SetActive(shouldBeActive);
            }
        }

        upgradeShipInventoryCategoryButton.interactable = shipInventoryScript.CanLevelUpCategory(selectedTab);
        upgradeIslandInventoryCategoryButton.interactable = islandInventoryScript.CanLevelUpCategory(selectedTab);
        UpdateInventoryText();
        scrollRect.normalizedPosition = new Vector2(0, 1);
    }

    public void UpgradeShipInventoryCategory()
    {
        shipInventoryScript.LevelUpInventoryCategory(selectedTab);
        upgradeShipInventoryCategoryButton.interactable = shipInventoryScript.CanLevelUpCategory(selectedTab);
        shipInventoryText.text = shipInventoryScript.GetUsedCapacity(selectedTab);
    }

    public void UpgradeIslandInventoryCategory()
    {
        islandInventoryScript.LevelUpInventoryCategory(selectedTab);
        upgradeIslandInventoryCategoryButton.interactable = islandInventoryScript.CanLevelUpCategory(selectedTab);
        islandInventoryText.text = islandInventoryScript.GetUsedCapacity(selectedTab);
    }

    public void UpdateInventoryRow(ResourceType resourceType, int resourceIndex)
    {
        if (inventoryRows == null || inventoryRows[(int)resourceType] == null || inventoryRows[(int)resourceType][resourceIndex] == null)
        {
            return;
        }

        InventoryRowScript inventoryRowScript = inventoryRows[(int)resourceType][resourceIndex];
        inventoryRowScript.shipResources = shipInventoryScript.GetResourceAmount(resourceType, resourceIndex);
        inventoryRowScript.islandResources = islandInventoryScript.GetResourceAmount(resourceType, resourceIndex);
        inventoryRowScript.UpdateValues();
        UpdateInventoryText();
    }

    public void UpdateInventoryText()
    {
        shipInventoryText.text = shipInventoryScript.GetUsedCapacity(selectedTab);
        islandInventoryText.text = islandInventoryScript.GetUsedCapacity(selectedTab);
    }

    public bool MoveResource(ResourceType resourceType, int resourceIndex, int difference)
    {
        if(difference < 0) // island -> ship
        {
            difference = -difference;
            if (shipInventoryScript.AddResource(resourceType, resourceIndex, difference) > 0)
            {
                return false;
            }
            islandInventoryScript.RemoveResource(resourceType, resourceIndex, difference);
        }
        else if(difference > 0) // ship -> island
        {
            if (islandInventoryScript.AddResource(resourceType, resourceIndex, difference) > 0)
            {
                return false;
            }
            shipInventoryScript.RemoveResource(resourceType, resourceIndex, difference);
        }
        UpdateInventoryText();
        return true;
    }


    public void DiscardResource(ResourceType resourceType, int resourceIndex, bool fromIsland)
    {
        if (fromIsland)
        {
            islandInventoryScript.RemoveResource(resourceType, resourceIndex);
        }
        else
        {
            shipInventoryScript.RemoveResource(resourceType, resourceIndex);
        }
        UpdateInventoryText();
    }

}
