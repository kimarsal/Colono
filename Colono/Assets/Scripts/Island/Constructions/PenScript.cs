using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ResourceScript;

public class PenScript : EnclosureScript
{
    public Transform animalTransform;
    public int[] animals;
    public int[] desiredAmounts;
    public List<AnimalScript> animalList = new List<AnimalScript>();
    public List<AnimalScript>[] confortableAnimals;
    public int animalAmount;

    public override bool canManagePeasants { get { return false; } }

    public override EditorScript editorScript { get { return CanvasScript.Instance.penEditor; } }

    private void Start()
    {
        if(animals == null || animals.Length == 0) InitializeLists();
    }

    private void InitializeLists()
    {
        int animalTypes = Enum.GetValues(typeof(AnimalType)).Length;
        animals = new int[animalTypes];
        desiredAmounts = new int[animalTypes];
        confortableAnimals = new List<AnimalScript>[animalTypes];
        for (int i = 0; i < animalTypes; i++)
        {
            confortableAnimals[i] = new List<AnimalScript>();
        }
    }

    public override void InitializeEnclosure(EnclosureScript enclosureScript, IslandScript islandScript)
    {
        base.InitializeEnclosure(enclosureScript, islandScript);

        animalTransform = new GameObject("Animals").transform;
        animalTransform.parent = transform;
        animalTransform.localPosition = Vector3.zero;

        PenScript penScript = (PenScript)enclosureScript;

        if (penScript != null)
        {
            foreach (AnimalScript animal in penScript.animalList)
            {
                AnimalScript animalScript = Instantiate(animal.gameObject, animal.position,
                    Quaternion.Euler(0, animal.orientation, 0), animalTransform).GetComponent<AnimalScript>();
                animalScript.InitializeAnimal(animal);

            }
        }
    }

    public void AddAnimal(AnimalScript animal)
    {
        AnimalScript animalScript = Instantiate(animal.gameObject, NPCManager.GetRandomPointWithinRange(minPos, maxPos),
                    Quaternion.Euler(0, animal.orientation, 0), animalTransform).GetComponent<AnimalScript>();
        animalScript.InitializeAnimal(animal);
        Destroy(animal.gameObject);

        if(animals == null || animals.Length == 0) InitializeLists();
        animalList.Add(animalScript);
        animals[(int)animalScript.animalType]++;
        desiredAmounts[(int)animalScript.animalType]++;

        animalScript.penScript = this;
    }

    public AnimalScript RemoveAnimal(AnimalType animalType)
    {
        for (int i = 0; i < animalList.Count; i++)
        {
            AnimalScript animalScript = animalList[i];
            if (animalScript.animalType == animalType)
            {
                animalList.RemoveAt(i);
                animals[(int)animalType]--;
                return animalScript;
            }
        }
        return null;
    }

    public void AgeUpAnimal(AnimalScript animalScript)
    {
        AnimalType animalType = animalScript.animalType;
        AnimalScript agedUpAnimalScript = Instantiate(ResourceScript.Instance.GetAnimalPrefab(animalType + 1),
            animalScript.transform.position, animalScript.transform.rotation, animalTransform).GetComponent<AnimalScript>();
        animalList.Add(agedUpAnimalScript);
        animalList.Remove(animalScript);
        Destroy(animalScript.gameObject);

        animals[(int)animalType]--;
        animals[(int)animalType + 1]++;

        CanvasScript.Instance.UpdatePenRow(animalType);
        CanvasScript.Instance.UpdatePenRow(animalType + 1);

        agedUpAnimalScript.penScript = this;
    }

    public void BreedAnimals(AnimalScript animalScript1, AnimalScript animalScript2)
    {
        animalScript1.EndPairing();
        animalScript2.EndPairing();

        AnimalType animalType = animalScript1.animalType - 1;
        AnimalScript bornAnimalScript = Instantiate(ResourceScript.Instance.GetAnimalPrefab(animalType),
            animalScript1.transform.position, animalScript1.transform.rotation, transform).GetComponent<AnimalScript>();
        animalList.Add(bornAnimalScript);

        animals[(int)animalType]++;
        CanvasScript.Instance.UpdatePenRow(animalType);
        bornAnimalScript.penScript = this;
    }

    public void AnimalIsConfortable(AnimalScript animalScript)
    {
        confortableAnimals[(int)animalScript.animalType].Add(animalScript);
        ManageAnimals((int)animalScript.animalType);
    }

    public void ChangeDesiredAmount(AnimalType animalType, int difference)
    {
        desiredAmounts[(int)animalType] += difference;
        ManageAnimals((int)animalType);
    }

    private void ManageAnimals(int animalType)
    {
        if (confortableAnimals[animalType].Count >= 2 //Hi ha dos animals preparats per aparellar-se
            && desiredAmounts[animalType] > animals[animalType]) //Es volen més animals del mateix tipus
        {
            AnimalScript animalScript1 = confortableAnimals[animalType][0];
            AnimalScript animalScript2 = confortableAnimals[animalType][1];
            confortableAnimals[animalType].RemoveAt(0);
            confortableAnimals[animalType].RemoveAt(0);

            animalScript1.GetReadyForPairing(animalScript2);
            animalScript2.GetReadyForPairing(animalScript1);

            return;
        }

        if (confortableAnimals[animalType].Count > 0
            && desiredAmounts[animalType] < animals[animalType])
        {
            AnimalScript animalScript = confortableAnimals[animalType][0];
            confortableAnimals[animalType].RemoveAt(0);
            StartCoroutine(animalScript.Die());
        }
    }

    public override void FinishUpBusiness()
    {
        base.FinishUpBusiness();
        foreach (AnimalScript animalScript in animalList)
        {
            ShipScript.Instance.shipInteriorPen.AddAnimal(animalScript);
        }
    }
}