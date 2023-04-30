using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingScript : MonoBehaviour
{
    private float timeSinceLastCatch;
    public float catchSpeed = 0.1f;

    private void Update()
    {
        timeSinceLastCatch += Time.deltaTime * catchSpeed;
        if (timeSinceLastCatch > 1)
        {
            timeSinceLastCatch = 0;
            int fishAmount = Random.Range(0, ShipScript.Instance.level);

            if(fishAmount > 0)
            {
                ShipScript.Instance.shipInterior.AddResource(ResourceScript.ResourceType.Meat, (int)ResourceScript.MeatType.Fish, fishAmount);
            }
        }
    }
}
