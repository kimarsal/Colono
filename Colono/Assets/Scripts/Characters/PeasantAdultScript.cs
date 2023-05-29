using Newtonsoft.Json;
using System.Collections;
using UnityEngine;

[JsonObject(MemberSerialization.OptIn)]
public class PeasantAdultScript : PeasantScript
{
    [Header("Tools")]
    public GameObject axe;
    public GameObject shovel;
    public GameObject basket;
    public GameObject wateringCan;

    [JsonProperty] public int childrenAmount = 0;

    public TaskScript task;
    [JsonProperty] [JsonConverter(typeof(VectorConverter))] public Vector2 taskCell;
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

    public override void InitializePeasant(PeasantScript peasantInfo = null)
    {
        base.InitializePeasant(peasantInfo);

        if(peasantInfo != null)
        {
            PeasantAdultScript peasantAdultInfo = (PeasantAdultScript)peasantInfo;

            if(peasantAdultInfo.taskCell != Vector2.zero)
            {
                if(constructionScript == null)
                {
                    AssignTask(islandScript.itemDictionary[peasantAdultInfo.taskCell]); 
                }
                else
                {
                    AssignTask(((GardenScript)constructionScript).patchDictionary[peasantAdultInfo.taskCell]);
                }
            }
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

    protected override void ArrivedAtDestination()
    {
        if (age >= lifeExpectancy)
        {
            StartCoroutine(Die());
            return;
        }

        if (tavern != null) //Si ha anat a menjar
        {
            tavern.PeasantHasArrived(this);
            peasantRowScript?.PeasantArrivedToBuilding();
        }
        else if (cabin != null) //Si ha anat a dormir
        {
            cabin.PeasantHasArrived(this);
            peasantRowScript?.PeasantArrivedToBuilding();
        }
        else if (task != null) //Si té una tasca encarregada
        {
            DoTask();
        }
        else if (constructionScript != null) //Si té una construcció com a destí
        {
            if (constructionScript.constructionType == ConstructionScript.ConstructionType.Enclosure) //Si la construcció és exterior
            {
                StartCoroutine(WaitForNextRandomDestinationInEnclosure());
            }
            else
            {
                constructionScript.PeasantHasArrived(this);
                peasantRowScript?.PeasantArrivedToBuilding(constructionScript.constructionType == ConstructionScript.ConstructionType.Ship);
            }
        }
        else
        {
            StartCoroutine(WaitForNextRandomDestination()); //Sinó esperar al següent destí
        }
    }

    public void DoTask()
    {
        task.isBeingTakenCareOf = true;
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
    }

    private void TaskProgress()
    {
        if(task != null)
        {
            task.TaskProgress();
            hunger += 0.05f;
            exhaustion += 0.05f;
        }
    }

    public void CancelTask()
    {
        if (task != null)
        {
            animator.SetTrigger("CancelTask");
            task.CancelTask();
            task = null;
        }
    }

    private IEnumerator Die()
    {
        isDying = true;
        peasantRowScript?.PeasantDies();
        UnlinkFromTavernAndCabin();
        if (constructionScript != null) constructionScript.peasantList.Remove(this);
        islandScript.peasantList.Remove(this);

        navMeshAgent.isStopped = true;
        animator.SetFloat("Speed", 0);
        animator.SetInteger("State", (int)PeasantAction.Dying);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorClipInfo(0)[0].clip.length);
        Destroy(gameObject);
    }
}
