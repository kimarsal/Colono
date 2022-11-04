using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryRowScript : MonoBehaviour
{
    public ManageInventoryScript manageInventoryScript;

    public ResourceScript.ResourceType resourceType;
    public ResourceScript.MaterialType materialType;
    public ResourceScript.CropType cropType;

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
        bool canMoveResource;

        if (resourceType == ResourceScript.ResourceType.Material)
        {
            canMoveResource = manageInventoryScript.MoveMaterial(materialType, difference);
        }
        else
        {
            canMoveResource = manageInventoryScript.MoveCrop(cropType, difference);
        }

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

        if(resourceType == ResourceScript.ResourceType.Material)
        {
            manageInventoryScript.DiscardMaterial(materialType, fromIsland);
        }
        else
        {
            manageInventoryScript.DiscardCrop(cropType, fromIsland);
        }
    }

}
