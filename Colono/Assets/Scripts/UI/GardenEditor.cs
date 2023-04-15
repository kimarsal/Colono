using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ResourceScript;

public class GardenEditor : EditorScript
{
    private GardenScript gardenScript;

    public Dropdown cropDropdown;
    public Transform gridTransform;
    public GridLayoutGroup gridLayoutGroup;
    public GameObject cropButtonPrefab;

    public CropType selectedCropType;

    public override void SetEditor(ConstructionScript constructionScript)
    {
        base.SetEditor(constructionScript);

        gardenScript = (GardenScript)constructionScript;

        foreach (Transform cropButton in gridTransform)
        {
            Destroy(cropButton.gameObject);
        }
        gridLayoutGroup.constraintCount = gardenScript.width - 2;

        foreach(KeyValuePair<Vector2, CropType> pair in gardenScript.cropDictionary)
        {
            GameObject cropButton = Instantiate(cropButtonPrefab, gridTransform);
            CropButtonScript cropButtonScript = cropButton.GetComponent<CropButtonScript>();
            cropButtonScript.gardenEditor = this;
            cropButtonScript.cropImage.sprite = ResourceScript.Instance.GetResourceSprite(ResourceType.Crop, (int)pair.Value);
            cropButtonScript.cell = pair.Key;
        }
    }

    public void DropdownChange(int value)
    {
        selectedCropType = (CropType)value;
    }

    public void ChangeCrop(CropButtonScript cropButtonScript)
    {
        gardenScript.cropDictionary[cropButtonScript.cell] = selectedCropType;
        cropButtonScript.cropImage.sprite = ResourceScript.Instance.GetResourceSprite(ResourceType.Crop, (int)selectedCropType);
    }
}
