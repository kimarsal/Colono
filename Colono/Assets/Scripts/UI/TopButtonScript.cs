using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TopButtonScript : MonoBehaviour
{
    public IslandCellScript.SelectAction function;
    public BuildingScript buildingScript;
    public EnclosureScript.EnclosureType enclosureType;
    private Button button;

    [SerializeField] private TextMeshProUGUI requiredWoodText;
    [SerializeField] private TextMeshProUGUI requiredStoneText;
    [SerializeField] private TextMeshProUGUI requiredSproutsText;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(ButtonClick);

        if(function == IslandCellScript.SelectAction.PlaceBuilding)
        {
            requiredWoodText.text = buildingScript.requiredWood.ToString();
            requiredStoneText.text = buildingScript.requiredStone.ToString();
        }
    }

    public void UpdateAvailability()
    {
        bool hasEnoughWood = false;
        bool hasEnoughStone = false;
        bool hasEnoughSprouts = false;
        IslandScript islandScript = GameManager.Instance.closestIsland;
        switch(function)
        {
            case IslandCellScript.SelectAction.PlaceBuilding:
                hasEnoughWood = islandScript.GetResourceAmount(ResourceScript.ResourceType.Material, (int)ResourceScript.MaterialType.Wood) >= buildingScript.requiredWood;
                hasEnoughStone = islandScript.GetResourceAmount(ResourceScript.ResourceType.Material, (int)ResourceScript.MaterialType.Stone) >= buildingScript.requiredStone;
                button.interactable = hasEnoughWood && hasEnoughStone;
                break;
            case IslandCellScript.SelectAction.CreateEnclosure:
                button.interactable = hasEnoughWood = islandScript.GetResourceAmount(ResourceScript.ResourceType.Material, (int)ResourceScript.MaterialType.Wood) >= 8;
                break;
            case IslandCellScript.SelectAction.PlantTrees:
                button.interactable = hasEnoughSprouts = islandScript.GetResourceAmount(ResourceScript.ResourceType.Material, (int)ResourceScript.MaterialType.Sprout) > 0;
                break;
            case IslandCellScript.SelectAction.ClearItems: case IslandCellScript.SelectAction.CancelItemClearing:
                button.interactable = true;
                break;
        }

        requiredWoodText.color = hasEnoughWood ? Color.black : Color.red;
        requiredStoneText.color = hasEnoughStone ? Color.black : Color.red;
        requiredSproutsText.color = hasEnoughSprouts ? Color.black : Color.red;
    }

    public void ButtonClick()
    {
        GameManager.Instance.TopButtonClick(this);
    }

}
