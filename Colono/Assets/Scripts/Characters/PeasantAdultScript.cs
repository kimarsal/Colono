using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeasantAdultScript : PeasantScript
{
    public TaskScript task;

    [Header("Tools")]
    public GameObject axe;
    public GameObject shovel;
    public GameObject basket;
    public GameObject wateringCan;

    private ItemScript.ResourceType resourceType;
    private int resourceAmount;

    private PatchScript.CropType cropType;

    void Update()
    {
        CheckIfArrivedAtDestination();
        /*if (Input.GetKeyDown(KeyCode.T))
        {
            StopCharacter();
            animator.SetInteger("State", (int)PeasantState.Chopping);
        }
        else if (Input.GetKeyDown(KeyCode.Y))
        {
            StopCharacter();
            animator.SetInteger("State", (int)PeasantState.Digging);
        }
        else if (Input.GetKeyDown(KeyCode.U))
        {
            StopCharacter();
            animator.SetInteger("State", (int)PeasantState.Pulling);
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            StopCharacter();
            animator.SetInteger("Pick", 2);
            animator.SetInteger("State", (int)PeasantState.Gathering);
        }
        else if (Input.GetKeyDown(KeyCode.O))
        {
            StopCharacter();
            animator.SetInteger("Pick", 2);
            animator.SetInteger("State", (int)PeasantState.Watering);
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            StopCharacter();
            StartCoroutine(WaitForNextDestination());
        }*/
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

    public void CancelTask()
    {
        task = null;
        StopCharacter();
        StartCoroutine(WaitForNextRandomDestination());
    }

    public void DoTask()
    {
        transform.LookAt(task.center);
        if (task.taskType == TaskScript.TaskType.Item)
        {
            switch (((ItemScript)task).itemType)
            {
                case ItemScript.ItemType.Chop: animator.SetInteger("State", (int)PeasantState.Chopping); break;
                case ItemScript.ItemType.Dig: animator.SetInteger("State", (int)PeasantState.Digging); break;
                case ItemScript.ItemType.Pull: animator.SetInteger("State", (int)PeasantState.Pulling); break;
                case ItemScript.ItemType.Pick: animator.SetInteger("Pick", 0); animator.SetInteger("State", (int)PeasantState.Gathering); break;
            }
        }
        else
        {
            switch (((PatchScript)task).cropState)
            {
                case PatchScript.CropState.Barren: animator.SetInteger("State", (int)PeasantState.Planting); break;
                case PatchScript.CropState.Planted: case PatchScript.CropState.Grown: animator.SetInteger("State", (int)PeasantState.Watering); break;
                case PatchScript.CropState.Blossomed: animator.SetInteger("Pick", 1); animator.SetInteger("State", (int)PeasantState.Gathering); break;
            }
        }
        //StartCoroutine(PointTowardsTaskCenter());
    }

    private IEnumerator PointTowardsTaskCenter()
    {
        do
        {
            Vector3 direction = (task.center - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * navMeshAgent.angularSpeed);
            yield return new WaitForSeconds(0.1f);
        }
        while (Vector3.Angle(transform.forward, task.center - transform.position) > 10);
    }

    private void TaskProgress()
    {
        task.TaskProgress();
    }

    public void CompleteItemRemoval(ItemScript.ResourceType rt, int ra)
    {
        resourceType = rt;
        resourceAmount = ra;
        speechBubble.gameObject.SetActive(true);
        speechBubble.DisplayResource(resourceType);
        CancelTask();
    }

    public void CompleteCropHarvesting(PatchScript.CropType ct)
    {
        cropType = ct;
        speechBubble.gameObject.SetActive(true);
        speechBubble.DisplayCrop(cropType);
    }
}
