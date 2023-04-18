using Newtonsoft.Json;
using UnityEngine;

public class ItemScript : TaskScript
{
    public enum ActionType { Chop, Dig, Pull, Pick }
    public ActionType actionType;
    [JsonProperty] public Vector2 itemCell;
    [JsonProperty] public int orientation;

    public ResourceScript.MaterialType materialType;
    [JsonProperty]public int materialAmount;
    [JsonProperty]public Terrain.TerrainType terrainType;
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

    public void ChangeItemClearingState(bool toClear)
    {
        if (isScheduledForClearing == toClear) return;

        isScheduledForClearing = toClear;
        outline.enabled = toClear;
        if (toClear) islandScript.AddItemToClear(this);
        else islandScript.RemoveItemToClear(this);
    }

    public override void AssignPeasant(PeasantAdultScript newPeasantAdultScript)
    {
        base.AssignPeasant(newPeasantAdultScript);
        peasantIndex = islandScript.peasantList.IndexOf(newPeasantAdultScript);
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
}
