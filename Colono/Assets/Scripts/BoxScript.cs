using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxScript : MonoBehaviour
{
    public InventoryScript inventoryScript;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<ShipScript>().CollectBooty(inventoryScript);
            Destroy(gameObject);
        }
    }

}
