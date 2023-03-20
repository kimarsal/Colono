using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OfferButtonScript : MonoBehaviour
{
    private ResourceScript.ResourceType givenResourceType;
    private int givenResourceIndex;
    private int givenResourceAmount;
    private ResourceScript.ResourceType receivedResourceType;
    private int receivedResourceIndex;
    private int receivedResourceAmount;

    [SerializeField] private Image givenResourceImage;
    [SerializeField] private TextMeshProUGUI givenResourceAmountText;
    [SerializeField] private Image receivedResourceImage;
    [SerializeField] private TextMeshProUGUI receivedResourceAmountText;

    public void SetOffer(bool isBuy, ResourceScript.ResourceType resourceType, int resourceIndex, int price, int amount = 1)
    {
        if (isBuy)
        {
            givenResourceType = ResourceScript.ResourceType.Material;
            givenResourceIndex = (int)ResourceScript.MaterialType.Gem;
            givenResourceAmount = price;

            receivedResourceType = resourceType;
            receivedResourceIndex = resourceIndex;
            receivedResourceAmount = amount;
        }
        else
        {
            givenResourceType = resourceType;
            givenResourceIndex = resourceIndex;
            givenResourceAmount = amount;

            receivedResourceType = ResourceScript.ResourceType.Material;
            receivedResourceIndex = (int)ResourceScript.MaterialType.Gem;
            receivedResourceAmount = price;
        }

        givenResourceImage.sprite = IslandEditor.Instance.GetResourceSprite(givenResourceType, givenResourceIndex);
        givenResourceAmountText.text = givenResourceAmount.ToString();
        receivedResourceImage.sprite = IslandEditor.Instance.GetResourceSprite(receivedResourceType, receivedResourceIndex);
        receivedResourceAmountText.text = receivedResourceAmount.ToString();
    }
}
