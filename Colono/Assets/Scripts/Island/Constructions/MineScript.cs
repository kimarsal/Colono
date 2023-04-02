using Newtonsoft.Json;
using UnityEngine;

public class MineScript : BuildingScript
{
    private float timeSinceLastExtraction;
    public float extractionSpeed = 0.1f;

    public override EditorScript editorScript { get { return CanvasScript.Instance.mineEditor; } }

    private void Update()
    {
        timeSinceLastExtraction += Time.deltaTime * extractionSpeed;
        if(timeSinceLastExtraction > 1)
        {
            timeSinceLastExtraction = 0;
            int peasantAmount = (peasantList.Count - peasantsOnTheirWay);
            if (peasantAmount == 0) return;

            islandScript.AddResource(ResourceScript.ResourceType.Material, (int)ResourceScript.MaterialType.Stone, peasantAmount);
            if (Random.Range(0f,1f) < 0.1)
            {
                islandScript.AddResource(ResourceScript.ResourceType.Material, (int)ResourceScript.MaterialType.Gem);
            }
        }
    }

    public override PeasantScript RemovePeasant()
    {
        PeasantScript peasantScript = base.RemovePeasant();
        peasantScript.constructionScript = null;
        return peasantScript;
    }
}