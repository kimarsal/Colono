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

    public void DisplayMaterial(ResourceScript.MaterialType materialType)
    {
        if (activeSprite != null) activeSprite.SetActive(false);
        switch (materialType)
        {
            case ResourceScript.MaterialType.Wood: activeSprite = wood; break;
            case ResourceScript.MaterialType.Stone: activeSprite = stone; break;
            //case ResourceScript.MaterialType.Medicine: activeSprite = medicine; break;
        }
        activeSprite.SetActive(true);
    }

    public void DisplayCrop(ResourceScript.CropType cropType)
    {
        if (activeSprite != null) activeSprite.SetActive(false);
        switch (cropType)
        {
            case ResourceScript.CropType.Corn: activeSprite = corn; break;
            case ResourceScript.CropType.Cucumber: activeSprite = cucumber; break;
            case ResourceScript.CropType.Pepper: activeSprite = pepper; break;
            case ResourceScript.CropType.Potato: activeSprite = potato; break;
            case ResourceScript.CropType.Tomato: activeSprite = tomato; break;
        }
        activeSprite.SetActive(true);
    }

    public void HideSpeechBubble()
    {
        activeSprite.SetActive(false);
        gameObject.SetActive(false);
    }
}
