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
        //cropType = (ResourceScript.CropType)Random.Range(0, System.Enum.GetNames(typeof(ResourceScript.CropType)).Length);
        
        if (gardenScript.islandScript.UseResource(ResourceScript.ResourceType.Crop, (int)cropType))
        {
            orientation = Quaternion.Euler(0f, Random.Range(0, 359), 0f);
            cropState = CropState.Planted;
            crop = Instantiate(GetCropPrefab(), center, orientation, transform);
        }
        else
        {
            peasantScript.task = ((GardenScript)peasantScript.constructionScript).GetNextPendingTask();
            peasantScript.task.peasantScript = peasantScript;
            peasantScript.UpdateTask();
            peasantScript = null;
        }
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

        PeasantAdultScript p = peasantScript;
        peasantScript = null;
        if (p == null)
        {
            Debug.Log("Fuck bitches");
        }
        else if (p.constructionScript != null && p.constructionScript == gardenScript) //Si el granjer encara no ha marxat
        {
            if (p.hunger < 1 && p.exhaustion < 1) //Si el granjer no t� gana i no est� cansat
            {
                p.task = ((GardenScript)p.constructionScript).GetNextPendingTask();
                p.task.peasantScript = p;
            }
            else
            {
                p.task = null;
            }
            p.UpdateTask();
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

}
