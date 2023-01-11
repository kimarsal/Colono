using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static ResourceScript;
using UnityEngine.UI;

public class InventoryEditor : MonoBehaviour
{
    public IslandEditor islandEditor;
    public InventoryScript islandInventoryScript;
    public InventoryScript shipInventoryScript;

    public TextMeshProUGUI islandInventoryText;
    public TextMeshProUGUI shipInventoryText;

    public Transform rows;
    public ScrollRect scrollRect;
    public InventoryRowScript[][] inventoryRows;
    public GameObject inventoryRowPrefab;

    public Button[] tabButtons;

    private void SetRow(ResourceType resourceType, int resourceIndex)
    {
        GameObject gridRow = Instantiate(inventoryRowPrefab, rows);
        InventoryRowScript gridRowScript = gridRow.GetComponent<InventoryRowScript>();
        gridRowScript.inventoryEditor = this;
        gridRowScript.resourceType = resourceType;
        gridRowScript.resourceIndex = resourceIndex;
        gridRowScript.resourceImage.sprite = islandEditor.GetResourceSprite(resourceType, resourceIndex);
        gridRowScript.islandResources = islandInventoryScript.GetResourceAmount(resourceType, resourceIndex);
        gridRowScript.shipResources = shipInventoryScript.GetResourceAmount(resourceType, resourceIndex);
        gridRowScript.UpdateValues();
        inventoryRows[(int)resourceType][resourceIndex] = gridRowScript;
    }

    public void SetGrid()
    {
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

        SelectTab(-1);
        UpdateInventoryText();

    }

    public void SelectTab(int tabIndex)
    {
        for(int i = 0; i < inventoryRows.Length; i++)
        {
            tabButtons[i].interactable = tabIndex != i;

            foreach(InventoryRowScript row in inventoryRows[i])
            {
                // TODO: Si ha estat descobert
                row.gameObject.SetActive(tabIndex == -1 || tabIndex == i);
            }
        }
        tabButtons[inventoryRows.Length].interactable = tabIndex != -1;

        scrollRect.normalizedPosition = new Vector2(0, 1);
    }

    public void UpdateInventoryRow(ResourceType resourceType, int resourceIndex)
    {
        if (inventoryRows == null || inventoryRows[(int)resourceType] == null || inventoryRows[(int)resourceType][resourceIndex] == null)
        {
            return;
        }

        InventoryRowScript inventoryRowScript = inventoryRows[(int)resourceType][resourceIndex];
        inventoryRowScript.islandResources = islandInventoryScript.GetResourceAmount(resourceType, resourceIndex);
        inventoryRowScript.shipResources = shipInventoryScript.GetResourceAmount(resourceType, resourceIndex);
        inventoryRowScript.UpdateValues();
        UpdateInventoryText();
    }

    public void UpdateInventoryText()
    {
        islandInventoryText.text = islandInventoryScript.usage + "/" + islandInventoryScript.capacity;
        shipInventoryText.text = shipInventoryScript.usage + "/" + shipInventoryScript.capacity;
    }

    public bool MoveResource(ResourceType resourceType, int resourceIndex, int difference)
    {
        if(difference > 0) // island -> ship
        {
            if(shipInventoryScript.AddResource(resourceType, resourceIndex, difference) > 0)
            {
                return false;
            }
            islandInventoryScript.RemoveResource(resourceType, resourceIndex, difference);
        }
        else if(difference < 0) // ship -> island
        {
            difference = -difference;
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
