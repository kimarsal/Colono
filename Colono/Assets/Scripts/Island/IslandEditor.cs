using UnityEngine;
using static PatchScript;
using static ResourceScript;

public class IslandEditor : MonoBehaviour
{
    [Header("NPCs")]
    [SerializeField] private GameObject malePeasantPrefab;
    [SerializeField] private GameObject femalePeasantPrefab;
    [SerializeField] private GameObject childPeasantPrefab;
    [SerializeField] private GameObject maleWarriorPrefab;
    [SerializeField] private GameObject femaleWarriorPrefab;

    [SerializeField] private Color[] skinColorsCaucassian;
    [SerializeField] private Color[] hairColors;

    [Header("Cell materials")]
    public Material hoverMaterial;
    public Material selectingMaterial;
    public Material selectingFirstCellMaterial;
    public Material selectedMaterial;
    public Material invalidSelectionMaterial;
    public Material selectedHoverMaterial;
    public Material patchMaterial;

    [Header("Buildings")]
    [SerializeField] private GameObject[] buildings;

    [Header("Fences")]
    public GameObject[] fences;
    public GameObject post;
    public GameObject gateClosed;
    public GameObject gateOpen;

    [Header("Resources")]
    [SerializeField] private Sprite[] materialSprites;
    [SerializeField] private Sprite[] cropSprites;
    [SerializeField] private Sprite[] animalSprites;
    [SerializeField] private Sprite[] meatSprites;

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

    public Sprite GetResourceSprite(ResourceType resourceType, int resourceIndex)
    {
        switch (resourceType)
        {
            case ResourceType.Material: return materialSprites[resourceIndex];
            case ResourceType.Crop: return cropSprites[resourceIndex];
            case ResourceType.Meat: return meatSprites[resourceIndex];
            case ResourceType.Animal: return animalSprites[resourceIndex];
        }
        return null;
    }

    public GameObject GetItemPrefab(Terrain.TerrainType terrainType, int itemIndex)
    {
        switch (terrainType)
        {
            case Terrain.TerrainType.Grass: return fieldItems[itemIndex];
            case Terrain.TerrainType.Grass2: return hillItems[itemIndex];
        }
        return null;
    }

    public GameObject GetRandomItemPrefab(Terrain.TerrainType terrainType, out int itemIndex)
    {
        switch (terrainType)
        {
            case Terrain.TerrainType.Grass: itemIndex = Random.Range(0, fieldItems.Length); return fieldItems[itemIndex];
            case Terrain.TerrainType.Grass2: itemIndex = Random.Range(0, hillItems.Length); return hillItems[itemIndex];
        }
        itemIndex = -1;
        return null;
    }
    
    public GameObject GetCropPrefab(CropType cropType, CropState cropState)
    {
        GameObject prefab = null;
        switch (cropType)
        {
            case CropType.Onion: prefab = onion[(int)cropState]; break;
            case CropType.Carrot: prefab = carrot[(int)cropState]; break;
            case CropType.Eggplant: prefab = eggplant[(int)cropState]; break;
            case CropType.Cucumber: prefab = cucumber[(int)cropState]; break;
            case CropType.Cabbage: prefab = cabbage[(int)cropState]; break;

            case CropType.Potato: prefab = potato[(int)cropState]; break;
            case CropType.Tomato: prefab = tomato[(int)cropState]; break;
            case CropType.Zucchini: prefab = zucchini[(int)cropState]; break;
            case CropType.Pepper: prefab = pepper[(int)cropState]; break;
            case CropType.Corn: prefab = corn[(int)cropState]; break;
        }
        if (prefab == null) prefab = grass[(int)cropState];
        return prefab;
    }

    public GameObject GetAnimalPrefab(AnimalType animalType)
    {
        return animals[(int)animalType];
    }

    public GameObject GetNPCPrefab(PeasantScript.PeasantType peasantType, PeasantScript.PeasantGender peasantGender)
    {
        switch (peasantType)
        {
            case PeasantScript.PeasantType.Adult: return peasantGender == PeasantScript.PeasantGender.Male ? malePeasantPrefab : femalePeasantPrefab;
            case PeasantScript.PeasantType.Child: return childPeasantPrefab;
        }
        return null;
    }

    public Color GetRandomSkinColor()
    {
        return skinColorsCaucassian[Random.Range(0, skinColorsCaucassian.Length)];
    }

    public Color GetRandomHairColor()
    {
        return hairColors[Random.Range(0, hairColors.Length)];
    }

    private static Color normalizeColor(Color color)
    {
        return new Color(color.r / 256, color.g / 256, color.b / 256, color.a);
    }

    public GameObject GetBuilding(BuildingScript.BuildingType buildingType)
    {
        return buildings[(int)buildingType];
    }

    /*private Color[] skinColorsCaucassian =
        new Color[] {
            new Color(255, 205, 148),
            new Color(255, 224, 189),
            new Color(234, 192, 134),
            new Color(255, 227, 159),
            new Color(255, 173, 96)
        };
    
     private Color[] hairColors =
        new Color[] {
            new Color(35, 18, 11),
            new Color(61, 35, 20),
            new Color(90, 56, 37),
            new Color(204, 153, 102),
            new Color(44, 22, 8)
        };*/
}
