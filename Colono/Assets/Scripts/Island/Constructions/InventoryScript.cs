using System;

[System.Serializable]
public class InventoryScript
{
    private InventoryCategory[] inventoryCategories = null;

    private void InitializeResources()
    {
        inventoryCategories = new InventoryCategory[3];
        inventoryCategories[0] = new InventoryCategory(Enum.GetValues(typeof(ResourceScript.MaterialType)).Length);
        inventoryCategories[1] = new InventoryCategory(Enum.GetValues(typeof(ResourceScript.CropType)).Length);
        inventoryCategories[2] = new InventoryCategory(Enum.GetValues(typeof(ResourceScript.MeatType)).Length);
    }

    public string GetUsedCapacity()
    {
        if (inventoryCategories == null) InitializeResources();

        int usage = 0;
        int capacity = 0;
        for (int i = 0; i < inventoryCategories.Length; i++)
        {
            usage += inventoryCategories[i].usage;
            capacity += inventoryCategories[i].capacity;
        }
        return usage + "/" + capacity;
    }

    public int GetResourceAmount(ResourceScript.ResourceType resourceType, int resourceIndex)
    {
        if (inventoryCategories == null) InitializeResources();
        return inventoryCategories[(int)resourceType].resources[resourceIndex];
    }

    public int AddResource(ResourceScript.ResourceType resourceType, int resourceIndex, int amount = 1)
    {
        if (inventoryCategories == null) InitializeResources();

        int originalAmount = amount;
        InventoryCategory inventoryCategory = inventoryCategories[(int)resourceType];
        if (inventoryCategory.capacity - inventoryCategory.usage < originalAmount)
        {
            amount = inventoryCategory.capacity - inventoryCategory.usage;
        }

        inventoryCategory.resources[resourceIndex] += amount;
        inventoryCategory.usage += amount;

        return originalAmount - amount;

    }

    public int RemoveResource(ResourceScript.ResourceType resourceType, int resourceIndex, int amount = 1)
    {
        int originalAmount = amount;
        InventoryCategory inventoryCategory = inventoryCategories[(int)resourceType];
        if (GetResourceAmount(resourceType, resourceIndex) < amount)
        {
            amount = inventoryCategory.resources[resourceIndex];
        }

        inventoryCategory.resources[resourceIndex] -= amount;
        inventoryCategory.usage -= amount;

        return originalAmount - amount;
    }

    public void AddCapacityToAllCategories(int extraCapacity)
    {
        if (inventoryCategories == null) InitializeResources();

        for (int i = 0; i < inventoryCategories.Length; i++)
        {
            inventoryCategories[i].capacity += extraCapacity;
        }
    }

    public void AddCapacityToCategory(ResourceScript.ResourceType resourceType, int extraCapacity)
    {
        inventoryCategories[(int)resourceType].capacity += extraCapacity;
    }
}

public class InventoryCategory
{
    public int capacity;
    public int usage;
    public int[] resources;

    public InventoryCategory(int resourceTypeAmount)
    {
        capacity = 30;
        resources = new int[resourceTypeAmount];
    }
}