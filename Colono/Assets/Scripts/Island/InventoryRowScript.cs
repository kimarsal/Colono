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
    public int resourcesInIsland;
    public int resourcesInShip;

    public void UpdateValues()
    {
        totalResources = resourcesInIsland + resourcesInShip;
        resourceSlider.maxValue = totalResources;
        resourceSlider.value = resourcesInShip;
        resourceSlider.enabled = totalResources > 0;
        blocker.SetActive(totalResources == 0);

        UpdateText();
    }

    public void OnSliderValueChange()
    {
        int difference = (int)resourceSlider.value - resourcesInShip;
        bool canMoveResource = inventoryEditor.MoveResource(resourceType, resourceIndex, difference);

        if (canMoveResource)
        {
            resourcesInIsland -= difference;
            resourcesInShip += difference;

            UpdateText();
        }
        else
        {
            resourceSlider.value = resourcesInShip;
        }
    }

    public void UpdateText()
    {
        resourcesInIslandText.text = resourcesInIsland.ToString();
        resourcesInShipText.text = resourcesInShip.ToString();
        discardResourceFromIslandButton.enabled = resourcesInIsland > 0;
        discardResourceFromShipButton.enabled = resourcesInShip > 0;
    }

    public void DiscardResource(bool fromIsland)
    {
        if (fromIsland)
        {
            resourcesInIsland--;
        }
        else
        {
            resourcesInShip--;
        }
        UpdateValues();

        inventoryEditor.DiscardResource(resourceType, resourceIndex, fromIsland);
    }

}
