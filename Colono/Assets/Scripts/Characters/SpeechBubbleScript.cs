using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeechBubbleScript : MonoBehaviour
{
    public GameObject wood;
    public GameObject stone;
    public GameObject medicine;
    public GameObject corn;
    public GameObject cucumber;
    public GameObject grape;
    public GameObject pepper;
    public GameObject potato;
    public GameObject tomato;

    private GameObject activeSprite;

    void Update()
    {
        transform.rotation = Quaternion.Euler(0, -transform.parent.rotation.y, 0);
    }

    public void DisplayResource(ItemScript.ResourceType resourceType)
    {
        if (activeSprite != null) activeSprite.SetActive(false);
        switch (resourceType)
        {
            case ItemScript.ResourceType.Wood: activeSprite = wood; break;
            case ItemScript.ResourceType.Stone: activeSprite = stone; break;
            case ItemScript.ResourceType.Medicine: activeSprite = medicine; break;
        }
        activeSprite.SetActive(true);
    }

    public void DisplayCrop(PatchScript.CropType cropType)
    {
        if (activeSprite != null) activeSprite.SetActive(false);
        switch (cropType)
        {
            case PatchScript.CropType.Corn: activeSprite = corn; break;
            case PatchScript.CropType.Cucumber: activeSprite = cucumber; break;
            case PatchScript.CropType.Grape: activeSprite = grape; break;
            case PatchScript.CropType.Pepper: activeSprite = pepper; break;
            case PatchScript.CropType.Potato: activeSprite = potato; break;
            case PatchScript.CropType.Tomato: activeSprite = tomato; break;
        }
        activeSprite.SetActive(true);
    }

    public void HideSpeechBubble()
    {
        activeSprite.SetActive(false);
        gameObject.SetActive(false);
    }
}
