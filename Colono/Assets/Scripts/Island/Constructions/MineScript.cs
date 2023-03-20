using Newtonsoft.Json;
using UnityEngine;

public class MineScript : BuildingScript
{
    private float timeSinceLastExtraction;
    public float extractionSpeed = 0.2f;

    public override bool canManagePeasants { get { return true; } }

    public override EditorScript editorScript { get { return null; } }

    private void Update()
    {
        timeSinceLastExtraction += Time.deltaTime * extractionSpeed;
        if(timeSinceLastExtraction > 1)
        {
            if(Random.Range(0,1) < 0.1)
            {
                islandScript.AddResource(ResourceScript.ResourceType.Material, (int)ResourceScript.MaterialType.Gem);
            }
            else
            {
                int extractedAmount = (peasantList.Count - peasantsOnTheirWay);
                islandScript.AddResource(ResourceScript.ResourceType.Material, (int)ResourceScript.MaterialType.Stone, extractedAmount);
            }
            timeSinceLastExtraction = 0;
        }
    }
}