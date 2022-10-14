using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IslandEditor : MonoBehaviour
{
    [Header("NPCs")]
    public GameObject malePeasantPrefab;
    public GameObject femalePeasantPrefab;
    public GameObject childPeasantPrefab;
    public GameObject maleWarriorPrefab;
    public GameObject femaleWarriorPrefab;

    [Header("Cell materials")]
    public Material hoverMaterial;
    public Material selectingMaterial;
    public Material selectingFirstCellMaterial;
    public Material selectedMaterial;
    public Material invalidSelectionMaterial;
    public Material selectedHoverMaterial;
    public Material patchMaterial;

    [Header("Buildings")]
    public GameObject warehouse;
    public GameObject cabin;
    public GameObject tavern;
    public GameObject alchemist;
    public GameObject mine;

    [Header("Fences")]
    public GameObject[] fences;
    public GameObject post;
    public GameObject gateClosed;
    public GameObject gateOpen;

    [Header("Crops")]
    public GameObject[] corn;
    public GameObject[] cucumber;
    public GameObject[] grape;
    public GameObject[] pepper;
    public GameObject[] potato;
    public GameObject[] tomato;
    public GameObject[] grass;

    [Header("Items")]
    public GameObject[] beachItems;
    public GameObject[] fieldItems;
    public GameObject[] hillItems;
    public GameObject[] mountainItems;

    [Header("Others")]
    public GameObject itemClearingCanvas;
    public GameObject constructionCenterPrefab;
    public GameObject coastObstacle;
}
