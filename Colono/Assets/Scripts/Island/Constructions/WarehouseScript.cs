using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarehouseScript : BuildingScript
{
    public override void EditConstruction()
    {
        islandScript.gameManager.canvasScript.ShowInventoryEditor();
    }
}
