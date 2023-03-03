
using Newtonsoft.Json;
using UnityEngine;

public class MineScript : BuildingScript
{
    private float timeSinceLastExtraction;
    public float extractionSpeed = 0.2f;

    [JsonIgnore] public override bool canManagePeasants { get { return true; } }

    private void Update()
    {
        timeSinceLastExtraction += Time.deltaTime * extractionSpeed;
        if(timeSinceLastExtraction > 1)
        {
            int extractedAmount = (peasantList.Count - peasantsOnTheirWay);
            islandScript.AddResource(ResourceScript.ResourceType.Material, (int)ResourceScript.MaterialType.Stone, extractedAmount);
            timeSinceLastExtraction = 0;
        }
    }

    public override void EditConstruction()
    {
        throw new System.NotImplementedException();
    }
}