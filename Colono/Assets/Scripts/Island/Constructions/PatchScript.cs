using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PatchScript;

public class PatchScript : TaskScript
{
    public GardenScript gardenScript;
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
        crop = Instantiate(gardenScript.islandEditor.GetCropPrefab(cropType, patchInfo.cropState), center, Quaternion.Euler(0, orientation, 0), transform);
    }

    public void PlantCrop()
    {
        orientation = Random.Range(0, 359);
        cropState = CropState.Planted;
        crop = Instantiate(gardenScript.islandEditor.GetCropPrefab(cropType, cropState), center, Quaternion.Euler(0, orientation, 0), transform);
    }

    public override void TaskProgress()
    {
        if (cropType != gardenScript.cropDictionary[cell])
        {
            cropState = CropState.Barren;
            cropType = gardenScript.cropDictionary[cell];
            Destroy(crop);
        }
        else if (cropState == CropState.Barren)
        {
            cropType = gardenScript.cropDictionary[cell];
            PlantCrop();
        }
        else
        {
            if (cropState < CropState.Blossomed)
            {
                cropState++;
            }
            else //if (cropState == CropState.Blossomed)
            {
                gardenScript.islandScript.AddResource(ResourceScript.ResourceType.Crop, (int)cropType, 3);
                cropState = CropState.Grown;
            }
            Destroy(crop);
            crop = Instantiate(gardenScript.islandEditor.GetCropPrefab(cropType, cropState), center, Quaternion.Euler(0, orientation, 0), transform);
        }

        if (peasantScript != null) //Si el granjer encara no ha marxat
        {
            if (peasantScript.hunger < 1 && peasantScript.exhaustion < 1) //Si el granjer no té gana i no està cansat
            {
                PatchScript nextPatch = (PatchScript)gardenScript.GetNextPendingTask();
                peasantScript.task = nextPatch;
                if(nextPatch != null)
                {
                    nextPatch.peasantScript = peasantScript;
                    gardenScript.islandScript.UseResource(ResourceScript.ResourceType.Crop, (int)nextPatch.cropType);
                }
            }
            else
            {
                peasantScript.task = null;
            }
            peasantScript.UpdateTask();
            peasantScript = null;
        }
    }

    public override void CancelTask()
    {
        if (cropState == CropState.Barren)
        {
            gardenScript.islandScript.AddResource(ResourceScript.ResourceType.Crop, (int)cropType);
        }
    }

    public PatchInfo GetPatchInfo()
    {
        PatchInfo patchInfo = new PatchInfo();
        patchInfo.cell = new SerializableVector2(cell);
        patchInfo.orientation = orientation;
        patchInfo.cropType = cropType;
        patchInfo.cropState = cropState;
        return patchInfo;
    }
}

[System.Serializable]
public class PatchInfo
{
    public SerializableVector2 cell;
    public int orientation;
    public ResourceScript.CropType cropType;
    public CropState cropState;
}