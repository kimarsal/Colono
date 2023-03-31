using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarehouseScript : BuildingScript
{
    public override EditorScript editorScript { get { return CanvasScript.Instance.inventoryEditor; } }

    private void Start()
    {
        islandScript.inventoryScript.AddCapacityToAllCategories();
    }

    public override void FinishUpBusiness()
    {
        islandScript.inventoryScript.RemoveCapacityFromAllCategories();
    }
}
