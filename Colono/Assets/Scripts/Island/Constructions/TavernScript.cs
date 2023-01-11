using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TavernScript : BuildingScript
{
    public List<TavernEditor.Recipe> recipes = new List<TavernEditor.Recipe>();

    private void Start()
    {
        recipes.Add(new TavernEditor.Recipe());
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
        foreach(TavernEditor.Recipe recipe in recipes)
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
        if(peasantScript.peasantType == PeasantScript.PeasantType.Adult && peasantScript.constructionScript != null && peasantScript.exhaustion < 1)
        {
            PeasantAdultScript peasantAdultScript = (PeasantAdultScript)peasantScript;
            if (peasantAdultScript.task == null)
            {
                peasantAdultScript.task = peasantAdultScript.constructionScript.GetNextPendingTask();
                peasantAdultScript.task.peasantScript = peasantAdultScript;
            }
        }
        peasantScript.UpdateTask();
    }
}
