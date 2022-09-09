using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingScript : ConstructionScript, IPointerClickHandler
{
    public enum BuildingType { WoodHouse, StoneHouse, Mine};
    public BuildingType type;

    public int orientation;

    public void OnPointerClick(PointerEventData eventData)
    {
        //islandCellScript.SelectBuilding(gameObject, type);
    }

    public void EnableCollider()
    {
        //GetComponent<BoxCollider>().enabled = true;
    }
}
