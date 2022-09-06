using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingScript : MonoBehaviour, IPointerClickHandler
{
    public enum BuildingType { WoodHouse, StoneHouse, Mine};
    public BuildingType type;
    public int length;
    public int width;
    public int orientation;

    public IslandCellScript islandCellScript;
    public Vector2[] cells;

    public void OnPointerClick(PointerEventData eventData)
    {
        islandCellScript.SelectBuilding(gameObject, type);
    }

    public void EnableCollider()
    {
        GetComponent<BoxCollider>().enabled = true;
    }
}
