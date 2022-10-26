using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ResourceScript;
using static UnityEngine.Rendering.DebugUI.Table;

public class GardenEditor : MonoBehaviour
{
    public GardenScript gardenScript;

    public Transform listTransform;
    private CropButtonScript[] listButtons;
    public Transform gridTransform;
    public GridLayoutGroup gridLayoutGroup;
    public GameObject cropButtonPrefab;

    public ResourceScript.CropType selectedCropType;

    private void Awake()
    {
        listTransform.GetComponent<RectTransform>().sizeDelta = new Vector2(listTransform.GetComponent<RectTransform>().sizeDelta.x,
            System.Enum.GetValues(typeof(ResourceScript.CropType)).Length * cropButtonPrefab.GetComponent<RectTransform>().sizeDelta.y);
    }

    public void SetGrid()
    {
        foreach (Transform cropButton in listTransform)
        {
            Destroy(cropButton.gameObject);
        }
        foreach (Transform cropButton in gridTransform)
        {
            Destroy(cropButton.gameObject);
        }

        listButtons = new CropButtonScript[System.Enum.GetValues(typeof(ResourceScript.CropType)).Length];
        for (int i = 0; i < listButtons.Length; i++)
        {
            GameObject cropButton = Instantiate(cropButtonPrefab, listTransform);
            CropButtonScript cropButtonScript = cropButton.GetComponent<CropButtonScript>();
            cropButtonScript.gardenEditor = this;
            cropButtonScript.cropType = (ResourceScript.CropType)i;
            cropButtonScript.cropImage.sprite = gardenScript.islandScript.islandCellScript.islandEditorScript.cropSprites[i];
            listButtons[i] = cropButtonScript;
        }

        gridLayoutGroup.cellSize = new Vector2(cropButtonPrefab.GetComponent<RectTransform>().sizeDelta.x, cropButtonPrefab.GetComponent<RectTransform>().sizeDelta.y);
        gridTransform.GetComponent<RectTransform>().sizeDelta = new Vector2((gardenScript.width - 2) * gridLayoutGroup.cellSize.x, (gardenScript.length - 2) * gridLayoutGroup.cellSize.y);
        foreach (KeyValuePair<Vector2, PatchScript> pair in gardenScript.patchesDictionary)
        {
            GameObject cropButton = Instantiate(cropButtonPrefab, gridTransform);
            CropButtonScript cropButtonScript = cropButton.GetComponent<CropButtonScript>();
            cropButtonScript.gardenEditor = this;
            cropButtonScript.cropType = pair.Value.cropType;
            cropButtonScript.cropImage.sprite = gardenScript.islandScript.islandCellScript.islandEditorScript.cropSprites[(int)pair.Value.cropType];
            cropButtonScript.cell = pair.Key;
            cropButtonScript.isOnGrid = true;
        }
    }

    public void SelectCrop(ResourceScript.CropType cropType)
    {
        selectedCropType = cropType;
        for (int i = 0; i < listButtons.Length; i++)
        {
            listButtons[i].gameObject.GetComponent<Button>().enabled = i != (int)cropType;
        }
    }

    public void ChangeCrop(CropButtonScript cropButtonScript)
    {
        //gardenScript.patchesDictionary[cropButtonScript.cell].cropType = cropButtonScript.cropType;
        cropButtonScript.cropImage.sprite = gardenScript.islandScript.islandCellScript.islandEditorScript.cropSprites[(int)selectedCropType];
    }
}
