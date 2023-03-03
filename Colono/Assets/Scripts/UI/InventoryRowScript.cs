using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryRowScript : MonoBehaviour
{
    public InventoryEditor inventoryEditor;

    public ResourceScript.ResourceType resourceType;
    public int resourceIndex;

    public GameObject blocker;
    public Image resourceImage;
    public TextMeshProUGUI resourcesInIslandText;
    public Button discardResourceFromIslandButton;
    public Slider resourceSlider;
    public TextMeshProUGUI resourcesInShipText;
    public Button discardResourceFromShipButton;

    private int totalResources;
    public int shipResources;
    public int islandResources;

    public void UpdateValues()
    {
        totalResources = shipResources + islandResources;
        resourceSlider.maxValue = totalResources;
        resourceSlider.value = islandResources;
        resourceSlider.enabled = totalResources > 0;
        blocker.SetActive(totalResources == 0);

        UpdateText();
    }

    public void OnSliderValueChange()
    {
        int difference = (int)resourceSlider.value - islandResources;
        bool canMoveResource = inventoryEditor.MoveResource(resourceType, resourceIndex, difference);

        if (canMoveResource)
        {
            shipResources -= difference;
            islandResources += difference;

            UpdateText();
        }
        else
        {
            resourceSlider.value = islandResources;
        }
    }

    public void UpdateText()
    {
        resourcesInShipText.text = shipResources.ToString();
        resourcesInIslandText.text = islandResources.ToString();
        discardResourceFromShipButton.enabled = shipResources > 0;
        discardResourceFromIslandButton.enabled = islandResources > 0;
    }

    public void DiscardResource(bool fromIsland)
    {
        if (fromIsland)
        {
            islandResources--;
        }
        else
        {
            shipResources--;
        }
        UpdateValues();

        inventoryEditor.DiscardResource(resourceType, resourceIndex, fromIsland);
    }

}
