using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeButtonScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI priceText;

    public void UpdateButton(int constructionLevel, int availableGems)
    {
        GetComponent<Button>().interactable = constructionLevel < 10 && availableGems >= constructionLevel;
        priceText.text = constructionLevel.ToString();
    }
}
