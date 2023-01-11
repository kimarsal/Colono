using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceScript
{
    public static int GetEnumLength(ResourceType resourceType)
    {
        switch (resourceType)
        {
            case ResourceType.Material: return System.Enum.GetValues(typeof(Material)).Length;
            case ResourceType.Crop: return System.Enum.GetValues(typeof(CropType)).Length;
            case ResourceType.Meat: return System.Enum.GetValues(typeof(MeatType)).Length;
            case ResourceType.Animal: return System.Enum.GetValues(typeof(AnimalType)).Length;
        }
        return 0;
    }

    public enum ResourceType { Material, Crop, Meat, Animal }
    public enum MaterialType { Wood, Stone, Medicine };
    public enum CropType { Onion, Carrot, Eggplant, Cucumber, Cabbage, Potato, Tomato, Zucchini, Pepper, Corn }
    public enum MeatType { Cow, Pork, Mutton, Chicken, Fish }

    public enum AnimalType { Calf, Cow, Piglet, Pig, Lamb, Sheep, Chick, Chicken }
}
