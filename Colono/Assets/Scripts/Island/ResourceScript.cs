using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceScript : MonoBehaviour
{
    public static ResourceScript Instance { get; private set; }

    public enum ResourceType { Material, Crop, Meat, Animal }
    public enum MaterialType { Wood, Stone, Flower, Mushroom, Sprout, Gem };
    public enum CropType { Onion, Carrot, Eggplant, Cucumber, Cabbage, Potato, Tomato, Zucchini, Pepper, Corn }
    public enum MeatType { Cow, Pork, Mutton, Chicken, Fish }

    public enum AnimalType { Calf, Cow, Piglet, Pig, Lamb, Sheep, Chick, Chicken }

    public static int GetEnumLength(ResourceType resourceType)
    {
        switch (resourceType)
        {
            case ResourceType.Material: return System.Enum.GetValues(typeof(MaterialType)).Length;
            case ResourceType.Crop: return System.Enum.GetValues(typeof(CropType)).Length;
            case ResourceType.Meat: return System.Enum.GetValues(typeof(MeatType)).Length;
            case ResourceType.Animal: return System.Enum.GetValues(typeof(AnimalType)).Length;
        }
        return 0;
    }

    [SerializeField] private Transform itemsTransform;

    [Header("NPCs")]
    [SerializeField] private PeasantScript malePeasantPrefab;
    [SerializeField] private PeasantScript femalePeasantPrefab;
    [SerializeField] private PeasantScript childPeasantPrefab;
    [SerializeField] private Color[] skinColorsCaucassian;
    [SerializeField] private Color[] hairColors;

    [SerializeField] private GameObject[] malePeasantHeadPrefabs;
    [SerializeField] private GameObject[] femalePeasantHeadPrefabs;
    [SerializeField] private GameObject[] childPeasantHeadPrefabs;

    [Header("Cell materials")]
    public Material hoverMaterial;
    public Material selectingMaterial;
    public Material selectingFirstCellMaterial;
    public Material selectedMaterial;
    public Material invalidSelectionMaterial;
    public Material selectedHoverMaterial;
    public Material patchMaterial;

    [Header("Constructions")]
    public Sprite shipSprite;
    public Sprite warehouseSprite;
    public Sprite tavernSprite;
    public Sprite cabinSprite;
    public Sprite gardenSprite;
    public Sprite penSprite;
    public Sprite mineSprite;

    [Header("Buildings")]
    [SerializeField] private BuildingScript[] buildings;

    [Header("Fences")]
    [SerializeField] private GameObject[] fences;
    public GameObject post;
    public GameObject gate;

    [Header("Resources")]
    [SerializeField] private Sprite[] materialSprites;
    [SerializeField] private Sprite[] cropSprites;
    [SerializeField] private Sprite[] animalSprites;
    [SerializeField] private Sprite[] meatSprites;

    [Header("Crops")]
    [SerializeField] private GameObject[] onion;
    [SerializeField] private GameObject[] carrot;
    [SerializeField] private GameObject[] eggplant;
    [SerializeField] private GameObject[] cucumber;
    [SerializeField] private GameObject[] cabbage;
    [SerializeField] private GameObject[] potato;
    [SerializeField] private GameObject[] tomato;
    [SerializeField] private GameObject[] zucchini;
    [SerializeField] private GameObject[] pepper;
    [SerializeField] private GameObject[] corn;
    [SerializeField] private GameObject cropSprout;

    [Header("Animals")]
    [SerializeField] private AnimalScript[] animals;

    [Header("Recipes")]
    public Sprite[] cookedVegetableSprites;
    public Sprite[] cookedMeatSprites;

    [Header("Items")]
    //public ItemScript[] beachItems;
    [SerializeField] private ItemScript[] fieldItems;
    [SerializeField] private ItemScript[] hillItems;
    //public ItemScript[] mountainItems;
    public TreeSproutScript treeSprout;

    [Header("Others")]
    public GameObject islandMapIcon;
    public GameObject coastObstacle;
    public GameObject enclosureCenter;
    public BoxScript box;
    public InventoryChangeScript inventoryChange;

    [Header("Pooling")]
    [SerializeField] private Vector3 fieldItemsStartPos;
    [SerializeField] private Vector3 hillItemsStartPos;
    [SerializeField] private int itemSpacing;
    private List<ItemScript>[] instantiatedFieldItems;
    private List<ItemScript>[] instantiatedHillItems;
    private int currentFieldIndex;
    private int currentHillIndex;
    private int lastFieldIndex;
    private int lastHillIndex;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        instantiatedFieldItems = new List<ItemScript>[fieldItems.Length];
        instantiatedHillItems = new List<ItemScript>[hillItems.Length];
        for(int i = 0; i < fieldItems.Length; i++)
        {
            instantiatedFieldItems[i] = new List<ItemScript>();
        }
        for (int i = 0; i < hillItems.Length; i++)
        {
            instantiatedHillItems[i] = new List<ItemScript>();
        }
        lastFieldIndex = -1;
        lastHillIndex = -1;

        StartCoroutine(SpawnCoroutine());
    }

    private IEnumerator SpawnCoroutine()
    {
        while (true)
        {
            while(currentFieldIndex != lastFieldIndex && instantiatedFieldItems[currentFieldIndex].Count > 10)
            {
                currentFieldIndex = (currentFieldIndex + 1) % (fieldItems.Length - 2);
            }
            if(currentFieldIndex != lastFieldIndex)
            {
                ItemScript fieldItem = Instantiate(fieldItems[currentFieldIndex],
                    fieldItemsStartPos + new Vector3(instantiatedFieldItems[currentFieldIndex].Count * itemSpacing, 0, currentFieldIndex * itemSpacing),
                    Quaternion.identity, itemsTransform);
                instantiatedFieldItems[currentFieldIndex].Add(fieldItem);
                lastFieldIndex = currentFieldIndex;
            }
            currentFieldIndex = (currentFieldIndex + 1) % (fieldItems.Length - 2);

            yield return new WaitForSeconds(1f);

            while (currentHillIndex != lastHillIndex && instantiatedHillItems[currentHillIndex].Count > 10)
            {
                currentHillIndex = (currentHillIndex + 1) % (hillItems.Length - 2);
            }
            if(currentHillIndex != lastHillIndex)
            {
                ItemScript hillItem = Instantiate(hillItems[currentHillIndex],
                    hillItemsStartPos + new Vector3(instantiatedHillItems[currentHillIndex].Count * itemSpacing, 0, currentHillIndex * itemSpacing),
                    Quaternion.identity, itemsTransform);
                instantiatedHillItems[currentHillIndex].Add(hillItem);
                lastHillIndex = currentHillIndex;
            }
            currentHillIndex = (currentHillIndex + 1) % (hillItems.Length - 2);

            yield return new WaitForSeconds(1f);
        }
    }

    private ItemScript GetInstantiatedItemPrefab(Terrain.TerrainType terrainType, int itemIndex)
    {
        ItemScript itemScript = null;
        switch (terrainType)
        {
            case Terrain.TerrainType.Field:
                if (instantiatedFieldItems == null || instantiatedFieldItems[itemIndex].Count == 0)
                {
                    itemScript = Instantiate(fieldItems[itemIndex], itemsTransform);
                }
                else
                {
                    itemScript = instantiatedFieldItems[itemIndex][instantiatedFieldItems[itemIndex].Count - 1];
                    instantiatedFieldItems[itemIndex].RemoveAt(instantiatedFieldItems[itemIndex].Count - 1);
                }
                break;
            case Terrain.TerrainType.Hill:
                if (instantiatedHillItems == null || instantiatedHillItems[itemIndex].Count == 0)
                {
                    itemScript = Instantiate(hillItems[itemIndex], itemsTransform);
                }
                else
                {
                    itemScript = instantiatedHillItems[itemIndex][instantiatedHillItems[itemIndex].Count - 1];
                    instantiatedHillItems[itemIndex].RemoveAt(instantiatedHillItems[itemIndex].Count - 1);
                }
                break;
        }
        return itemScript;
    }

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

    public ItemScript GetItemPrefab(Terrain.TerrainType terrainType, int itemIndex)
    {
        return GetInstantiatedItemPrefab(terrainType, itemIndex);
    }

    public ItemScript GetRandomItemPrefab(Terrain.TerrainType terrainType, out int itemIndex)
    {
        switch (terrainType)
        {
            case Terrain.TerrainType.Field: itemIndex = Random.Range(0, fieldItems.Length); break;
            case Terrain.TerrainType.Hill: itemIndex = Random.Range(0, hillItems.Length); break;
            default: itemIndex = -1; break;
        }
        return GetInstantiatedItemPrefab(terrainType, itemIndex);
    }

    public ItemScript GetRandomTreePrefab(Terrain.TerrainType terrainType, out int itemIndex)
    {
        switch (terrainType)
        {
            case Terrain.TerrainType.Field: itemIndex = Random.Range(0, 14); return fieldItems[itemIndex];
            case Terrain.TerrainType.Hill: itemIndex = Random.Range(0, 4); return hillItems[itemIndex];
        }
        itemIndex = -1;
        return null;
    }

    public GameObject GetRandomFencePrefab()
    {
        return fences[UnityEngine.Random.Range(0, fences.Length)];
    }

    public GameObject GetCropPrefab(CropType cropType, PatchScript.CropState cropState)
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
        if (prefab == null) prefab = cropSprout;
        return prefab;
    }

    public AnimalScript GetAnimalPrefab(AnimalType animalType)
    {
        return animals[(int)animalType];
    }

    public PeasantScript GetPeasantPrefab(PeasantScript.PeasantType peasantType, PeasantScript.PeasantGender peasantGender)
    {
        PeasantScript prefab = null;
        switch (peasantType)
        {
            case PeasantScript.PeasantType.Adult: prefab = peasantGender == PeasantScript.PeasantGender.Male ? malePeasantPrefab : femalePeasantPrefab; break;
            case PeasantScript.PeasantType.Child: prefab = childPeasantPrefab; prefab.peasantGender = peasantGender; break;
        }
        return prefab;
    }

    public GameObject GetPeasantHeadPrefab(PeasantScript peasantScript)
    {
        GameObject prefab = null;
        if(peasantScript.peasantType == PeasantScript.PeasantType.Adult)
        {
            if(peasantScript.peasantGender == PeasantScript.PeasantGender.Male)
            {
                prefab = malePeasantHeadPrefabs[peasantScript.headType];
            }
            else
            {
                prefab = femalePeasantHeadPrefabs[peasantScript.headType];
            }
        }
        else
        {
            prefab = childPeasantHeadPrefabs[peasantScript.headType];
        }

        return prefab;
    }

    public Color GetRandomSkinColor()
    {
        return skinColorsCaucassian[Random.Range(0, skinColorsCaucassian.Length)];
    }

    public Color GetRandomHairColor()
    {
        return hairColors[Random.Range(0, hairColors.Length)];
    }

    public BuildingScript GetBuilding(BuildingScript.BuildingType buildingType)
    {
        return Instantiate(buildings[(int)buildingType]);
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
        };

    private static Color normalizeColor(Color color)
    {
        return new Color(color.r / 256, color.g / 256, color.b / 256, color.a);
    }*/
}
