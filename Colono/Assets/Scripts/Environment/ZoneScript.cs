using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ZoneScript : MonoBehaviour, IPointerClickHandler
{
    public IslandCellScript islandCellScript;
    public Vector2[] cells;

    public void OnPointerClick(PointerEventData eventData)
    {
        islandCellScript.ZoneSelected(gameObject);
    }
}
