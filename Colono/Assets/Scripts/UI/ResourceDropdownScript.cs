using UnityEngine;
using static ResourceScript;
using UnityEngine.UI;

public class ResourceDropdownScript : MonoBehaviour
{
    public ResourceType resourceType;
    public bool displayIntroducedCrops;
    public bool displayNativeCrops;
    private Dropdown dropdown;

    void Start()
    {
        dropdown = GetComponent<Dropdown>();
        dropdown.options.Add(new Dropdown.OptionData());

        int length = GetEnumLength(resourceType);
        for (int i = 0; i < length; i++)
        {
            if(resourceType == ResourceType.Crop)
            {
                if (i < length / 2 && !displayIntroducedCrops || i >= length / 2 && !displayNativeCrops) continue;
            }

            dropdown.options.Add(new Dropdown.OptionData(IslandEditor.Instance.GetResourceSprite(resourceType, i)));
        }
        dropdown.value = 0;

    }
}
