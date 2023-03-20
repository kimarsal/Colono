using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TavernScript : BuildingScript
{
    public List<Recipe> recipeList = new List<Recipe>();

    public override EditorScript editorScript { get { return CanvasScript.Instance.tavernEditor; } }

    private void Start()
    {
        recipeList.Add(new Recipe());
    }

    public override PeasantScript PeasantHasArrived(PeasantScript peasantScript)
    {
        PeasantScript newPeasantScript = base.PeasantHasArrived(peasantScript);
        StartCoroutine(FeedPeasantCoroutine(newPeasantScript));
        return null;
    }

    private IEnumerator FeedPeasantCoroutine(PeasantScript peasantScript)
    {
        yield return new WaitForSeconds(5);

        PeasantScript newPeasantScript = Instantiate(peasantScript.gameObject,
                entry.position, Quaternion.identity,
                islandScript.npcsTransform).GetComponent<PeasantScript>();
        newPeasantScript.InitializePeasant(peasantScript);
        Destroy(peasantScript.gameObject);

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

        newPeasantScript.hunger =- hungerPoints;
        newPeasantScript.tavern = null;
        if (newPeasantScript.peasantType == PeasantScript.PeasantType.Adult)
        {
            PeasantAdultScript peasantAdultScript = (PeasantAdultScript)newPeasantScript;
            peasantAdultScript.taskSourceInterface.GetNextPendingTask(peasantAdultScript);
        }
        else newPeasantScript.UpdateTask();
    }

    public override PeasantScript RemovePeasant()
    {
        PeasantScript peasantScript = base.RemovePeasant();
        peasantScript.tavern = null;
        return peasantScript;
    }

}