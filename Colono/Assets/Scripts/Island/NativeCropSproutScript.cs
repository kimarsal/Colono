using UnityEngine;

public class NativeCropSproutScript : ItemScript
{
    public override void TaskProgress()
    {
        int cropTypes = System.Enum.GetValues(typeof(ResourceScript.CropType)).Length;
        islandScript.AddResource(center, ResourceScript.ResourceType.Crop, Random.Range(cropTypes / 2, cropTypes));
        islandScript.RemoveItemToClear(this);
        islandScript.RemoveItemAtCell(cell);
        Destroy(gameObject);
    }
}
