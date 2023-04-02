using UnityEngine;
using static ResourceScript;

public class EnemyShipScript : MonoBehaviour
{

    public Vector3 position;
    public int orientation;
    public InventoryScript inventoryScript;

    private void Start()
    {
        if (inventoryScript == null) RandomizeInventory();
    }

    void Update()
    {
        position = transform.position;
        orientation = (int)transform.rotation.eulerAngles.y % 360;
    }

    public void RandomizeInventory()
    {
        inventoryScript = new InventoryScript();
        inventoryScript.AddCapacityToAllCategories();

        while (!inventoryScript.IsFull())
        {
            ResourceType resourceType = (ResourceType)Random.Range(0, 3);
            int resourceIndex = Random.Range(0, GetEnumLength(resourceType));
            inventoryScript.AddResource(resourceType, resourceIndex);
        }
    }
}
