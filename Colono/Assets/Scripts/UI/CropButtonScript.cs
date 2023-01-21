using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CropButtonScript : MonoBehaviour
{
    public GardenEditor gardenEditor;
    public Image cropImage;
    public Vector2 cell;

    public void SelectCrop()
    {
        gardenEditor.ChangeCrop(this);
    }
}
