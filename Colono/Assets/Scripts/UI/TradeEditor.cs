
using UnityEngine;

public class TradeEditor : EditorScript
{
    [SerializeField] private OfferButtonScript offerButtonPrefab;
    [SerializeField] private Transform buysTransform;
    [SerializeField] private Transform sellsTransform;

    public override void SetEditor(ConstructionScript constructionScript)
    {
        InventoryScript inventoryScript = EnemyController.Instance.GetComponent<EnemyShipScript>().inventoryScript;

        for(int i = 0; i < 3; i++)
        {
            for (int j = 0; j < ResourceScript.GetEnumLength((ResourceScript.ResourceType)i); j++)
            {
                int amountInShip = ShipScript.Instance.shipInterior.inventoryScript.GetResourceAmount((ResourceScript.ResourceType)i, j);
                int amountInEnemyShip = inventoryScript.GetResourceAmount((ResourceScript.ResourceType)i, j);

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
}
