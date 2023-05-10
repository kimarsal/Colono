using Newtonsoft.Json;
using UnityEngine;

public class MineScript : BuildingScript
{
    [JsonProperty] private float timeSinceLastExtraction;
    public float extractionSpeed = 0.1f;

    public override Sprite sprite { get { return ResourceScript.Instance.mineSprite; } }
    public override bool canManagePeasants { get { return true; } }
    public override EditorScript editorScript { get { return CanvasScript.Instance.mineEditor; } }

    private void Update()
    {
        timeSinceLastExtraction += Time.deltaTime * extractionSpeed;
        if(timeSinceLastExtraction > 1)
        {
            timeSinceLastExtraction = 0;
            if (peasantsInside == 0) return;

            islandScript.AddResource(ResourceScript.ResourceType.Material, (int)ResourceScript.MaterialType.Stone, peasantsInside);
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