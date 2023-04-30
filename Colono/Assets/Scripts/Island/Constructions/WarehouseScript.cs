

using UnityEngine;

public class WarehouseScript : BuildingScript
{
    public override Sprite sprite { get { return ResourceScript.Instance.warehouseSprite; } }
    public override EditorScript editorScript { get { return CanvasScript.Instance.inventoryEditor; } }

    public override void FinishUpBusiness()
    {
        islandScript.inventoryScript.RemoveCapacityFromAllCategories();
    }
}
