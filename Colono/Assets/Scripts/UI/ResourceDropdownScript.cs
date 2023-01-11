using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ResourceScript;
using UnityEngine.UI;

public class ResourceDropdownScript : MonoBehaviour
{
    public ResourceType resourceType;

    void Start()
    {
        IslandEditor islandEditor = GameObject.FindGameObjectWithTag("GameController").GetComponent<IslandEditor>();
        Dropdown dropdown = GetComponent<Dropdown>();

        int length = ResourceScript.GetEnumLength(resourceType);
        for (int i = 0; i < length; i++)
        {
            dropdown.options.Add(new Dropdown.OptionData(islandEditor.GetResourceSprite(resourceType, i)));
        }
        dropdown.template.GetComponent<RectTransform>().sizeDelta = new Vector2(dropdown.template.GetComponent<RectTransform>().sizeDelta.x, length * 100 / 2);
        dropdown.value = 0;

    }
}
