using UnityEngine;

public class NativeCropSproutScript : ItemScript
{
    public override void TaskProgress()
    {
        int cropTypes = System.Enum.GetValues(typeof(ResourceScript.CropType)).Length;
        islandScript.AddResource(ResourceScript.ResourceType.Crop, Random.Range(cropTypes / 2, cropTypes));
        islandScript.RemoveItemToClear(this);
        islandScript.RemoveItemAtCell(itemCell);
        Destroy(gameObject);
    }
}
