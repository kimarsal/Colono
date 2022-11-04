using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ResourceScript;
using static UnityEngine.Rendering.DebugUI.Table;

public class GardenEditor : MonoBehaviour
{
    public GardenScript gardenScript;

    public Dropdown cropDropdown;
    public Transform gridTransform;
    public GridLayoutGroup gridLayoutGroup;
    public GameObject cropButtonPrefab;

    public ResourceScript.CropType selectedCropType;

    public void SetGrid()
    {
        foreach (Transform cropButton in gridTransform)
        {
            Destroy(cropButton.gameObject);
        }

        int cropTypes = System.Enum.GetValues(typeof(ResourceScript.CropType)).Length;
        cropDropdown.options = new List<Dropdown.OptionData>(cropTypes);
        for(int i = 0; i < cropTypes; i++)
        {
            cropDropdown.options.Add(new Dropdown.OptionData(gardenScript.islandScript.islandCellScript.islandEditorScript.cropSprites[i]));
        }
        cropDropdown.template.GetComponent<RectTransform>().sizeDelta = new Vector2(cropDropdown.template.GetComponent<RectTransform>().sizeDelta.x, cropTypes * 100 / 2);
        cropDropdown.value = 0;
        
        gridLayoutGroup.cellSize = new Vector2(cropButtonPrefab.GetComponent<RectTransform>().sizeDelta.x, cropButtonPrefab.GetComponent<RectTransform>().sizeDelta.y);
        gridTransform.GetComponent<RectTransform>().sizeDelta = new Vector2((gardenScript.width - 2) * gridLayoutGroup.cellSize.x, (gardenScript.length - 2) * gridLayoutGroup.cellSize.y);

        for(int i = 0; i < gardenScript.crops.Length; i++)
        {
            GameObject cropButton = Instantiate(cropButtonPrefab, gridTransform);
            CropButtonScript cropButtonScript = cropButton.GetComponent<CropButtonScript>();
            cropButtonScript.gardenEditor = this;
            cropButtonScript.cropImage.sprite = gardenScript.islandScript.islandCellScript.islandEditorScript.cropSprites[(int)gardenScript.crops[i]];
            cropButtonScript.index = i;
        }
    }

    public void DropdownChange(int value)
    {
        selectedCropType = (ResourceScript.CropType)value;
    }

    public void ChangeCrop(CropButtonScript cropButtonScript)
    {
        gardenScript.crops[cropButtonScript.index] = selectedCropType;
        cropButtonScript.cropImage.sprite = gardenScript.islandScript.islandCellScript.islandEditorScript.cropSprites[(int)selectedCropType];
    }
}
