using UnityEngine;
using static ResourceScript;

[System.Serializable]
public class EnemyShipScript : MonoBehaviour
{
    public Vector3 position;
    public int orientation;
    public InventoryScript inventoryScript;

    private void Start()
    {
        RandomizeInventory();
    }

    public void Initialize(EnemyShipScript enemyShipInfo = null)
    {
        if (enemyShipInfo == null)
        {
            RandomizeInventory();
        }
        else
        {
            transform.position = enemyShipInfo.position;
            transform.rotation = Quaternion.Euler(0, enemyShipInfo.orientation, 0);
            inventoryScript = enemyShipInfo.inventoryScript;
        }
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

        for (int i = 0; i < 30 && !inventoryScript.IsFull(); i++)
        {
            ResourceType resourceType = (ResourceType)Random.Range(0, 3);
            int resourceIndex = Random.Range(0, GetEnumLength(resourceType));
            inventoryScript.AddResource(resourceType, resourceIndex);
        }
    }
}
