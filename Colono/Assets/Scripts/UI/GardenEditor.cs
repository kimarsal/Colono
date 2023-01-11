using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ResourceScript;

public class GardenEditor : MonoBehaviour
{
    public GardenScript gardenScript;

    public Dropdown cropDropdown;
    public Transform gridTransform;
    public GridLayoutGroup gridLayoutGroup;
    public GameObject cropButtonPrefab;

    public CropType selectedCropType;

    public void SetGrid()
    {
        foreach (Transform cropButton in gridTransform)
        {
            Destroy(cropButton.gameObject);
        }

        int cropTypes = System.Enum.GetValues(typeof(CropType)).Length;
        cropDropdown.ClearOptions();
        for(int i = 0; i < cropTypes; i++)
        {
            cropDropdown.options.Add(new Dropdown.OptionData(gardenScript.islandEditor.cropSprites[i]));
        }
        cropDropdown.template.GetComponent<RectTransform>().sizeDelta = new Vector2(cropDropdown.template.GetComponent<RectTransform>().sizeDelta.x, cropTypes * 100 / 2);
        //cropDropdown.value = 0;
        
        gridLayoutGroup.cellSize = new Vector2(cropButtonPrefab.GetComponent<RectTransform>().sizeDelta.x, cropButtonPrefab.GetComponent<RectTransform>().sizeDelta.y);
        gridLayoutGroup.constraintCount = gardenScript.width - 2;

        for (int i = 0; i < gardenScript.crops.Length; i++)
        {
            GameObject cropButton = Instantiate(cropButtonPrefab, gridTransform);
            CropButtonScript cropButtonScript = cropButton.GetComponent<CropButtonScript>();
            cropButtonScript.gardenEditor = this;
            cropButtonScript.cropImage.sprite = gardenScript.islandEditor.cropSprites[(int)gardenScript.crops[i]];
            cropButtonScript.index = i;
        }
    }

    public void DropdownChange(int value)
    {
        selectedCropType = (CropType)value;
    }

    public void ChangeCrop(CropButtonScript cropButtonScript)
    {
        gardenScript.crops[cropButtonScript.index] = selectedCropType;
        cropButtonScript.cropImage.sprite = gardenScript.islandEditor.cropSprites[(int)selectedCropType];
    }
}
