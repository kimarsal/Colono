using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IslandEditor : MonoBehaviour
{
    [Header("Cell materials")]
    public Material hoverMaterial;
    public Material selectingMaterial;
    public Material selectingFirstCellMaterial;
    public Material selectedMaterial;
    public Material invalidSelectionMaterial;
    public Material selectedHoverMaterial;

    [Header("Buildings")]
    public GameObject warehouse;
    public GameObject residence;
    public GameObject mine;

    [Header("Fences")]
    public GameObject[] fences;
    public GameObject post;
    public GameObject gateClosed;
    public GameObject gateOpen;

    [Header("Crops")]
    public GameObject tomato;

    [Header("Decorations")]
    public GameObject[] trees;
    public GameObject[] bushes;
    public GameObject[] rocks;
    public GameObject[] flowers;
    public GameObject[] miscellaneous;

    [Header("Others")]
    public GameObject constructionCanvas;
    public GameObject coastObstacle;
}
