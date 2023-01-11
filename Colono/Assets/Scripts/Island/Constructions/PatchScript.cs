using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatchScript : TaskScript
{
    public GardenScript gardenScript;
    public IslandEditor islandEditor;
    public enum CropState { Planted, Grown, Blossomed, Dead, Barren }
    public int index;
    public GameObject crop;
    public ResourceScript.CropType cropType;
    private Quaternion orientation;
    public CropState cropState = CropState.Barren;

    private void Start()
    {
        taskType = TaskType.Patch;
    }

    public void PlantCrop()
    {
        orientation = Quaternion.Euler(0f, Random.Range(0, 359), 0f);
        cropState = CropState.Planted;
        crop = Instantiate(GetCropPrefab(), center, orientation, transform);
    }

    public override void TaskProgress()
    {
        if (cropType != gardenScript.crops[index])
        {
            cropState = CropState.Barren;
            cropType = gardenScript.crops[index];
            Destroy(crop);
        }
        else if (cropState == CropState.Barren)
        {
            cropType = gardenScript.crops[index];
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
            crop = Instantiate(GetCropPrefab(), center, orientation, transform);
        }

        if (peasantScript != null) //Si el granjer encara no ha marxat
        {
            if (peasantScript.hunger < 1 && peasantScript.exhaustion < 1) //Si el granjer no té gana i no està cansat
            {
                PatchScript nextPatch = (PatchScript)gardenScript.GetNextPendingTask();
                peasantScript.task = nextPatch;
                nextPatch.peasantScript = peasantScript;
            }
            else
            {
                peasantScript.task = null;
            }
            peasantScript.UpdateTask();
            peasantScript = null;
        }
    }

    private GameObject GetCropPrefab()
    {
        GameObject prefab = null;
        switch (cropType)
        {
            case ResourceScript.CropType.Onion: prefab = islandEditor.onion[(int)cropState]; break;
            case ResourceScript.CropType.Carrot: prefab = islandEditor.carrot[(int)cropState]; break;
            case ResourceScript.CropType.Eggplant: prefab = islandEditor.eggplant[(int)cropState]; break;
            case ResourceScript.CropType.Cucumber: prefab = islandEditor.cucumber[(int)cropState]; break;
            case ResourceScript.CropType.Cabbage: prefab = islandEditor.cabbage[(int)cropState]; break;

            case ResourceScript.CropType.Potato: prefab = islandEditor.potato[(int)cropState]; break;
            case ResourceScript.CropType.Tomato: prefab = islandEditor.tomato[(int)cropState]; break;
            case ResourceScript.CropType.Zucchini: prefab = islandEditor.zucchini[(int)cropState]; break;
            case ResourceScript.CropType.Pepper: prefab = islandEditor.pepper[(int)cropState]; break;
            case ResourceScript.CropType.Corn: prefab = islandEditor.corn[(int)cropState]; break;
        }
        if (prefab == null) prefab = islandEditor.grass[(int)cropState];
        return prefab;
    }

    public override void CancelTask()
    {
        if (cropState == CropState.Barren)
        {
            gardenScript.islandScript.AddResource(ResourceScript.ResourceType.Crop, (int)cropType);
        }
    }
}
