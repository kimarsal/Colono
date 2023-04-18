

public class WarehouseScript : BuildingScript
{
    public override EditorScript editorScript { get { return CanvasScript.Instance.inventoryEditor; } }

    public override void FinishUpBusiness()
    {
        islandScript.inventoryScript.RemoveCapacityFromAllCategories();
    }
}
