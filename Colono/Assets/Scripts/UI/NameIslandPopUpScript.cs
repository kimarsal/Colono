using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NameIslandPopUpScript : PopUpScript
{
    [SerializeField] private TMP_InputField islandNameText;
    [SerializeField] private Button confirmButton;

    public void TextChange()
    {
        confirmButton.interactable = islandNameText.text != "";
    }

    public void Confirm()
    {
        CanvasScript.Instance.dockButton.GetComponentInChildren<TextMeshProUGUI>().text = "Dock onto " + islandNameText.text;
        GameManager.Instance.closestIsland.islandName = islandNameText.text;
        GameManager.Instance.closestIsland.islandNameText.text = islandNameText.text;
        islandNameText.text = "";
        GameManager.Instance.Dock();
        HidePopUp();
    }
}
