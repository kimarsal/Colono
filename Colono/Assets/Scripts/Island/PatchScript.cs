using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatchScript : TaskScript
{
    public IslandEditor islandEditor;
    public enum CropType { Corn, Cucumber, Grape, Pepper, Potato, Tomato }
    public enum CropState { Planted, Grown, Blossomed, Dead, Barren }
    public Vector2 cell;
    private GameObject crop;
    private Quaternion orientation;
    public CropType cropType;
    public CropState cropState = CropState.Barren;
    private float timeSinceLastChange = 0f;

    private void Start()
    {
        taskType = TaskType.Patch;
    }

    public void PlantCrop(CropType type)
    {
        cropType = type;
        orientation = Quaternion.Euler(0f, Random.Range(0, 359), 0f);
        cropState = CropState.Planted;
        crop = Instantiate(GetCropPrefab(), center, orientation, transform);
    }

    void Update()
    {
        timeSinceLastChange += Time.deltaTime;

        if (timeSinceLastChange > 2f)
        {
            if(cropState < CropState.Blossomed)
            {
                cropState++;
                Destroy(crop);
                crop = Instantiate(GetCropPrefab(), center, orientation, transform);
            }
            timeSinceLastChange = 0;
        }
    }

    private GameObject GetCropPrefab()
    {
        GameObject prefab = null;
        switch (cropType)
        {
            case CropType.Corn: prefab = islandEditor.corn[(int)cropState]; break;
            case CropType.Cucumber: prefab = islandEditor.cucumber[(int)cropState]; break;
            case CropType.Grape: prefab = islandEditor.grape[(int)cropState]; break;
            case CropType.Pepper: prefab = islandEditor.pepper[(int)cropState]; break;
            case CropType.Potato: prefab = islandEditor.potato[(int)cropState]; break;
            case CropType.Tomato: prefab = islandEditor.tomato[(int)cropState]; break;
        }
        if (prefab == null) prefab = islandEditor.grass[Random.Range(0, islandEditor.grass.Length)];
        return prefab;
    }
}
