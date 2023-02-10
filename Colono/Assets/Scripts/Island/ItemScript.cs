using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class ItemScript : TaskScript
{
    public enum ActionType { None, Chop, Dig, Pull, Pick }
    [JsonIgnore] public ActionType actionType;
    public Vector2 itemCell;
    public int orientation;

    [JsonIgnore] public ResourceScript.MaterialType materialType;
    public int materialAmount;
    public Terrain.TerrainType terrainType;
    public int itemIndex;

    [JsonIgnore] public IslandScript islandScript;
    public bool isScheduledForClearing;
    [JsonIgnore] private Outline outline;

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

    public void ChangeItemClearingState(bool toClear)
    {
        if (isScheduledForClearing == toClear) return;

        isScheduledForClearing = toClear;
        outline.enabled = toClear;
        if (toClear) islandScript.AddItemToClear(this);
        else islandScript.RemoveItemToClear(this);
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
}
