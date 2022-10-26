using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatchScript : TaskScript
{
    public GardenScript gardenScript;
    public IslandEditor islandEditor;
    public enum CropState { Planted, Grown, Blossomed, Dead, Barren }
    public Vector2 cell;
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
        //cropType = (ResourceScript.CropType)Random.Range(0, System.Enum.GetNames(typeof(ResourceScript.CropType)).Length);
        
        if (gardenScript.islandScript.UseCrop(cropType))
        {
            orientation = Quaternion.Euler(0f, Random.Range(0, 359), 0f);
            cropState = CropState.Planted;
            crop = Instantiate(GetCropPrefab(), center, orientation, transform);
        }
        else
        {
            TaskScript patch = ((GardenScript)peasantScript.constructionScript).GetNextPendingTask();
            patch.peasantScript = peasantScript;
            peasantScript.task = patch;
            peasantScript.UpdateTask();
            peasantScript = null;
        }
    }

    public override void TaskProgress()
    {
        if (cropState == CropState.Barren) PlantCrop();
        else
        {
            if (cropState < CropState.Blossomed)
            {
                cropState++;
                Destroy(crop);
                crop = Instantiate(GetCropPrefab(), center, orientation, transform);
            }
            else if (cropState == CropState.Blossomed)
            {
                gardenScript.islandScript.AddCrops(cropType, 3);
                cropState = CropState.Grown;
                Destroy(crop);
                crop = Instantiate(GetCropPrefab(), center, orientation, transform);
            }
        }

        PeasantAdultScript p = peasantScript;
        peasantScript = null;
        if (p.constructionScript != null && p.constructionScript == gardenScript) //Si el granjer encara no ha marxat
        {
            TaskScript patch = ((GardenScript)p.constructionScript).GetNextPendingTask();
            patch.peasantScript = p;
            p.task = patch;
            p.UpdateTask();
        }
    }

    private GameObject GetCropPrefab()
    {
        GameObject prefab = null;
        switch (cropType)
        {
            case ResourceScript.CropType.Corn: prefab = islandEditor.corn[(int)cropState]; break;
            case ResourceScript.CropType.Cucumber: prefab = islandEditor.cucumber[(int)cropState]; break;
            case ResourceScript.CropType.Grape: prefab = islandEditor.grape[(int)cropState]; break;
            case ResourceScript.CropType.Pepper: prefab = islandEditor.pepper[(int)cropState]; break;
            case ResourceScript.CropType.Potato: prefab = islandEditor.potato[(int)cropState]; break;
            case ResourceScript.CropType.Tomato: prefab = islandEditor.tomato[(int)cropState]; break;
        }
        if (prefab == null) prefab = islandEditor.grass[Random.Range(0, islandEditor.grass.Length)];
        return prefab;
    }

}
