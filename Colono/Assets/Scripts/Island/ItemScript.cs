using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemScript : TaskScript
{
    public enum ItemType { Chop, Dig, Pull, Pick }
    public ItemType itemType;
    public Vector2 itemCell;
    public int orientation;

    public ResourceScript.MaterialType materialType;
    public int materialAmount;
    public Terrain.TerrainType terrainType;
    public int itemIndex;

    public IslandScript islandScript;
    public bool isScheduledForClearing;
    private Outline outline;

    private void Start()
    {
        outline = gameObject.AddComponent<Outline>();
        outline.OutlineColor = Color.red;
        outline.OutlineMode = Outline.Mode.OutlineAll;
        outline.OutlineWidth = 10;
        outline.enabled = false;

        taskType = TaskType.Item;
        center = transform.position;
    }

    public bool ChangeItemClearingState(bool toClear)
    {
        if (isScheduledForClearing != toClear)
        {
            isScheduledForClearing = toClear;
            outline.enabled = toClear;
            if (!toClear)
            {
                islandScript.RemoveItemToClear(this);
            }
            return true;
        }
        return false;
    }

    public override void TaskProgress()
    {
        materialAmount--;
        islandScript.AddResource(ResourceScript.ResourceType.Material, (int)materialType);
        if (materialAmount == 0)
        {
            islandScript.RemoveItemToClear(this);
            islandScript.RemoveItemAtCell(itemCell);
            Destroy(gameObject);
        }
    }

    public override void CancelTask()
    {
        return;
    }

    public ItemInfo GetItemInfo()
    {
        ItemInfo itemInfo = new ItemInfo();
        itemInfo.terrainType = terrainType;
        itemInfo.itemIndex = itemIndex;
        itemInfo.cell = new SerializableVector2(itemCell);
        itemInfo.orientation = orientation;
        itemInfo.materialAmount = materialAmount;
        itemInfo.isScheduledForClearing = isScheduledForClearing;
        return itemInfo;
    }
}

[System.Serializable]
public class ItemInfo
{
    public Terrain.TerrainType terrainType;
    public int itemIndex;
    public SerializableVector2 cell;
    public int orientation;
    public int materialAmount;
    public bool isScheduledForClearing;
}
