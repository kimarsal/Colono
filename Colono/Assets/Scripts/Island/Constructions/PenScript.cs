using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ResourceScript;

public class PenScript : EnclosureScript
{
    private Transform animalTransform;
    private CanvasScript canvasScript;
    public int[] animals = new int[Enum.GetValues(typeof(AnimalType)).Length];
    public int[] desiredAmounts = new int[Enum.GetValues(typeof(AnimalType)).Length];
    public List<AnimalScript> animalList = new List<AnimalScript>();
    public int animalAmount;

    public GameObject openGate;
    public GameObject closedGate;
    public bool npcIsInside = false;
    public bool npcHasEntered = false;
    public bool npcHasExited = false;

    private void Start()
    {
        openGate.GetComponent<PenDoorScript>().penScript = this;
        closedGate.GetComponent<PenDoorScript>().penScript = this;
        canvasScript = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>().canvasScript;
    }

    public void InitializePen(PenInfo penInfo)
    {
        animalTransform = new GameObject("Animals").transform;
        animalTransform.parent = transform;
        animalTransform.localPosition = Vector3.zero;

        foreach (AnimalInfo animalInfo in penInfo.animalList)
        {
            GameObject animalPrefab = islandEditor.GetAnimalPrefab(animalInfo.animalType);
            AnimalScript animalScript = Instantiate(animalPrefab, animalInfo.position.UnityVector,
                Quaternion.Euler(0, animalInfo.orientation, 0), animalTransform).GetComponent<AnimalScript>();
            animalScript.InitializeAnimal(animalInfo);

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

    public void AgeUpAnimal(AnimalScript animalScript)
    {
        AnimalType animalType = animalScript.animalType;
        GameObject agedUpAnimal = islandEditor.GetAnimalPrefab(animalType + 1);
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
        else
        {
            agedUpAnimal.transform.localScale = Vector3.one;
        }
        agedUpAnimalScript.penScript = this;
    }

    public void ChangeDesiredAmount(AnimalType animalType, int difference)
    {
        desiredAmounts[(int)animalType] += difference;
    }

    public override TaskScript GetNextPendingTask()
    {
        return null;
    }

    public PenInfo GetPenInfo()
    {
        PenInfo penInfo = new PenInfo();
        foreach(AnimalScript animalScript in animalList)
        {
            penInfo.animalList.Add(animalScript.GetAnimalInfo());
        }
        return penInfo;
    }
}

[System.Serializable]
public class PenInfo : EnclosureInfo
{
    public List<AnimalInfo> animalList = new List<AnimalInfo>();
}
