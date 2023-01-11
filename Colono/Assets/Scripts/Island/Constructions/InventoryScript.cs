using System;
using UnityEngine;

public class InventoryScript : MonoBehaviour
{
    public int capacity;
    public int usage;
    public int[][] resources;

    private void Start()
    {
        if (resources == null) InitializeResources();
    }

    private void InitializeResources()
    {
        resources = new int[Enum.GetValues(typeof(ResourceScript.ResourceType)).Length][];
        resources[0] = new int[Enum.GetValues(typeof(ResourceScript.MaterialType)).Length];
        resources[1] = new int[Enum.GetValues(typeof(ResourceScript.CropType)).Length];
        resources[2] = new int[Enum.GetValues(typeof(ResourceScript.MeatType)).Length];
        resources[3] = new int[Enum.GetValues(typeof(ResourceScript.AnimalType)).Length];
    }

    public int GetResourceAmount(ResourceScript.ResourceType resourceType, int resourceIndex)
    {
        if (resources == null) InitializeResources();
        return resources[(int)resourceType][resourceIndex];
    }

    public int AddResource(ResourceScript.ResourceType resourceType, int resourceIndex, int amount = 1)
    {
        int originalAmount = amount;
        if(capacity - usage < originalAmount)
        {
            amount = capacity - usage;
        }
        
        if (resources == null) InitializeResources();
        resources[(int)resourceType][resourceIndex] += amount;
        usage += amount;

        return originalAmount - amount;

    }

    public int RemoveResource(ResourceScript.ResourceType resourceType, int resourceIndex, int amount = 1)
    {
        int originalAmount = amount;
        if(GetResourceAmount(resourceType, resourceIndex) < amount)
        {
            amount = resources[(int)resourceType][resourceIndex];
        }

        resources[(int)resourceType][resourceIndex] -= amount;
        usage -= amount;

        return originalAmount - amount;
    }
}
