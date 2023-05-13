using UnityEngine;
using static ResourceScript;
using UnityEngine.UI;

public class ResourceDropdownScript : MonoBehaviour
{
    public ResourceType resourceType;
    public bool includeEmptyOption;
    public bool displayIntroducedCrops;
    public bool displayNativeCrops;
    private Dropdown dropdown;
    [SerializeField] private Transform contentTransform;

    void Awake()
    {
        dropdown = GetComponent<Dropdown>();
        if(includeEmptyOption) dropdown.options.Add(new Dropdown.OptionData());

        int length = GetEnumLength(resourceType);
        for (int i = 0; i < length; i++)
        {
            if(resourceType == ResourceType.Crop)
            {
                if (i < length / 2 && !displayIntroducedCrops || i >= length / 2 && !displayNativeCrops) continue;
            }

            dropdown.options.Add(new Dropdown.OptionData(ResourceScript.Instance.GetResourceSprite(resourceType, i)));
        }
        //dropdown.value = 0;
    }

    private void Update()
    {
        if (resourceType != ResourceType.Crop) return;

        int i = -1;
        foreach(Transform item in transform.GetChild(transform.childCount - 1).GetChild(0).GetChild(0))
        {
            if (i != -1) item.gameObject.SetActive(GameManager.Instance.discoveredCrops[i]);
            i++;
        }
    }
}
