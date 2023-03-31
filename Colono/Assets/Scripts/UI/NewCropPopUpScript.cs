using UnityEngine;
using UnityEngine.UI;

public class NewCropPopUpScript : PopUpScript
{
    [SerializeField] private Image cropImage;

    public void ShowNewCrop(int cropType)
    {
        cropImage.sprite = IslandEditor.Instance.GetResourceSprite(ResourceScript.ResourceType.Crop, cropType);
        ShowPopUp();
    }
}
