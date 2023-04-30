using UnityEngine;
using UnityEngine.UI;

public class NewCropPopUpScript : PopUpScript
{
    [SerializeField] private Image cropImage;
    [SerializeField] private RectTransform cropBackground;

    public void ShowNewCrop(int cropType)
    {
        cropImage.sprite = ResourceScript.Instance.GetResourceSprite(ResourceScript.ResourceType.Crop, cropType);
        ShowPopUp();
    }

    private void Update()
    {
        cropBackground.Rotate(new Vector3(0, 0, 3) * Time.deltaTime);
    }
}
