using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeRowScript : MonoBehaviour
{
    public TavernEditor tavernEditor;
    public TavernEditor.Recipe recipe;

    public Button moveRecipeUpButton;
    public Button moveRecipeDownButton;
    public Dropdown introducedCropDropdown;
    public Dropdown nativeCropDropdown;
    public Dropdown meatDropdown;
    public Button removeRecipeButton;

    public GameObject multipleIngredientDish;
    public Image singleIngredientImage;
    public Image firstIngredientImage;
    public Image secondIngredientImage;
    public Image thirdIngredientImage;
    public TextMeshProUGUI hungerPointsText;

    public int ingredientAmount;

    public void SetRow()
    {
        introducedCropDropdown.options = tavernEditor.introducedCropDropdownOptions;
        nativeCropDropdown.options = tavernEditor.nativeCropDropdownOptions;
        meatDropdown.options = tavernEditor.meatDropdownOptions;

        introducedCropDropdown.value = recipe.introducedCrop + 1;
        nativeCropDropdown.value = recipe.nativeCrop + 1;
        meatDropdown.value = recipe.meat + 1;
    }

    public void RecipeChange()
    {
        ingredientAmount = 0;
        if (recipe.meat != -1)
        {
            Sprite sprite = tavernEditor.tavernScript.islandEditor.cookedMeatSprites[recipe.meat];
            singleIngredientImage.sprite = sprite;
            firstIngredientImage.sprite = sprite;
            ingredientAmount++;
        }

        if (recipe.introducedCrop != -1) {
            Sprite sprite = tavernEditor.tavernScript.islandEditor.cookedVegetableSprites[recipe.introducedCrop];
            if(recipe.meat == -1)
            {
                firstIngredientImage.sprite = sprite;
                singleIngredientImage.sprite = sprite;
            }
            else
            {
                secondIngredientImage.sprite = sprite;
            }
            ingredientAmount++;
        }

        if (recipe.nativeCrop != -1)
        {
            Sprite sprite = tavernEditor.tavernScript.islandEditor.cookedVegetableSprites[System.Enum.GetValues(typeof(ResourceScript.CropType)).Length / 2 + recipe.nativeCrop];
            if (recipe.meat == -1 && recipe.introducedCrop == -1)
            {
                singleIngredientImage.sprite = sprite;
            }
            else if (recipe.meat == -1)
            {
                secondIngredientImage.sprite = sprite;
            }
            else
            {
                thirdIngredientImage.sprite = sprite;
            }
            ingredientAmount++;
        }

        if(ingredientAmount == 1)
        {
            multipleIngredientDish.SetActive(false);
            singleIngredientImage.gameObject.SetActive(true);
        }
        else
        {
            singleIngredientImage.gameObject.SetActive(false);
            multipleIngredientDish.SetActive(true);
            thirdIngredientImage.gameObject.SetActive(ingredientAmount == 3);
        }

        hungerPointsText.text = recipe.hungerPoints.ToString();
    }

    public void IntroducedCropDropdownValueChange(int value)
    {
        if(ingredientAmount == 1 && value == 0)
        {
            introducedCropDropdown.value = recipe.introducedCrop + 1;
        }
        else
        {
            ingredientAmount += value == 0 ? -1 : 1;
            recipe.introducedCrop = introducedCropDropdown.value - 1;
            RecipeChange();
        }
    }

    public void NativeCropDropdownValueChange(int value)
    {
        if (ingredientAmount == 1 && value == 0)
        {
            nativeCropDropdown.value = recipe.nativeCrop + 1;
        }
        else
        {
            recipe.nativeCrop = nativeCropDropdown.value - 1;
            RecipeChange();
        }
    }

    public void MeatDropdownValueChange(int value)
    {
        if (ingredientAmount == 1 && value == 0)
        {
            meatDropdown.value = recipe.meat + 1;
        }
        else
        {
            recipe.meat = meatDropdown.value - 1;
            RecipeChange();
        }
    }

    public void MoveRecipe(bool moveUp)
    {
        tavernEditor.MoveRecipe(this, moveUp);
    }

    public void RemoveRecipe()
    {
        tavernEditor.RemoveRecipe(this);
    }
}
