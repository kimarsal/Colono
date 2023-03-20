using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarehouseScript : BuildingScript
{
    public override EditorScript editorScript { get { return CanvasScript.Instance.inventoryEditor; } }
}
