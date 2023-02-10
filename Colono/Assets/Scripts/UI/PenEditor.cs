using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static ResourceScript;
using UnityEngine.UI;
using System;

public class PenEditor : MonoBehaviour
{
    public IslandEditor islandEditor;
    public PenScript penScript;
    public ShipScript shipScript;

    public TextMeshProUGUI penAnimalsText;
    public TextMeshProUGUI shipAnimalsText;

    public Transform rows;
    public ScrollRect scrollRect;
    public PenRowScript[] penRows;
    public GameObject penRowPrefab;

    public void SetGrid()
    {
        foreach (Transform row in rows)
        {
            Destroy(row.gameObject);
        }

        int animalTypes = System.Enum.GetValues(typeof(AnimalType)).Length;
        penRows = new PenRowScript[animalTypes];
        for (int i = 0; i < animalTypes; i++)
        {
            GameObject gridRow = Instantiate(penRowPrefab, rows);
            PenRowScript penRowScript = gridRow.GetComponent<PenRowScript>();
            penRowScript.penEditor = this;
            penRowScript.animalType = (AnimalType)i;
            penRowScript.animalImage.sprite = islandEditor.GetResourceSprite(ResourceType.Animal, i);
            penRowScript.penAnimals = penScript.animals[i];
            penRowScript.shipAnimals = shipScript.animals[i];
            penRowScript.desiredAmount = penScript.desiredAmounts[i];
            penRowScript.pairingCrop = penScript.pairingCrops[i];
            penRowScript.UpdateValues();
            penRows[i] = penRowScript;
        }

        UpdatePenText();

    }

    public void UpdatePenRow(AnimalType animalType)
    {
        PenRowScript penRowScript = penRows[(int)animalType];
        penRowScript.penAnimals = penScript.animals[(int)animalType];
        penRowScript.shipAnimals = shipScript.animals[(int)animalType];
        penRowScript.UpdateValues();
        UpdatePenText();
    }

    public void UpdatePenText()
    {
        penAnimalsText.text = penScript.animalAmount + "/" + penScript.maxPeasants;
        shipAnimalsText.text = shipScript.animalAmount + "/" + shipScript.maxPeasants;
    }

    public void MoveAnimal(AnimalType animalType, bool toPen)
    {
        if(toPen)
        {
            AnimalScript animalScript = shipScript.RemoveAnimal(penScript, animalType);
            AddAnimal(animalScript);
        }
        else
        {
            AnimalScript animalScript = RemoveAnimal(animalType);
            shipScript.AddAnimal(animalScript);
        }

        UpdatePenText();
    }

    public void AddAnimal(AnimalScript animalScript)
    {
        penScript.animalList.Add(animalScript);
        penScript.animals[(int)animalScript.animalType]++;
        penScript.animalAmount++;
        penScript.desiredAmounts[(int)animalScript.animalType]++;

        if (animalScript.animalType == AnimalType.Chicken)
        {
            animalScript.transform.localScale = Vector3.one * 0.1f;
        }
        else if (animalScript.animalType == AnimalType.Chick)
        {
            animalScript.transform.localScale = Vector3.one * 0.05f;
        }
        animalScript.penScript = penScript;
        

    }

    public AnimalScript RemoveAnimal(AnimalType animalType)
    {
        for(int i = 0; i < penScript.animalList.Count; i++)
        {
            AnimalScript animalScript = penScript.animalList[i];
            if(animalScript.animalType == animalType)
            {
                penScript.animalList.RemoveAt(i);
                penScript.animals[(int)animalType]--;
                penScript.animalAmount--;
                Destroy(animalScript.gameObject);
                return animalScript;
            }
        }
        return null;
    }

    public void ChangeDesiredAmount(AnimalType animalType, int difference)
    {
        penScript.ChangeDesiredAmount(animalType, difference);
    }

    public void ChangePairingCrop(AnimalType animalType, CropType cropType)
    {
        penScript.ChangePairingCrop(animalType, cropType);
    }

}
