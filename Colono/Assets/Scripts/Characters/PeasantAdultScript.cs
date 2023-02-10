using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PeasantAdultScript : PeasantScript
{
    public TaskScript task;
    private bool hasTaskChanged;

    [Header("Tools")]
    public GameObject axe;
    public GameObject shovel;
    public GameObject basket;
    public GameObject wateringCan;

    public TaskSourceInterface taskSourceInterface
    {
        get
        {
            if (constructionScript != null && constructionScript.constructionType == ConstructionScript.ConstructionType.Enclosure)
            {
                return (EnclosureScript)constructionScript;
            }
            return islandScript;
        }
    }

    public void ToggleAxe()
    {
        axe.SetActive(!axe.activeSelf);
    }

    public void ToggleShovel()
    {
        shovel.SetActive(!shovel.activeSelf);
    }

    public void ToggleBasket()
    {
        basket.SetActive(!basket.activeSelf);
    }

    public void ToggleWateringCan()
    {
        wateringCan.SetActive(!wateringCan.activeSelf);
    }

    protected IEnumerator WaitForNextRandomDestinationInEnclosure()
    {
        yield return new WaitForSeconds(1f);
        if (constructionScript != null) //Si encara està vinculat a la construcció
        {
            SetDestination(NPCManager.GetRandomPointWithinRange(((EnclosureScript)constructionScript).minPos, ((EnclosureScript)constructionScript).maxPos));
        }
        else SetDestination(NPCManager.GetRandomPoint(transform.position));
    }

    public override void UpdateTask()
    {
        StopCharacter();
        if (tavern != null) //Si ha d'anar a menjar
        {
            SetDestination(tavern.entry.position);
        }
        else if (cabin != null) //Si ha d'anar a dormir
        {
            SetDestination(cabin.entry.position);
        }
        else if (task != null) //Si té una tasca encarregada
        {
            SetDestination(task.center);
        }
        else if (constructionScript != null) //Si té una construcció com a destí
        {
            if (constructionScript.constructionType == ConstructionScript.ConstructionType.Enclosure) //Si la construcció és exterior
            {
                SetDestination(NPCManager.GetRandomPointWithinRange(((EnclosureScript)constructionScript).minPos, ((EnclosureScript)constructionScript).maxPos));
            }
            else
            {
                SetDestination(constructionScript.entry.position);
            }
        }
        else
        {
            SetDestination(NPCManager.GetRandomPoint(transform.position));
        }
    }

    public bool CanBeAsignedTask()
    {
        return task == null && hunger < 1 && exhaustion < 1;
    }

    public void AssignTask(TaskScript taskScript)
    {
        task = taskScript;
        if (task != null) task.peasantAdultScript = this;
        UpdateTask();
    }

    public override void ArrivedAtDestination()
    {
        if (tavern != null) //Si ha anat a menjar
        {
            tavern.FeedPeasant(this);
        }
        else if (cabin != null) //Si ha anat a dormir
        {
            cabin.RestPeasant(this);
        }
        else if (task != null) //Si té una tasca encarregada
        {
            DoTask();
        }
        else if (constructionScript != null) //Si té una construcció com a destí
        {
            if (constructionScript.constructionType == ConstructionScript.ConstructionType.Enclosure) //Si el destí és exterior
            {
                StartCoroutine(WaitForNextRandomDestinationInEnclosure());
            }
            else
            {
                if (constructionScript.constructionType == ConstructionScript.ConstructionType.Building)
                {
                    constructionScript.peasantsOnTheirWay--;
                    gameObject.SetActive(false);
                }
                else if (constructionScript.constructionType == ConstructionScript.ConstructionType.Ship)
                {
                    ((ShipScript)constructionScript).AddPeasant(this);
                    Destroy(gameObject);
                }
                constructionScript.UpdateConstructionDetails();
            }
        }
        else
        {
            StartCoroutine(WaitForNextRandomDestination()); //Sinó esperar al següent destí
        }
    }

    public void DoTask()
    {
        hasTaskChanged = false;
        if (task.taskType == TaskScript.TaskType.Item)
        {
            switch (((ItemScript)task).actionType)
            {
                case ItemScript.ActionType.Chop: animator.SetInteger("State", (int)PeasantAction.Chopping); break;
                case ItemScript.ActionType.Dig: animator.SetInteger("State", (int)PeasantAction.Digging); break;
                case ItemScript.ActionType.Pull: animator.SetInteger("State", (int)PeasantAction.Pulling); break;
                case ItemScript.ActionType.Pick: animator.SetInteger("Pick", 0); animator.SetInteger("State", (int)PeasantAction.Gathering); break;
            }
        }
        else if (task.taskType == TaskScript.TaskType.Patch)
        {
            PatchScript patchScript = (PatchScript)task;
            if (patchScript.cropState != PatchScript.CropState.Barren &&
                patchScript.cropType != ((GardenScript)patchScript.taskSourceScript).cropDictionary[patchScript.cell]) //S'ha d'arrencar l'anterior planta
            {
                animator.SetInteger("State", (int)PeasantAction.Pulling);
            }
            else
            {
                switch (patchScript.cropState)
                {
                    case PatchScript.CropState.Barren: animator.SetInteger("State", (int)PeasantAction.Planting); break;
                    case PatchScript.CropState.Planted: case PatchScript.CropState.Grown: animator.SetInteger("State", (int)PeasantAction.Watering); break;
                    case PatchScript.CropState.Blossomed: animator.SetInteger("Pick", 1); animator.SetInteger("State", (int)PeasantAction.Gathering); break;
                }
            }
        }
        else
        {
            ((PairingScript)task).ParticipantHasArrived();
        }
        //transform.LookAt(task.center);
        //StartCoroutine(PointTowardsTaskCenter());
    }

    /*private IEnumerator PointTowardsTaskCenter()
    {
        do
        {
            Vector3 direction = (task.center - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * navMeshAgent.angularSpeed);
            yield return new WaitForSeconds(0.1f);
        }
        while (Vector3.Angle(transform.forward, task.center - transform.position) > 10);
    }*/

    public void FeedAnimals()
    {
        animator.SetInteger("State", (int)PeasantAction.Feeding);
    }

    public void CancelTask()
    {
        if (task != null)
        {
            task.CancelTask();
            hasTaskChanged = true;
        }
    }

    private void TaskProgress()
    {
        if (!hasTaskChanged)
        {
            task.TaskProgress();
        }
        hunger += 0.05f;
        exhaustion += 0.05f;
    }
}
