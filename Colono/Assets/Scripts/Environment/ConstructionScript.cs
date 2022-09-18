using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ConstructionScript : MonoBehaviour
{
    public IslandScript islandScript;
    public Vector2[] cells;
    public int length;
    public int width;
    public Transform center;
    public bool isEnclosure;
    public int numPeasants;
}
