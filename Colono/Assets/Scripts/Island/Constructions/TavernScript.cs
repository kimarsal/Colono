using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TavernScript : BuildingScript
{
    [JsonProperty] public List<Recipe> recipeList = new List<Recipe>();

    public override EditorScript editorScript { get { return CanvasScript.Instance.tavernEditor; } }

    public override void InitializeBuilding(BuildingScript buildingInfo)
    {
        base.InitializeBuilding(buildingInfo);

        recipeList = ((TavernScript)buildingInfo).recipeList;
    }

    public override void AddPeasant(PeasantScript peasantScript)
    {
        peasantList.Add(peasantScript);
        UpdateConstructionDetails();
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

        if (peasantScript != null)
        {
            float hungerPoints = 0;
            foreach (Recipe recipe in recipeList)
            {
                if ((recipe.introducedCrop == -1 || islandScript.GetResourceAmount(ResourceScript.ResourceType.Crop, recipe.introducedCrop) > 0)
                    && (recipe.nativeCrop == -1 || islandScript.GetResourceAmount(ResourceScript.ResourceType.Crop, recipe.nativeCrop + 5) > 0)
                    && (recipe.meat == -1 || islandScript.GetResourceAmount(ResourceScript.ResourceType.Meat, recipe.meat) > 0))
                {
                    hungerPoints += recipe.hungerPoints * level;
                    if (recipe.introducedCrop != -1) islandScript.UseResource(ResourceScript.ResourceType.Crop, recipe.introducedCrop);
                    if (recipe.nativeCrop != -1) islandScript.UseResource(ResourceScript.ResourceType.Crop, recipe.nativeCrop + 5);
                    if (recipe.meat != -1) islandScript.UseResource(ResourceScript.ResourceType.Meat, recipe.meat);

                    if (hungerPoints >= peasantScript.hunger)
                    {
                        hungerPoints = peasantScript.hunger;
                        break;
                    }
                }
            }

            peasantList.Remove(peasantScript);
            peasantScript.transform.parent = islandScript.npcsTransform;
            peasantScript.navMeshAgent.Warp(entry.position);
            peasantScript.isInBuilding = false;
            peasantScript.hunger -= hungerPoints;
            peasantScript.tavern = null;

            if (peasantScript.peasantType == PeasantScript.PeasantType.Adult)
            {
                PeasantAdultScript peasantAdultScript = (PeasantAdultScript)peasantScript;
                if (peasantAdultScript.task == null) peasantAdultScript.taskSourceInterface.GetNextPendingTask(peasantAdultScript);
                else peasantScript.UpdateTask();
            }
            else peasantScript.UpdateTask();

            peasantsInside--;
        }
    }

    public override PeasantScript RemovePeasant()
    {
        PeasantScript peasantScript = base.RemovePeasant();
        peasantScript.tavern = null;
        return peasantScript;
    }

}