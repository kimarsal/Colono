using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static ResourceScript;

public class InventoryEditor : MonoBehaviour
{
    public IslandEditor islandEditor;
    public ShipScript shipScript;
    public IslandScript islandScript;

    public TextMeshProUGUI islandInventoryText;
    public TextMeshProUGUI shipInventoryText;

    public Transform rows;
    public InventoryRowScript[][] inventoryRows;
    public GameObject inventoryRowPrefab;

    public PenScript penScript;

    private void SetRow(ResourceType resourceType, int resourceIndex)
    {
        GameObject gridRow = Instantiate(inventoryRowPrefab, rows);
        InventoryRowScript gridRowScript = gridRow.GetComponent<InventoryRowScript>();
        gridRowScript.inventoryEditor = this;
        gridRowScript.resourceType = resourceType;
        gridRowScript.resourceIndex = resourceIndex;
        gridRowScript.resourceImage.sprite = islandEditor.GetResourceSprite(resourceType, resourceIndex);
        gridRowScript.resourcesInIsland = islandScript.resources[(int)resourceType][resourceIndex];
        gridRowScript.resourcesInShip = shipScript.resources[(int)resourceType][resourceIndex];
        gridRowScript.UpdateValues();
        inventoryRows[(int)resourceType][resourceIndex] = gridRowScript;
    }

    public void SetGrid()
    {
        foreach (Transform row in rows)
        {
            Destroy(row.gameObject);
        }

        inventoryRows = new InventoryRowScript[System.Enum.GetValues(typeof(ResourceType)).Length][];

        if (penScript == null)
        {
            inventoryRows[(int)ResourceType.Material] = new InventoryRowScript[System.Enum.GetValues(typeof(MaterialType)).Length];
            for (int i = 0; i < inventoryRows[(int)ResourceType.Material].Length; i++)
            {
                SetRow(ResourceType.Material, i);
            }

            inventoryRows[(int)ResourceType.Crop] = new InventoryRowScript[System.Enum.GetValues(typeof(CropType)).Length];
            for (int i = 0; i < inventoryRows[(int)ResourceType.Crop].Length; i++)
            {
                // TODO: Si ha estat descobert
                SetRow(ResourceType.Crop, i);
            }

            inventoryRows[(int)ResourceType.Meat] = new InventoryRowScript[System.Enum.GetValues(typeof(MeatType)).Length];
            for (int i = 0; i < inventoryRows[(int)ResourceType.Meat].Length; i++)
            {
                SetRow(ResourceType.Meat, i);
            }

            UpdateInventoryText();
        }
        else
        {
            inventoryRows[(int)ResourceType.Animal] = new InventoryRowScript[System.Enum.GetValues(typeof(AnimalType)).Length];
            for (int i = 0; i < inventoryRows[(int)ResourceType.Animal].Length; i++)
            {
                SetRow(ResourceType.Animal, i);
            }
        }
    }

    public void UpdateInventoryRow(ResourceType resourceType, int resourceIndex)
    {
        if (inventoryRows[(int)resourceType] != null && inventoryRows[(int)resourceType][resourceIndex] != null)
        {
            InventoryRowScript inventoryRowScript = inventoryRows[(int)resourceType][resourceIndex];
            inventoryRowScript.resourcesInIsland = islandScript.resources[(int)resourceType][resourceIndex];
            inventoryRowScript.resourcesInShip = shipScript.resources[(int)resourceType][resourceIndex];
            inventoryRowScript.UpdateValues();
            UpdateInventoryText();
        }
    }

    public void UpdateInventoryText()
    {
        islandInventoryText.text = islandScript.usage + "/" + islandScript.capacity;
        shipInventoryText.text = shipScript.usage + "/" + shipScript.capacity;
    }

    public bool MoveResource(ResourceType resourceType, int resourceIndex, int difference)
    {
        if (difference < 0 && islandScript.usage < islandScript.capacity || difference > 0 && shipScript.usage < shipScript.capacity)
        {
            islandScript.resources[(int)resourceType][resourceIndex] -= difference;
            islandScript.usage -= difference;

            shipScript.resources[(int)resourceType][resourceIndex] += difference;
            shipScript.usage += difference;

            UpdateInventoryText();

            return true;
        }
        return false;
    }


    public void DiscardResource(ResourceType resourceType, int resourceIndex, bool fromIsland)
    {
        if (fromIsland)
        {
            islandScript.resources[(int)resourceType][resourceIndex]--;
            islandScript.usage--;
        }
        else
        {
            shipScript.resources[(int)resourceType][resourceIndex]--;
            shipScript.usage--;
        }
        UpdateInventoryText();
    }

}
