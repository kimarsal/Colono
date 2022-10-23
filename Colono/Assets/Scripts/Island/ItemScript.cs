using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemScript : TaskScript
{
    public enum ItemType { Chop, Dig, Pull, Pick }
    public ItemType itemType;
    public Vector2 itemCell;

    public ResourceScript.MaterialType materialType;
    public int materialAmount;

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
                islandScript.npcManager.RemoveItemToClear(this);
            }
            return true;
        }
        return false;
    }

    public override void TaskProgress()
    {
        materialAmount--;
        islandScript.AddMaterial(materialType);
        if (materialAmount == 0)
        {
            islandScript.npcManager.RemoveItemToClear(this);
            islandScript.RemoveItemAtCell(itemCell);
            Destroy(gameObject);
        }
    }
}
