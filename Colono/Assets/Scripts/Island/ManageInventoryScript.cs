using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static ResourceScript;

public class ManageInventoryScript : MonoBehaviour
{
    public IslandEditor islandEditor;
    public ShipScript shipScript;
    public IslandScript islandScript;

    public TextMeshProUGUI islandInventoryText;
    public TextMeshProUGUI shipInventoryText;

    public Transform rows;
    public GridRowScript[] materialsGridRows;
    public GridRowScript[] cropsGridRows;
    public GameObject gridRowPrefab;

    private void Awake()
    {
        rows.GetComponent<RectTransform>().sizeDelta = new Vector2(rows.GetComponent<RectTransform>().sizeDelta.x,
            (System.Enum.GetValues(typeof(ResourceScript.MaterialType)).Length + System.Enum.GetValues(typeof(ResourceScript.CropType)).Length)
            * gridRowPrefab.GetComponent<RectTransform>().sizeDelta.y);
    }

    public void SetGrid()
    {
        foreach (Transform row in rows)
        {
            Destroy(row.gameObject);
        }

        materialsGridRows = new GridRowScript[System.Enum.GetValues(typeof(ResourceScript.MaterialType)).Length];
        cropsGridRows = new GridRowScript[System.Enum.GetValues(typeof(ResourceScript.CropType)).Length];

        for (int i = 0; i < materialsGridRows.Length; i++)
        {
            GameObject gridRow = Instantiate(gridRowPrefab, rows);
            GridRowScript gridRowScript = gridRow.GetComponent<GridRowScript>();
            gridRowScript.manageInventoryScript = this;
            gridRowScript.resourceType = ResourceScript.ResourceType.Material;
            gridRowScript.materialType = (ResourceScript.MaterialType)i;
            gridRowScript.resourceImage.sprite = islandEditor.materialSprites[i];
            gridRowScript.resourcesInIsland = islandScript.materials[i];
            gridRowScript.resourcesInShip = shipScript.materials[i];
            gridRowScript.UpdateValues();
            materialsGridRows[i] = gridRowScript;
        }

        for (int i = 0; i < cropsGridRows.Length; i++)
        {
            GameObject gridRow = Instantiate(gridRowPrefab, rows);
            GridRowScript gridRowScript = gridRow.GetComponent<GridRowScript>();
            gridRowScript.manageInventoryScript = this;
            gridRowScript.resourceType = ResourceScript.ResourceType.Crop;
            gridRowScript.cropType = (ResourceScript.CropType)i;
            gridRowScript.resourceImage.sprite = islandEditor.cropSprites[i];
            gridRowScript.resourcesInIsland = islandScript.crops[i];
            gridRowScript.resourcesInShip = shipScript.crops[i];
            gridRowScript.UpdateValues();
            cropsGridRows[i] = gridRowScript;
        }

        UpdateInventoryText();
    }

    public void UpdateMaterial(ResourceScript.MaterialType materialType)
    {
        if(materialsGridRows != null && materialsGridRows[0] != null)
        {
            materialsGridRows[(int)materialType].resourcesInIsland = islandScript.materials[(int)materialType];
            materialsGridRows[(int)materialType].resourcesInShip = shipScript.materials[(int)materialType];
            materialsGridRows[(int)materialType].UpdateValues();
            UpdateInventoryText();
        }
    }

    public void UpdateCrop(ResourceScript.CropType cropType)
    {
        if(cropsGridRows != null && cropsGridRows[0] != null)
        {
            cropsGridRows[(int)cropType].resourcesInIsland = islandScript.crops[(int)cropType];
            cropsGridRows[(int)cropType].resourcesInShip = shipScript.crops[(int)cropType];
            cropsGridRows[(int)cropType].UpdateValues();
            UpdateInventoryText();
        }
    }

    public void UpdateInventoryText()
    {
        islandInventoryText.text = islandScript.usage + "/" + islandScript.capacity;
        shipInventoryText.text = shipScript.usage + "/" + shipScript.capacity;
    }

    public bool MoveMaterial(ResourceScript.MaterialType materialType, int difference)
    {
        if (difference < 0 && islandScript.usage < islandScript.capacity || difference > 0 && shipScript.usage < shipScript.capacity)
        {
            islandScript.materials[(int)materialType] -= difference;
            islandScript.usage -= difference;

            shipScript.materials[(int)materialType] += difference;
            shipScript.usage += difference;

            UpdateInventoryText();

            return true;
        }
        return false;
    }

    public bool MoveCrop(ResourceScript.CropType cropType, int difference)
    {
        if(difference < 0 && islandScript.usage < islandScript.capacity || difference > 0 && shipScript.usage < shipScript.capacity)
        {
            islandScript.crops[(int)cropType] -= difference;
            islandScript.usage -= difference;

            shipScript.crops[(int)cropType] += difference;
            shipScript.usage += difference;

            UpdateInventoryText();

            return true;
        }
        return false;
    }

    public void DiscardMaterial(ResourceScript.MaterialType materialType, bool fromIsland)
    {
        if (fromIsland)
        {
            islandScript.materials[(int)materialType]--;
            islandScript.usage--;
        }
        else
        {
            shipScript.materials[(int)materialType]--;
            shipScript.usage--;
        }
        UpdateInventoryText();
    }

    public void DiscardCrop(ResourceScript.CropType cropType, bool fromIsland)
    {
        if (fromIsland)
        {
            islandScript.crops[(int)cropType]--;
            islandScript.usage--;
        }
        else
        {
            shipScript.crops[(int)cropType]--;
            shipScript.usage--;
        }
        UpdateInventoryText();
    }
}
