using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarehouseScript : BuildingScript
{
    public int capacity;
    public int usage;
    public int[] materials = new int[Enum.GetValues(typeof(ResourceScript.MaterialType)).Length];
    public int[] crops = new int[Enum.GetValues(typeof(ResourceScript.CropType)).Length];

    public int AddMaterials(int materialType, int materialAmount)
    {
        if (capacity - usage < materialAmount) materialAmount = capacity - usage;

        materials[materialType] += materialAmount;
        usage += materialAmount;
        return materialAmount;
    }

    public int AddCrops(int cropType, int cropAmount)
    {
        if (capacity - usage < cropAmount) cropAmount = capacity - usage;

        crops[cropType] += cropAmount;
        usage += cropAmount;
        return cropAmount;
    }

    public int RemoveMaterials(int materialType, int materialAmount)
    {
        if (materials[materialType] < materialAmount) materialAmount = materials[materialType];

        materials[materialType] -= materialAmount;
        usage -= materialAmount;
        return materialAmount;
    }

    public int RemoveCrops(int cropType, int cropAmount)
    {
        if (crops[cropType] < cropAmount) cropAmount = crops[cropType];

        crops[cropType] -= cropAmount;
        usage -= cropAmount;
        return cropAmount;
    }
}
