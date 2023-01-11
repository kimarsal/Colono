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

        penRows = new PenRowScript[System.Enum.GetValues(typeof(AnimalType)).Length];
        foreach (AnimalType animalType in (AnimalType[])Enum.GetValues(typeof(AnimalType)))
        {
            GameObject gridRow = Instantiate(penRowPrefab, rows);
            PenRowScript gridRowScript = gridRow.GetComponent<PenRowScript>();
            gridRowScript.penEditor = this;
            gridRowScript.animalType = animalType;
            gridRowScript.animalImage.sprite = islandEditor.animalSprites[(int)animalType];
            gridRowScript.penAnimals = penScript.animals[(int)animalType];
            gridRowScript.shipAnimals = shipScript.animals[(int)animalType];
            gridRowScript.desiredAmount = penScript.desiredAmounts[(int)animalType];
            gridRowScript.UpdateValues();
            penRows[(int)animalType] = gridRowScript;
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
            AnimalScript animalScript = shipScript.RemoveAnimal(animalType);
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

        animalScript.transform.parent = penScript.transform;
        animalScript.gameObject.SetActive(true);
        if (animalScript.animalType == AnimalType.Chicken)
        {
            animalScript.transform.localScale = Vector3.one * 0.1f;
        }
        else if (animalScript.animalType == AnimalType.Chick)
        {
            animalScript.transform.localScale = Vector3.one * 0.05f;
        }
        else
        {
            animalScript.transform.localScale = Vector3.one;
        }
        animalScript.penScript = penScript;
        animalScript.SetNewPen();
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
                return animalScript;
            }
        }
        return null;
    }

    public void ChangeDesiredAmount(AnimalType animalType, int difference)
    {
        penScript.ChangeDesiredAmount(animalType, difference);
    }

}
