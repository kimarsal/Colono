using System;

[Serializable]
public class InventoryScript
{
    private const int materialCapacity = 50;
    private const int cropCapacity = 20;
    private const int meatCapacity = 20;
    private const int maxLevel = 10;
    private InventoryCategory[] inventoryCategories = null;

    private void InitializeResources()
    {
        inventoryCategories = new InventoryCategory[3];
        inventoryCategories[0] = new InventoryCategory(Enum.GetValues(typeof(ResourceScript.MaterialType)).Length);
        inventoryCategories[1] = new InventoryCategory(Enum.GetValues(typeof(ResourceScript.CropType)).Length);
        inventoryCategories[2] = new InventoryCategory(Enum.GetValues(typeof(ResourceScript.MeatType)).Length);
    }

    public string GetUsedCapacity(int type)
    {
        if (inventoryCategories == null) InitializeResources();

        return inventoryCategories[type].usage + "/" + inventoryCategories[type].capacity;
    }

    public bool IsFull()
    {
        if (inventoryCategories == null) InitializeResources();

        int usage = 0;
        int capacity = 0;

        for(int i = 0; i < inventoryCategories.Length; i++)
        {
            usage += inventoryCategories[i].usage;
            capacity += inventoryCategories[i].capacity;
        }
        return usage == capacity;

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

    public void AddCapacityToAllCategories()
    {
        if (inventoryCategories == null) InitializeResources();

        inventoryCategories[0].AddCapacity(materialCapacity);
        inventoryCategories[1].AddCapacity(cropCapacity);
        inventoryCategories[2].AddCapacity(meatCapacity);
    }

    public void RemoveCapacityFromAllCategories()
    {
        if (inventoryCategories == null) InitializeResources();

        inventoryCategories[0].AddCapacity(-materialCapacity);
        inventoryCategories[1].AddCapacity(-cropCapacity);
        inventoryCategories[2].AddCapacity(-meatCapacity);
    }

    public bool CanLevelUpCategory(int type)
    {
        return inventoryCategories[type].capacity > 0 && inventoryCategories[type].level < maxLevel;
    }

    public void LevelUpInventoryCategory(int type)
    {
        inventoryCategories[type].level++;
    }
}

[Serializable]
public class InventoryCategory
{
    public int level;
    private int baseCapacity;
    public int capacity { get { return baseCapacity * level; } }
    public int usage;
    public int[] resources;

    public InventoryCategory(int resourceTypeAmount)
    {
        level = 1;
        resources = new int[resourceTypeAmount];
    }

    public void AddCapacity(int extraCapacity)
    {
        baseCapacity += extraCapacity;
    }
}