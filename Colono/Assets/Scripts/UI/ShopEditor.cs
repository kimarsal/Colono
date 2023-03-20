using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopEditor : EditorScript
{
    [SerializeField] private OfferButtonScript offerButtonPrefab;
    [SerializeField] private Transform buysTransform;
    [SerializeField] private Transform sellsTransform;

    private int materialTypes;
    private int cropTypes;
    private int meatTypes;

    public override void SetEditor(ConstructionScript constructionScript)
    {
        InventoryScript inventoryScript = ((ShipScript)constructionScript).inventoryScript;

        materialTypes = Enum.GetValues(typeof(ResourceScript.MaterialType)).Length;
        cropTypes = Enum.GetValues(typeof(ResourceScript.CropType)).Length;
        meatTypes = Enum.GetValues(typeof(ResourceScript.MeatType)).Length;

        for(int i = 0; i < cropTypes; i++)
        {
            Instantiate(offerButtonPrefab, buysTransform).SetOffer(true, ResourceScript.ResourceType.Crop, i, 2);
            Instantiate(offerButtonPrefab, sellsTransform).SetOffer(false, ResourceScript.ResourceType.Crop, i, 1);
        }
    }
}
