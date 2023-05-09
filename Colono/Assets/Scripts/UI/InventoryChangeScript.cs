using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryChangeScript : MonoBehaviour
{
    [SerializeField] private Image resourceImage;
    [SerializeField] private TextMeshProUGUI amountText;

    public void SetInventoryChange(Sprite sprite, int amount)
    {
        resourceImage.sprite = sprite;
        amountText.text = (amount > 0 ? "+" : "") + amount;
    }

    public void HideInventoryChange()
    {
        Destroy(gameObject);
    }
}
