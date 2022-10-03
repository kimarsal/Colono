using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemScript : TaskScript
{
    public enum ItemType { Chop, Dig, Pull, Pick }
    public ItemType itemType;
    public int itemStrength = 1;

    public enum ResourceType { Wood, Stone, Medicine };
    public ResourceType resourceType;
    public int resourceAmount = 1;

    public NPCManager npcManager;
    public GameObject clearingCanvas;
    public bool isScheduledForClearing;

    private void Start()
    {
        taskType = TaskType.Item;
        center = transform.position;
    }

    public bool ChangeItemClearingState(bool toClear)
    {
        if (isScheduledForClearing != toClear)
        {
            isScheduledForClearing = toClear;
            clearingCanvas.SetActive(toClear);
            if (!toClear && peasantScript != null) peasantScript.CancelTask();
            return true;
        }
        return false;
    }

    private void Update()
    {
        clearingCanvas.transform.GetChild(0).position = Camera.main.WorldToScreenPoint(transform.position) + new Vector3(0, 30f, 0);
    }

    public void ItemProgress()
    {
        itemStrength--;
        Debug.Log(itemStrength);
        if (itemStrength == 0)
        {
            Debug.Log(resourceType.ToString() + ": " + resourceAmount);
            peasantScript.CancelTask();
            npcManager.RemoveTask(this);
            Destroy(gameObject);
        }
    }
}
