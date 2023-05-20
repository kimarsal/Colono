
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class InventoryScript
{
    private const int materialCapacity = 50;
    private const int cropCapacity = 20;
    private const int meatCapacity = 20;
    [JsonProperty] private InventoryCategory[] inventoryCategories = null;

    private void InitializeResources()
    {
        inventoryCategories = new InventoryCategory[3];
        inventoryCategories[0] = new InventoryCategory(ResourceScript.GetEnumLength(ResourceScript.ResourceType.Material));
        inventoryCategories[1] = new InventoryCategory(ResourceScript.GetEnumLength(ResourceScript.ResourceType.Crop));
        inventoryCategories[2] = new InventoryCategory(ResourceScript.GetEnumLength(ResourceScript.ResourceType.Meat));
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

    public int GetCategoryLevel(int type)
    {
        return inventoryCategories[type].level;
    }

    public void LevelUpInventoryCategory(int type)
    {
        inventoryCategories[type].level++;
    }

    public bool CanWarehouseBeRemoved()
    {
        return inventoryCategories[0].capacity - inventoryCategories[0].usage < materialCapacity * inventoryCategories[0].level
            && inventoryCategories[1].capacity - inventoryCategories[1].usage < cropCapacity * inventoryCategories[1].level
            && inventoryCategories[2].capacity - inventoryCategories[2].usage < meatCapacity * inventoryCategories[2].level;
    }
}

[JsonObject(MemberSerialization.OptIn)]
public class InventoryCategory
{
    [JsonProperty] public int level;
    [JsonProperty] private int baseCapacity;
    public int capacity { get { return baseCapacity * level; } }
    [JsonProperty] public int usage;
    [JsonProperty] public int[] resources;

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