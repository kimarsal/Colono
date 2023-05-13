
using UnityEngine;
using UnityEngine.UI;

public class TradeEditor : EditorScript
{
    [SerializeField] private OfferButtonScript offerButtonPrefab;
    [SerializeField] private Transform buysTransform;
    [SerializeField] private Transform sellsTransform;
    private InventoryScript shipInventory;
    private InventoryScript enemyInventory;

    public override void SetEditor(ConstructionScript constructionScript)
    {
        if(shipInventory == null)
        {
            shipInventory = ShipScript.Instance.shipInterior.inventoryScript;
            enemyInventory = EnemyController.Instance.GetComponent<EnemyShipScript>().inventoryScript;
        }

        for(int i = 0; i < 3; i++)
        {
            for (int j = 0; j < ResourceScript.GetEnumLength((ResourceScript.ResourceType)i); j++)
            {
                if ((ResourceScript.ResourceType)i == ResourceScript.ResourceType.Material && (ResourceScript.MaterialType)j == ResourceScript.MaterialType.Gem) continue;

                int amountInShip = shipInventory.GetResourceAmount((ResourceScript.ResourceType)i, j);
                int amountInEnemyShip = enemyInventory.GetResourceAmount((ResourceScript.ResourceType)i, j);

                if (amountInShip > 0)
                {
                    int offeredAmount = Random.Range(1, amountInShip + 1);
                    Instantiate(offerButtonPrefab, sellsTransform).SetOffer(false, (ResourceScript.ResourceType)i, j, offeredAmount, offeredAmount);
                }
                if (amountInEnemyShip > 0)
                {
                    int offeredAmount = Random.Range(1, amountInEnemyShip + 1);
                    Instantiate(offerButtonPrefab, buysTransform).SetOffer(true, (ResourceScript.ResourceType)i, j, offeredAmount * 2, offeredAmount);
                }
            }
        }

        GetComponent<PopUpScript>().ShowPopUp();
    }

    internal void AcceptOffer(OfferButtonScript offerButton)
    {
        if(shipInventory.GetResourceAmount(offerButton.givenResourceType, offerButton.givenResourceIndex) >= offerButton.givenResourceAmount)
        {
            shipInventory.RemoveResource(offerButton.givenResourceType, offerButton.givenResourceIndex, offerButton.givenResourceAmount);
            enemyInventory.RemoveResource(offerButton.receivedResourceType, offerButton.receivedResourceIndex, offerButton.receivedResourceAmount);

            shipInventory.AddResource(offerButton.receivedResourceType, offerButton.receivedResourceIndex, offerButton.receivedResourceAmount);
            enemyInventory.AddResource(offerButton.givenResourceType, offerButton.givenResourceIndex, offerButton.givenResourceAmount);

            CanvasScript.Instance.InventoryChange(offerButton.givenResourceType, offerButton.givenResourceIndex, -offerButton.givenResourceAmount);
            CanvasScript.Instance.InventoryChange(offerButton.receivedResourceType, offerButton.receivedResourceIndex, offerButton.receivedResourceAmount);
        }
        offerButton.GetComponent<Button>().interactable = false;
    }
}
