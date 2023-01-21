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
        else if (task != null && hunger < 1 && exhaustion < 1) //Si té una tasca encarregada i pot fer-la
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
                constructionScript.peasantsOnTheirWay--;
                constructionScript.UpdateConstructionDetails();

                if (constructionScript.constructionType == ConstructionScript.ConstructionType.Building)
                {
                    gameObject.SetActive(false);
                }
                else if (constructionScript.constructionType == ConstructionScript.ConstructionType.Ship)
                {
                    ((ShipScript)constructionScript).AddPeasant(GetPeasantInfo());
                    Destroy(gameObject);
                }
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
            switch (((ItemScript)task).itemType)
            {
                case ItemScript.ItemType.Chop: animator.SetInteger("State", (int)PeasantAction.Chopping); break;
                case ItemScript.ItemType.Dig: animator.SetInteger("State", (int)PeasantAction.Digging); break;
                case ItemScript.ItemType.Pull: animator.SetInteger("State", (int)PeasantAction.Pulling); break;
                case ItemScript.ItemType.Pick: animator.SetInteger("Pick", 0); animator.SetInteger("State", (int)PeasantAction.Gathering); break;
            }
        }
        else
        {
            PatchScript patchScript = (PatchScript)task;
            if (patchScript.cropState != PatchScript.CropState.Barren &&
                patchScript.cropType != patchScript.gardenScript.cropDictionary[patchScript.cell]) //S'ha d'arrencar l'anterior planta
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
        transform.LookAt(task.center);
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
