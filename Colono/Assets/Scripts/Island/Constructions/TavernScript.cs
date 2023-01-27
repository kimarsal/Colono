using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TavernScript : BuildingScript
{
    public List<Recipe> recipeList = new List<Recipe>();

    private void Start()
    {
        recipeList.Add(new Recipe());
    }

    public override void EditConstruction()
    {
        islandScript.gameManager.canvasScript.ShowTavernEditor();
    }

    public void FeedPeasant(PeasantScript peasantScript)
    {
        StartCoroutine(FeedPeasantCoroutine(peasantScript));
    }

    private IEnumerator FeedPeasantCoroutine(PeasantScript peasantScript)
    {
        peasantScript.gameObject.SetActive(false);
        yield return new WaitForSeconds(5);
        peasantScript.gameObject.SetActive(true);

        int hungerPoints = 0;
        foreach(Recipe recipe in recipeList)
        {
            if ((recipe.introducedCrop == -1 || islandScript.GetResourceAmount(ResourceScript.ResourceType.Crop, recipe.introducedCrop) > 0)
                && (recipe.nativeCrop == -1 || islandScript.GetResourceAmount(ResourceScript.ResourceType.Crop, recipe.nativeCrop) > 0)
                && (recipe.meat == -1 || islandScript.GetResourceAmount(ResourceScript.ResourceType.Meat, recipe.meat) > 0))
            {
                hungerPoints = recipe.hungerPoints;
                if (recipe.introducedCrop != -1) islandScript.UseResource(ResourceScript.ResourceType.Crop, recipe.introducedCrop);
                if (recipe.nativeCrop != -1) islandScript.UseResource(ResourceScript.ResourceType.Crop, recipe.nativeCrop);
                if (recipe.meat != -1) islandScript.UseResource(ResourceScript.ResourceType.Crop, recipe.meat);
                break;
            }
        }

        peasantScript.hunger =- hungerPoints;
        peasantScript.tavern = null;
        if (peasantScript.peasantType == PeasantScript.PeasantType.Adult)
        {
            PeasantAdultScript peasantAdultScript = (PeasantAdultScript)peasantScript;
            peasantAdultScript.taskSourceScript.GetNextPendingTask(peasantAdultScript);
        }
        else peasantScript.UpdateTask();
    }

    public override PeasantScript RemovePeasant()
    {
        PeasantScript peasantScript = base.RemovePeasant();
        peasantScript.tavern = null;
        return peasantScript;
    }

    public TavernInfo GetTavernInfo()
    {
        TavernInfo tavernInfo = new TavernInfo();
        tavernInfo.recipeList = recipeList;
        return tavernInfo;
    }
}

[System.Serializable]
public class TavernInfo : BuildingInfo
{
    public List<Recipe> recipeList;
}