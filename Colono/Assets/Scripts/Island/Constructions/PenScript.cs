using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static ResourceScript;

public class PenScript : EnclosureScript
{
    public Transform animalTransform;
    private CanvasScript canvasScript;
    public int[] animals;
    public int[] desiredAmounts;
    public List<AnimalScript> animalList = new List<AnimalScript>();
    public CropType[] pairingCrops;
    public List<AnimalScript>[] animalsReadyForPairing;
    public int animalAmount;
    private int animalTypes;
    private int lastAnimalTypePairing = 0;

    public GameObject openGate;
    public GameObject closedGate;
    public bool npcIsInside = false;
    public bool npcHasEntered = false;
    public bool npcHasExited = false;

    private void Start()
    {
        animalTypes = Enum.GetValues(typeof(AnimalType)).Length;
        animals = new int[animalTypes];
        desiredAmounts = new int[animalTypes];
        animalsReadyForPairing = new List<AnimalScript>[animalTypes];
        pairingCrops = new CropType[animalTypes];
        for (int i = 0; i < animalTypes; i++)
        {
            animalsReadyForPairing[i] = new List<AnimalScript>();
        }

        openGate.GetComponent<PenDoorScript>().penScript = this;
        closedGate.GetComponent<PenDoorScript>().penScript = this;
        canvasScript = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>().canvasScript;
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
                GameObject animalPrefab = islandScript.islandEditor.GetAnimalPrefab(animal.animalType);
                AnimalScript animalScript = Instantiate(animalPrefab, animal.position,
                    Quaternion.Euler(0, animal.orientation, 0), animalTransform).GetComponent<AnimalScript>();
                animalScript.InitializeAnimal(animal);

            }
        }
    }

    void Update()
    {
        if (npcHasEntered)
        {
            openGate.SetActive(true);
            closedGate.SetActive(false);
            npcHasEntered = false;
        }
        else if (npcHasExited)
        {
            if (!npcIsInside)
            {
                openGate.SetActive(false);
                closedGate.SetActive(true);
                npcHasExited = false;
            }
        }
    }

    public override void EditConstruction()
    {
        islandScript.gameManager.canvasScript.ShowPenEditor();
    }

    public void AgeUpAnimal(AnimalScript animalScript)
    {
        AnimalType animalType = animalScript.animalType;
        GameObject agedUpAnimal = islandScript.islandEditor.GetAnimalPrefab(animalType + 1);
        AnimalScript agedUpAnimalScript = Instantiate(agedUpAnimal, animalScript.transform.position, animalScript.transform.rotation, transform).GetComponent<AnimalScript>();
        animalList.Add(agedUpAnimalScript);
        animalList.Remove(animalScript);
        Destroy(animalScript.gameObject);

        animals[(int)animalType]--;
        animals[(int)animalType + 1]++;

        canvasScript.UpdatePenRow(animalType);
        canvasScript.UpdatePenRow(animalType + 1);

        if (animalScript.animalType == AnimalType.Chicken)
        {
            agedUpAnimal.transform.localScale = Vector3.one * 0.1f;
        }
        agedUpAnimalScript.penScript = this;
    }

    public void BreedAnimals(AnimalScript animalScript1, AnimalScript animalScript2)
    {
        animalScript1.EndPairing();
        animalScript2.EndPairing();

        AnimalType animalType = animalScript1.animalType - 1;
        GameObject bornAnimal = islandScript.islandEditor.GetAnimalPrefab(animalType);
        AnimalScript bornAnimalScript = Instantiate(bornAnimal, animalScript1.transform.position, animalScript1.transform.rotation, transform).GetComponent<AnimalScript>();
        animalList.Add(bornAnimalScript);

        animals[(int)animalType]++;
        canvasScript.UpdatePenRow(animalType);

        if (bornAnimalScript.animalType == AnimalType.Chick)
        {
            bornAnimal.transform.localScale = Vector3.one * 0.05f;
        }
        bornAnimalScript.penScript = this;
    }

    private void PutAnimalOutOfItsMisery(int animalType)
    {

    }

    public void ChangeDesiredAmount(AnimalType animalType, int difference)
    {
        desiredAmounts[(int)animalType] += difference;
        TryToCreatePairing((int)animalType);
    }

    public void ChangePairingCrop(AnimalType animalType, CropType cropType)
    {
        pairingCrops[(int)animalType] = cropType;
        TryToCreatePairing((int)animalType);
    }

    public void AnimalIsReadyForPairing(AnimalScript animalScript)
    {
        animalsReadyForPairing[(int)animalScript.animalType].Add(animalScript);
        TryToCreatePairing((int)animalScript.animalType);
    }

    private void TryToCreatePairing(int animalType)
    {
        if (CheckIfPairingIsPossible(animalType))
        {
            foreach (PeasantAdultScript peasantScript in peasantList)
            {
                if (GetNextPendingTask(peasantScript))
                {
                    break;
                }
            }
        }
    }

    private bool CheckIfPairingIsPossible(int animalType)
    {
        return animalsReadyForPairing[animalType].Count == 2 //Hi ha dos animals preparats per aparellar-se
            && desiredAmounts[animalType] > animals[animalType] //Es volen més animals del mateix tipus
            && islandScript.GetResourceAmount(ResourceType.Crop, (int)pairingCrops[animalType]) > 0; //Es té la verdura necessària per aparellar-los
    }

    private PairingScript CreatePairing(int animalType, PeasantAdultScript peasantAdultScript)
    {
        Vector3 position = NPCManager.GetRandomPointWithinRange(minPos, maxPos);

        GameObject pairing = new GameObject("Pairing");
        pairing.transform.parent = transform;
        pairing.transform.position = position;

        PairingScript pairingScript = pairing.AddComponent<PairingScript>();
        pairingScript.InitializePairing(this, animalType, peasantAdultScript);
        return pairingScript;
    }

    public override bool GetNextPendingTask(PeasantAdultScript peasantAdultScript)
    {
        if (!peasantAdultScript.CanBeAsignedTask()) return false;

        TaskScript nextTaskScript = null;
        int index = (lastAnimalTypePairing + 1) % animalTypes;
        while (index != lastAnimalTypePairing)
        {
            if (CheckIfPairingIsPossible(index))
            {
                lastAnimalTypePairing = index;
                nextTaskScript = CreatePairing(index, peasantAdultScript);
                return true;
            }
            else if (index % 2 != 0 && desiredAmounts[index] < animals[index])
            {
                PutAnimalOutOfItsMisery(index);
            }
            index = (index + 1) % animalTypes;
        }
        peasantAdultScript.AssignTask(nextTaskScript);
        return nextTaskScript != null;
    }

}