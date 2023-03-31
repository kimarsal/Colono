using UnityEngine;
using static PatchScript;

public class PatchScript : TaskScript
{
    public enum CropState { Planted, Grown, Blossomed, Dead, Barren }

    private GardenScript gardenScript;
    public Vector2 cell;
    private int orientation;
    public GameObject crop;
    public ResourceScript.CropType cropType;
    public CropState cropState = CropState.Barren;

    private float timeSinceLastStateChange;
    private float timeSinceLastTakenCareOf;
    private const float timeBetweenStates = 60f;
    private const float maxUnattendedTime = 60f;

    public void InitializePatch(PatchInfo patchInfo)
    {
        orientation = patchInfo.orientation;
        crop = Instantiate(IslandEditor.Instance.GetCropPrefab(cropType, patchInfo.cropState), center, Quaternion.Euler(0, orientation, 0), transform);
    }

    private void Start()
    {
        gardenScript = (GardenScript)taskSourceScript;
        taskType = TaskType.Patch;
    }

    private void Update()
    {
        timeSinceLastStateChange += Time.deltaTime * gardenScript.level;
        timeSinceLastTakenCareOf += Time.deltaTime / gardenScript.level;

        if (!isBeingTakenCareOf)
        {
            if(timeSinceLastTakenCareOf > maxUnattendedTime && cropState != CropState.Barren)
            {
                cropState = CropState.Dead;
                ChangeCropState();
            }
            else if(timeSinceLastStateChange > timeBetweenStates && cropState < CropState.Blossomed)
            {
                cropState++;
                ChangeCropState();
            }
        }
    }

    private void ChangeCropState()
    {
        Destroy(crop);
        if(cropState < CropState.Barren)
        {
            crop = Instantiate(IslandEditor.Instance.GetCropPrefab(cropType, cropState), center, Quaternion.Euler(0, orientation, 0), transform);
        }
        timeSinceLastStateChange = 0;
    }

    public override void TaskProgress()
    {
        if (cropState == CropState.Dead || cropType != gardenScript.cropDictionary[cell]) // S'arrenca l'anterior planta
        {
            cropState = CropState.Barren;
            cropType = gardenScript.cropDictionary[cell];
            Destroy(crop);
        }
        else if (cropState == CropState.Barren) // Es planta una llavor
        {
            cropType = gardenScript.cropDictionary[cell];
            orientation = Random.Range(0, 359);
            cropState = CropState.Planted;
            crop = Instantiate(IslandEditor.Instance.GetCropPrefab(cropType, cropState), center, Quaternion.Euler(0, orientation, 0), transform);
        }
        else if (cropState == CropState.Blossomed) // Es cullen els fruits
        {
            gardenScript.islandScript.AddResource(ResourceScript.ResourceType.Crop, (int)cropType, 3);
            cropState = CropState.Grown;
            ChangeCropState();
        }
        isBeingTakenCareOf = false;
        timeSinceLastTakenCareOf = 0;

        base.TaskProgress();
    }

    public override void CancelTask()
    {
        base.CancelTask();
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