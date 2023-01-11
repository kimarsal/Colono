using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using static TavernEditor;

public class TavernEditor : MonoBehaviour
{
    public TavernScript tavernScript;
    public Transform listTransform;
    public GameObject rowPrefab;
    public Transform addRecipeItemTransform;
    public List<Dropdown.OptionData> introducedCropDropdownOptions;
    public List<Dropdown.OptionData> nativeCropDropdownOptions;
    public List<Dropdown.OptionData> meatDropdownOptions;

    public void SetList()
    {
        for (int i = 0; i < listTransform.childCount - 1; i++)
        {
            Destroy(listTransform.GetChild(i).gameObject);
        }

        int introducedCropTypes = System.Enum.GetValues(typeof(ResourceScript.CropType)).Length / 2;
        int nativeCropTypes = introducedCropTypes;
        int meatTypes = System.Enum.GetValues(typeof(ResourceScript.MeatType)).Length;

        introducedCropDropdownOptions = new List<Dropdown.OptionData>(introducedCropTypes+1);
        introducedCropDropdownOptions.Add(new Dropdown.OptionData());
        for (int i = 0; i < introducedCropTypes; i++)
        {
            introducedCropDropdownOptions.Add(new Dropdown.OptionData(tavernScript.islandEditor.cropSprites[i]));
        }

        nativeCropDropdownOptions = new List<Dropdown.OptionData>(nativeCropTypes+1);
        nativeCropDropdownOptions.Add(new Dropdown.OptionData());
        for (int i = 0; i < nativeCropTypes; i++)
        {
            nativeCropDropdownOptions.Add(new Dropdown.OptionData(tavernScript.islandEditor.cropSprites[introducedCropTypes + i]));
        }

        meatDropdownOptions = new List<Dropdown.OptionData>(meatTypes+1);
        meatDropdownOptions.Add(new Dropdown.OptionData());
        for (int i = 0; i < meatTypes; i++)
        {
            meatDropdownOptions.Add(new Dropdown.OptionData(tavernScript.islandEditor.meatSprites[i]));
        }

        foreach (Recipe recipe in tavernScript.recipes)
        {
            GameObject row = Instantiate(rowPrefab, listTransform);
            RecipeRowScript recipeRowScript = row.GetComponent<RecipeRowScript>();
            recipeRowScript.tavernEditor = this;
            recipeRowScript.recipe = recipe;
            recipeRowScript.SetRow();
        }

        addRecipeItemTransform.SetAsLastSibling();

        listTransform.GetChild(0).GetComponent<RecipeRowScript>().moveRecipeUpButton.enabled = false;
        listTransform.GetChild(tavernScript.recipes.Count - 1).GetComponent<RecipeRowScript>().moveRecipeDownButton.enabled = false;

    }

    public void AddRecipe()
    {
        Recipe recipe = new Recipe();
        tavernScript.recipes.Add(recipe);

        GameObject row = Instantiate(rowPrefab, listTransform);
        RecipeRowScript recipeRowScript = row.GetComponent<RecipeRowScript>();
        recipeRowScript.tavernEditor = this;
        recipeRowScript.recipe = recipe;
        recipeRowScript.SetRow();
        recipeRowScript.moveRecipeDownButton.enabled = false;

        addRecipeItemTransform.SetAsLastSibling();

        listTransform.GetChild(0).GetComponent<RecipeRowScript>().removeRecipeButton.enabled = true;
        listTransform.GetChild(tavernScript.recipes.Count - 2).GetComponent<RecipeRowScript>().moveRecipeDownButton.enabled = true;
    }

    public void MoveRecipe(RecipeRowScript row, bool moveUp)
    {
        int index = row.transform.GetSiblingIndex();
        int nextIndex = index + (moveUp ? -1 : 1);

        if (nextIndex < 0 || nextIndex == tavernScript.recipes.Count) return;

        Recipe aux = tavernScript.recipes[nextIndex];
        tavernScript.recipes[nextIndex] = tavernScript.recipes[index];
        tavernScript.recipes[index] = aux;

        row.transform.SetSiblingIndex(nextIndex);
        listTransform.GetChild(index).GetComponent<RecipeRowScript>().moveRecipeUpButton.enabled = index > 0;
        listTransform.GetChild(index).GetComponent<RecipeRowScript>().moveRecipeDownButton.enabled = index < tavernScript.recipes.Count - 1;
        listTransform.GetChild(nextIndex).GetComponent<RecipeRowScript>().moveRecipeUpButton.enabled = nextIndex > 0;
        listTransform.GetChild(nextIndex).GetComponent<RecipeRowScript>().moveRecipeDownButton.enabled = nextIndex < tavernScript.recipes.Count - 1;
    }

    public void RemoveRecipe(RecipeRowScript row)
    {
        if (tavernScript.recipes.Count == 1) return;

        int index = tavernScript.recipes.IndexOf(row.recipe);
        tavernScript.recipes.Remove(row.recipe);

        Destroy(listTransform.GetChild(index).gameObject);

        listTransform.GetChild(0).GetComponent<RecipeRowScript>().moveRecipeUpButton.enabled = false;
        listTransform.GetChild(tavernScript.recipes.Count - 1).GetComponent<RecipeRowScript>().moveRecipeDownButton.enabled = false;
        if (tavernScript.recipes.Count == 1) listTransform.GetChild(0).GetComponent<RecipeRowScript>().removeRecipeButton.enabled = false;
    }

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

        public int hungerPoints { get
            {
                int ingredients = 0;
                if (introducedCrop != -1) ingredients++;
                if (nativeCrop != -1) ingredients++;
                if (meat != -1) ingredients++;
                return ingredients * 3;
            }
        }
    }


}
