using Newtonsoft.Json;
using UnityEngine;

public class PatchScript : TaskScript
{
    public enum CropState { Planted, Grown, Blossomed, Dead, Barren }

    private GardenScript gardenScript;
    [JsonProperty] public ResourceScript.CropType cropType;
    [JsonProperty] public CropState cropState = CropState.Barren;
    [JsonProperty] private int orientation;
    private GameObject crop;

    [JsonProperty] private float timeSinceLastStateChange;
    [JsonProperty] private float timeSinceLastTakenCareOf;
    public const float timeBetweenStates = 60f;
    public const float maxUnattendedTime = 60f;

    public void InitializePatch(PatchScript patchInfo)
    {
        orientation = patchInfo.orientation;
        crop = Instantiate(ResourceScript.Instance.GetCropPrefab(cropType, patchInfo.cropState), center, Quaternion.Euler(0, orientation, 0), transform);
        crop.transform.localScale = Vector3.one * 0.4f;
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
            crop = Instantiate(ResourceScript.Instance.GetCropPrefab(cropType, cropState), center, Quaternion.Euler(0, orientation, 0), transform);
            crop.transform.localScale = Vector3.one * 0.4f;
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
            crop = Instantiate(ResourceScript.Instance.GetCropPrefab(cropType, cropState), center, Quaternion.Euler(0, orientation, 0), transform);
            crop.transform.localScale = Vector3.one * 0.4f;
        }
        else if (cropState == CropState.Blossomed) // Es cullen els fruits
        {
            gardenScript.islandScript.AddResource(ResourceScript.ResourceType.Crop, (int)cropType, 3);
            cropState = CropState.Grown;
            ChangeCropState();
        }
        isBeingTakenCareOf = false;
        timeSinceLastTakenCareOf = 0;

        peasantAdultScript.task = null;
        taskSourceScript.GetNextPendingTask(peasantAdultScript);
        peasantAdultScript = null;
    }

    public override void CancelTask()
    {
        isBeingTakenCareOf = false;
        if (cropState == CropState.Barren)
        {
            ((GardenScript)taskSourceScript).islandScript.AddResource(ResourceScript.ResourceType.Crop, (int)cropType);
        }
    }
}