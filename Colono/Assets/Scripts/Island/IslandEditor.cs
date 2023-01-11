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

    [Header("Resources")]
    public Sprite[] materialSprites;
    public Sprite[] cropSprites;
    public Sprite[] animalSprites;
    public Sprite[] meatSprites;

    [Header("Crops")]
    public GameObject[] onion;
    public GameObject[] carrot;
    public GameObject[] eggplant;
    public GameObject[] cucumber;
    public GameObject[] cabbage;

    public GameObject[] potato;
    public GameObject[] tomato;
    public GameObject[] zucchini;
    public GameObject[] pepper;
    public GameObject[] corn;

    public GameObject[] grass;

    [Header("Animals")]
    public GameObject[] animals;

    [Header("Recipes")]
    public Sprite[] cookedVegetableSprites;
    public Sprite[] cookedMeatSprites;

    [Header("Items")]
    public GameObject[] beachItems;
    public GameObject[] fieldItems;
    public GameObject[] hillItems;
    public GameObject[] mountainItems;

    [Header("Others")]
    public GameObject coastObstacle;
    public GameObject enclosureCenter;

    public Sprite GetResourceSprite(ResourceScript.ResourceType resourceType, int resourceIndex)
    {
        switch (resourceType)
        {
            case ResourceScript.ResourceType.Material: return materialSprites[resourceIndex];
            case ResourceScript.ResourceType.Crop: return cropSprites[resourceIndex];
            case ResourceScript.ResourceType.Meat: return meatSprites[resourceIndex];
            case ResourceScript.ResourceType.Animal: return animalSprites[resourceIndex];
        }
        return null;
    }
}
