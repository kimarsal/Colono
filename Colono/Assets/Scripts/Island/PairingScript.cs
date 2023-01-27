using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PairingScript : TaskScript
{
    private AnimalScript animalScript1;
    private AnimalScript animalScript2;
    private int participantsInPlace;
    private ResourceScript.CropType pairingCrop;

    void Start()
    {
        taskType = TaskType.Pairing;
    }

    public void InitializePairing(PenScript penScript, int animalType, PeasantAdultScript peasantAdultScript)
    {
        center = transform.position;
        taskSourceScript = penScript;
        this.peasantAdultScript = peasantAdultScript;
        animalScript1 = penScript.animalsReadyForPairing[animalType][0];
        animalScript2 = penScript.animalsReadyForPairing[animalType][1];
        penScript.animalsReadyForPairing[animalType].RemoveAt(0);
        penScript.animalsReadyForPairing[animalType].RemoveAt(0);

        pairingCrop = penScript.pairingCrops[animalType];
        penScript.islandScript.UseResource(ResourceScript.ResourceType.Crop, (int)pairingCrop);
    }

    public void StartPairing()
    {
        animalScript1.GetReadyForPairing(this);
        animalScript2.GetReadyForPairing(this);
    }

    public void ParticipantHasArrived()
    {
        participantsInPlace++;
        if(participantsInPlace == 3)
        {
            peasantAdultScript.FeedAnimals();
        }
    }

    public override void TaskProgress()
    {
        ((PenScript)taskSourceScript).BreedAnimals(animalScript1, animalScript2);
        taskSourceScript.GetNextPendingTask(peasantAdultScript);
        Destroy(gameObject);
    }

    public override void CancelTask()
    {
        ((PenScript)taskSourceScript).islandScript.AddResource(ResourceScript.ResourceType.Crop, (int)pairingCrop);
        animalScript1.EndPairing();
        animalScript2.EndPairing();
        taskSourceScript.GetNextPendingTask(peasantAdultScript);
        Destroy(gameObject);
    }
}
