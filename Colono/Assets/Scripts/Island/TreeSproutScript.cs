using UnityEngine;

public class TreeSproutScript : ItemScript
{
    public float growth;
    public float growthSpeed = 0.1f;

    void Update()
    {
        growth += Time.deltaTime * growthSpeed;

        if (!isBeingTakenCareOf)
        {
            if (growth > 1)
            {
                int index;
                ItemScript itemScript = Instantiate(ResourceScript.Instance.GetRandomTreePrefab(terrainType, out index), transform.position, transform.rotation, transform.parent).GetComponent<ItemScript>();
                itemScript.islandScript = islandScript;
                itemScript.terrainType = terrainType;
                itemScript.itemIndex = index;
                itemScript.cell = cell;
                itemScript.orientation = orientation;
                islandScript.RemoveItemAtCell(cell);
                islandScript.AddItem(itemScript);
                Destroy(gameObject);
            }
        }
    }
}
