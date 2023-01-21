using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PenRowScript : MonoBehaviour
{
    public PenEditor penEditor;

    public ResourceScript.AnimalType animalType;

    public GameObject blocker;
    public Image animalImage;
    public TextMeshProUGUI animalsInPenText;
    public Button moveAnimalToPenButton;
    public Button moveAnimalToShipButton;
    public TextMeshProUGUI animalsInShipText;
    public Dropdown cropDropdown;
    public Button increaseDesiredAmountButton;
    public Button decreaseDesiredAmountButton;
    public TextMeshProUGUI desiredAmountText;

    public int penAnimals;
    public int shipAnimals;
    public int desiredAmount;

    private void Start()
    {
        if((int)animalType % 2 == 0)
        {
            cropDropdown.gameObject.SetActive(false);
            UpdateDesiredAmount();
        }
    }

    public void UpdateValues()
    {
        blocker.SetActive(penAnimals + shipAnimals == 0);

        moveAnimalToPenButton.interactable = shipAnimals > 0;
        moveAnimalToShipButton.interactable = penAnimals > 0;

        UpdateText();
    }

    public void MoveAnimal(bool toPen)
    {
        penEditor.MoveAnimal(animalType, toPen);

        penAnimals += toPen ? 1 : -1;
        shipAnimals += toPen ? -1 : 1;

        UpdateValues();
    }

    public void ChangeDesiredAmount(int difference)
    {
        penEditor.ChangeDesiredAmount(animalType, difference);

        desiredAmount += difference;

        UpdateDesiredAmount();
    }

    public void UpdateText()
    {
        animalsInPenText.text = penAnimals.ToString();
        animalsInShipText.text = shipAnimals.ToString();
    }

    public void UpdateDesiredAmount()
    {
        desiredAmountText.text = desiredAmount.ToString();
        increaseDesiredAmountButton.interactable = desiredAmount < 99;
        decreaseDesiredAmountButton.interactable = desiredAmount > 0;
    }

}
