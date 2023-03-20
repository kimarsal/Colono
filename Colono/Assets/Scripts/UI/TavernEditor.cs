using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TavernEditor : EditorScript
{
    private TavernScript tavernScript;
    [SerializeField] private Transform listTransform;
    [SerializeField] private GameObject rowPrefab;
    [SerializeField] private Transform addRecipeItemTransform;
    public List<Dropdown.OptionData> introducedCropDropdownOptions;
    public List<Dropdown.OptionData> nativeCropDropdownOptions;
    public List<Dropdown.OptionData> meatDropdownOptions;

    public override void SetEditor(ConstructionScript constructionScript)
    {
        tavernScript = (TavernScript)constructionScript;

        for (int i = 0; i < listTransform.childCount - 1; i++)
        {
            Destroy(listTransform.GetChild(i).gameObject);
        }

        int introducedCropTypes = System.Enum.GetValues(typeof(ResourceScript.CropType)).Length / 2;
        int nativeCropTypes = introducedCropTypes;
        int meatTypes = System.Enum.GetValues(typeof(ResourceScript.MeatType)).Length;

        introducedCropDropdownOptions = new List<Dropdown.OptionData> { new Dropdown.OptionData() }; //S'afegeix una opció buida
        for (int i = 0; i < introducedCropTypes; i++)
        {
            introducedCropDropdownOptions.Add(new Dropdown.OptionData(IslandEditor.Instance.GetResourceSprite(ResourceScript.ResourceType.Crop, i)));
        }

        nativeCropDropdownOptions = new List<Dropdown.OptionData> { new Dropdown.OptionData() }; //S'afegeix una opció buida
        for (int i = 0; i < nativeCropTypes; i++)
        {
            nativeCropDropdownOptions.Add(new Dropdown.OptionData(IslandEditor.Instance.GetResourceSprite(ResourceScript.ResourceType.Crop, introducedCropTypes + i)));
        }

        meatDropdownOptions = new List<Dropdown.OptionData> { new Dropdown.OptionData() }; //S'afegeix una opció buida
        for (int i = 0; i < meatTypes; i++)
        {
            meatDropdownOptions.Add(new Dropdown.OptionData(IslandEditor.Instance.GetResourceSprite(ResourceScript.ResourceType.Meat, i)));
        }

        foreach (Recipe recipe in tavernScript.recipeList)
        {
            GameObject row = Instantiate(rowPrefab, listTransform);
            RecipeRowScript recipeRowScript = row.GetComponent<RecipeRowScript>();
            recipeRowScript.tavernEditor = this;
            recipeRowScript.recipe = recipe;
            recipeRowScript.SetRow();
        }

        addRecipeItemTransform.SetAsLastSibling();

        listTransform.GetChild(0).GetComponent<RecipeRowScript>().moveRecipeUpButton.enabled = false;
        listTransform.GetChild(tavernScript.recipeList.Count - 1).GetComponent<RecipeRowScript>().moveRecipeDownButton.enabled = false;

    }

    public void AddRecipe()
    {
        Recipe recipe = new Recipe();
        tavernScript.recipeList.Add(recipe);

        GameObject row = Instantiate(rowPrefab, listTransform);
        RecipeRowScript recipeRowScript = row.GetComponent<RecipeRowScript>();
        recipeRowScript.tavernEditor = this;
        recipeRowScript.recipe = recipe;
        recipeRowScript.SetRow();
        recipeRowScript.moveRecipeDownButton.enabled = false;

        addRecipeItemTransform.SetAsLastSibling();

        listTransform.GetChild(0).GetComponent<RecipeRowScript>().removeRecipeButton.enabled = true;
        listTransform.GetChild(tavernScript.recipeList.Count - 2).GetComponent<RecipeRowScript>().moveRecipeDownButton.enabled = true;
    }

    public void MoveRecipe(RecipeRowScript row, bool moveUp)
    {
        int index = row.transform.GetSiblingIndex();
        int nextIndex = index + (moveUp ? -1 : 1);

        if (nextIndex < 0 || nextIndex == tavernScript.recipeList.Count) return;

        Recipe aux = tavernScript.recipeList[nextIndex];
        tavernScript.recipeList[nextIndex] = tavernScript.recipeList[index];
        tavernScript.recipeList[index] = aux;

        row.transform.SetSiblingIndex(nextIndex);
        listTransform.GetChild(index).GetComponent<RecipeRowScript>().moveRecipeUpButton.enabled = index > 0;
        listTransform.GetChild(index).GetComponent<RecipeRowScript>().moveRecipeDownButton.enabled = index < tavernScript.recipeList.Count - 1;
        listTransform.GetChild(nextIndex).GetComponent<RecipeRowScript>().moveRecipeUpButton.enabled = nextIndex > 0;
        listTransform.GetChild(nextIndex).GetComponent<RecipeRowScript>().moveRecipeDownButton.enabled = nextIndex < tavernScript.recipeList.Count - 1;
    }

    public void RemoveRecipe(RecipeRowScript row)
    {
        if (tavernScript.recipeList.Count == 1) return;

        int index = tavernScript.recipeList.IndexOf(row.recipe);
        tavernScript.recipeList.Remove(row.recipe);

        Destroy(listTransform.GetChild(index).gameObject);

        listTransform.GetChild(0).GetComponent<RecipeRowScript>().moveRecipeUpButton.enabled = false;
        listTransform.GetChild(tavernScript.recipeList.Count - 1).GetComponent<RecipeRowScript>().moveRecipeDownButton.enabled = false;
        if (tavernScript.recipeList.Count == 1) listTransform.GetChild(0).GetComponent<RecipeRowScript>().removeRecipeButton.enabled = false;
    }

}

[System.Serializable]
public class Recipe
{
    public int introducedCrop;
    public int nativeCrop;
    public int meat;

    public Recipe()
    {
        introducedCrop = (int)ResourceScript.CropType.Onion;
        nativeCrop = -1;
        meat = -1;
    }

    public int hungerPoints
    {
        get
        {
            int ingredients = 0;
            if (introducedCrop != -1) ingredients++;
            if (nativeCrop != -1) ingredients++;
            if (meat != -1) ingredients++;
            return ingredients * 3;
        }
    }
}