using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CropButtonScript : MonoBehaviour
{
    public GardenEditor gardenEditor;
    public Image cropImage;
    public ResourceScript.CropType cropType;
    public Vector2 cell;
    public bool isOnGrid;

    public void SelectCrop()
    {
        if (isOnGrid)
        {
            gardenEditor.ChangeCrop(this);
        }
        else
        {
            gardenEditor.SelectCrop(cropType);
        }
    }
}
