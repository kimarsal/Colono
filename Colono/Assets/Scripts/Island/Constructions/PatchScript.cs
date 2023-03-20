using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PatchScript;

public class PatchScript : TaskScript
{
    public enum CropState { Planted, Grown, Blossomed, Dead, Barren }

    public Vector2 cell;
    private int orientation;
    public GameObject crop;
    public ResourceScript.CropType cropType;
    public CropState cropState = CropState.Barren;

    private void Start()
    {
        taskType = TaskType.Patch;
    }

    public void InitializePatch(PatchInfo patchInfo)
    {
        orientation = patchInfo.orientation;
        crop = Instantiate(IslandEditor.Instance.GetCropPrefab(cropType, patchInfo.cropState), center, Quaternion.Euler(0, orientation, 0), transform);
    }

    public void PlantCrop()
    {
        orientation = Random.Range(0, 359);
        cropState = CropState.Planted;
        crop = Instantiate(IslandEditor.Instance.GetCropPrefab(cropType, cropState), center, Quaternion.Euler(0, orientation, 0), transform);
    }

    public override void TaskProgress()
    {
        GardenScript gardenScript = (GardenScript)taskSourceScript;
        if (cropType != gardenScript.cropDictionary[cell]) // S'arrenca l'anterior planta
        {
            cropState = CropState.Barren;
            cropType = gardenScript.cropDictionary[cell];
            Destroy(crop);
        }
        else if (cropState == CropState.Barren) // Es planta una llavor
        {
            cropType = gardenScript.cropDictionary[cell];
            PlantCrop();
        }
        else
        {
            if (cropState < CropState.Blossomed) // Es rega la planta
            {
                cropState++;
            }
            else // Es cullen els fruits
            {
                gardenScript.islandScript.AddResource(ResourceScript.ResourceType.Crop, (int)cropType, 3);
                cropState = CropState.Grown;
            }
            Destroy(crop);
            crop = Instantiate(IslandEditor.Instance.GetCropPrefab(cropType, cropState), center, Quaternion.Euler(0, orientation, 0), transform);
        }

        base.TaskProgress();
    }

    public override void CancelTask()
    {
        if (cropState == CropState.Barren)
        {
            ((GardenScript)taskSourceScript).islandScript.AddResource(ResourceScript.ResourceType.Crop, (int)cropType);
        }
    }
}

[System.Serializable]
public class PatchInfo
{
    public Vector2 cell;
    public int orientation;
    public ResourceScript.CropType cropType;
    public CropState cropState;
}