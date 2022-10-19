using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PeasantAdultScript : PeasantScript
{
    public TaskScript task;

    [Header("Tools")]
    public GameObject axe;
    public GameObject shovel;
    public GameObject basket;
    public GameObject wateringCan;

    [Header("Resources")]
    public int capacity;
    public int usage;
    public int[] materials = new int[Enum.GetValues(typeof(ResourceScript.MaterialType)).Length];
    public int[] crops = new int[Enum.GetValues(typeof(ResourceScript.CropType)).Length];

    public enum PeasantState { HangingOut, Working, GoingToEat, GoingToSleep, CarryingToWarehouse, CarryingToShip }
    public PeasantState peasantState;
    public WarehouseScript warehouseScript;

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
        if (constructionScript != null)
        {
            SetDestination(NPCManager.GetRandomPointWithinRange(((EnclosureScript)constructionScript).minPos, ((EnclosureScript)constructionScript).maxPos));
        }
        else SetDestination(NPCManager.GetRandomPoint(transform.position));
    }

    public override void UpdateTask()
    {
        animator.SetInteger("State", (int)PeasantAction.Moving);
        if (peasantState == PeasantState.CarryingToWarehouse) //Si està portant recursos al magatzem
        {
            SetDestination(warehouseScript.entry.position);
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
        else SetDestination(NPCManager.GetRandomPoint(transform.position));
    }

    public override void ArrivedAtDestination()
    {
        if (peasantState == PeasantState.CarryingToWarehouse) //Si està portant recursos al magatzem
        {
            AddToWarehouse();
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
                if (constructionScript.constructionType == ConstructionScript.ConstructionType.Ship)
                {
                    transform.parent = ((ShipScript)constructionScript).npcs.transform;
                }

                constructionScript.peasantsOnTheirWay--;
                constructionScript.UpdateConstructionDetails();
                gameObject.SetActive(false);
            }
        }
        else
        {
            StartCoroutine(WaitForNextRandomDestination()); //Sinó esperar al següent destí
        }
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
                case ItemScript.ItemType.Chop: animator.SetInteger("State", (int)PeasantAction.Chopping); break;
                case ItemScript.ItemType.Dig: animator.SetInteger("State", (int)PeasantAction.Digging); break;
                case ItemScript.ItemType.Pull: animator.SetInteger("State", (int)PeasantAction.Pulling); break;
                case ItemScript.ItemType.Pick: animator.SetInteger("Pick", 0); animator.SetInteger("State", (int)PeasantAction.Gathering); break;
            }
        }
        else
        {
            switch (((PatchScript)task).cropState)
            {
                case PatchScript.CropState.Barren: animator.SetInteger("State", (int)PeasantAction.Planting); break;
                case PatchScript.CropState.Planted: case PatchScript.CropState.Grown: animator.SetInteger("State", (int)PeasantAction.Watering); break;
                case PatchScript.CropState.Blossomed: animator.SetInteger("Pick", 1); animator.SetInteger("State", (int)PeasantAction.Gathering); break;
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
        hunger += 0.05f;
        exhaustion += 0.05f;
    }

    public void GatherMaterialFromItem(ResourceScript.MaterialType materialType)
    {
        materials[(int)materialType]++;
        usage++;
        /*speechBubble.gameObject.SetActive(true);
        speechBubble.DisplayMaterial(materialType);*/

        if (usage == capacity)
        {
            warehouseScript = (WarehouseScript)npcManager.islandScript.GetAvailableBuilding(BuildingScript.BuildingType.Warehouse);
            if(warehouseScript != null)
            {
                peasantState = PeasantState.CarryingToWarehouse;
            }
            CancelTask();
        }
    }

    public void CompleteCropHarvesting(ResourceScript.CropType cropType, int cropAmount)
    {
        if (capacity - usage < cropAmount) cropAmount = capacity - usage;
        crops[(int)cropType] += cropAmount;
        usage += cropAmount;
        /*speechBubble.gameObject.SetActive(true);
        speechBubble.DisplayCrop(cropType);*/

        if(usage == capacity)
        {
            peasantState = PeasantState.CarryingToWarehouse;
        }

        CancelTask();
    }

    public void AddToWarehouse()
    {
        bool warehouseIsFull = false;
        int i = 0;
        while(i < materials.Length && !warehouseIsFull)
        {
            int materialAmount = warehouseScript.AddMaterials(i, materials[i]);
            if (materialAmount < materials[i]) warehouseIsFull = true;
            materials[i] -= materialAmount;
            usage -= materialAmount;
            i++;
        }
        i = 0;
        while (i < crops.Length && !warehouseIsFull)
        {
            int cropAmount = warehouseScript.AddCrops(i, crops[i]);
            if (cropAmount < crops[i]) warehouseIsFull = true;
            crops[i] -= cropAmount;
            usage -= cropAmount;
            i++;
        }
        peasantState = (constructionScript == null ? PeasantState.HangingOut : PeasantState.Working);
        UpdateTask();

    }
}
