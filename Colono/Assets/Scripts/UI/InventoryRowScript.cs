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
    public int islandResources;
    public int shipResources;

    public void UpdateValues()
    {
        totalResources = islandResources + shipResources;
        resourceSlider.maxValue = totalResources;
        resourceSlider.value = shipResources;
        resourceSlider.enabled = totalResources > 0;
        blocker.SetActive(totalResources == 0);

        UpdateText();
    }

    public void OnSliderValueChange()
    {
        int difference = (int)resourceSlider.value - shipResources;
        bool canMoveResource = inventoryEditor.MoveResource(resourceType, resourceIndex, difference);

        if (canMoveResource)
        {
            islandResources -= difference;
            shipResources += difference;

            UpdateText();
        }
        else
        {
            resourceSlider.value = shipResources;
        }
    }

    public void UpdateText()
    {
        resourcesInIslandText.text = islandResources.ToString();
        resourcesInShipText.text = shipResources.ToString();
        discardResourceFromIslandButton.enabled = islandResources > 0;
        discardResourceFromShipButton.enabled = shipResources > 0;
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
