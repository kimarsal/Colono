using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ZoneScript : ConstructionScript, IPointerClickHandler
{
    public enum ZoneType { Orchard, Barn };
    public ZoneType type;

    public GameObject openGate;
    public GameObject closedGate;
    public Dictionary<Vector2, CropScript> crops = new Dictionary<Vector2, CropScript>();

    public void OnPointerClick(PointerEventData eventData)
    {
        //islandCellScript.SelectZone(gameObject, type);
    }

    public void ToggleGate()
    {
        openGate.SetActive(!openGate.activeInHierarchy);
        closedGate.SetActive(!closedGate.activeInHierarchy);
    }

    public void AddCrop(Vector2 cell, CropScript crop)
    {
        RemoveCrop(cell);
        crops.Add(cell, crop);
    }

    public void RemoveCrop(Vector2 cell)
    {
        if (crops.ContainsKey(cell))
        {
            Destroy(crops[cell].gameObject);
            crops.Remove(cell);
        }
    }

    public bool isPatchEmpty(Vector2[] selectedCells)
    {
        foreach(Vector2 cell in selectedCells)
        {
            if (crops.ContainsKey(cell)) return false;
        }
        return true;
    }
}
