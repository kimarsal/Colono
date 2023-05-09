using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class ItemScript : TaskScript
{
    public enum ActionType { Chop, Dig, Pull, Pick }
    public ActionType actionType;
    [JsonProperty] public int orientation;

    public ResourceScript.MaterialType materialType;
    [JsonProperty] public int materialAmount;
    [JsonProperty] public Terrain.TerrainType terrainType;
    [JsonProperty] public int itemIndex;

    public IslandScript islandScript;
    [JsonProperty] public bool isScheduledForClearing;
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

    public void Initialize(KeyValuePair<Vector2, ItemScript> pair)
    {
        Vector3 itemPos = islandScript.transform.position + MeshGenerator.GetCellCenter(pair.Key, islandScript.meshData);
        transform.position = itemPos;
        transform.rotation = Quaternion.Euler(0, pair.Value.orientation, 0);
        transform.parent = islandScript.itemsTransform.transform;
        terrainType = pair.Value.terrainType;
        itemIndex = pair.Value.itemIndex;
        cell = pair.Key;
        orientation = pair.Value.orientation;
        materialAmount = pair.Value.materialAmount;

        if (isScheduledForClearing) islandScript.itemsToClear.Add(this);
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
        islandScript.AddResource(center, ResourceScript.ResourceType.Material, (int)materialType);
        if (materialAmount == 0)
        {
            islandScript.RemoveItemToClear(this);
            islandScript.RemoveItemAtCell(cell);
            Destroy(gameObject);
        }
    }
}
