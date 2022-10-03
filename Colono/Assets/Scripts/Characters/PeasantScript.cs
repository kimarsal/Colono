using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PeasantScript : MonoBehaviour
{
    public enum PeasantType { Adult, Child }
    public PeasantType peasantType;

    public float walkSpeed;
    public float runSpeed;
    private bool isRunning = false;

    public enum PeasantState { Moving, Chopping, Digging, Pulling, Planting, Watering, Gathering, Milking, Dancing };
    protected Animator animator;
    public PeasantState state;
    private NavMeshAgent navMeshAgent;
    public ConstructionScript constructionScript;

    [Header("Appearence")]
    public GameObject head1;
    public GameObject head2;

    public GameObject lowerNaked;
    public GameObject upperNaked;

    public GameObject lowerClothed;
    public GameObject upperClothed;

    public Material material;
    private GameObject head;
    private GameObject lower;
    private GameObject upper;

    public bool isNative = false;

    public TaskScript task;

    private void Start()
    {
        InitializePeasant();
    }

    public void InitializePeasant()
{
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator.SetInteger("State", (int)PeasantState.Moving);

        if (Random.Range(0, 2) == 0)
        {
            head1.SetActive(false);
            head = head2;
        }
        else
        {
            head2.SetActive(false);
            head = head1;
        }

        if (isNative)
        {
            lowerClothed.SetActive(false);
            upperClothed.SetActive(false);
            lower = lowerNaked;
            upper = upperNaked;
        }
        else
        {
            lowerNaked.SetActive(false);
            upperNaked.SetActive(false);
            lower = lowerClothed;
            upper = upperClothed;
        }

        Material newMaterial = new Material(material);
        newMaterial.SetColor("_OTHERCOLOR", Random.ColorHSV()); //Top roba interior dona
        //newMaterial.SetColor("_LEATHER1COLOR", Random.ColorHSV()); //Sabates home i dona
        //newMaterial.SetColor("_LEATHER2COLOR", Random.ColorHSV()); //Bossa, armilla, cordes i sabates
        //newMaterial.SetColor("_LEATHER3COLOR", Random.ColorHSV()); //Cordes dona
        //newMaterial.SetColor("_LEATHER4COLOR", Random.ColorHSV()); //Cordes home
        newMaterial.SetColor("_CLOTH3COLOR", Random.ColorHSV()); //Pantalons, blusa, samarreta
        newMaterial.SetColor("_CLOTH4COLOR", Random.ColorHSV()); //Camisa, faldilla, pantalons + roba interior
        newMaterial.SetColor("_SKINCOLOR", NPCManager.GetRandomSkinColor()); //Color pell
        newMaterial.SetColor("_HAIRCOLOR", NPCManager.GetRandomHairColor()); //Color cabell
        head.GetComponent<SkinnedMeshRenderer>().material = newMaterial;
        lower.GetComponent<SkinnedMeshRenderer>().material = newMaterial;
        upper.GetComponent<SkinnedMeshRenderer>().material = newMaterial;
    }

    protected void CheckIfArrivedAtDestination()
    {
        if (!navMeshAgent.isStopped)
        {
            float d = Vector3.Distance(transform.position, navMeshAgent.destination);
            if (d < 0.5f)
            {
                StopCharacter();
                if(task != null)
                {
                    DoTask();
                }
                else
                {
                    if (constructionScript == null) StartCoroutine(WaitForNextDestination());
                    else if (constructionScript.constructionType != ConstructionScript.ConstructionType.Enclosure) gameObject.SetActive(false);
                }
            }
        }
    }

    protected void StopCharacter()
    {
        navMeshAgent.isStopped = true;
        animator.SetFloat("Speed", 0);
        animator.SetInteger("State", (int)PeasantState.Moving);
    }

    protected IEnumerator WaitForNextDestination()
    {
        yield return new WaitForSeconds(1f);
        SetDestination(NPCManager.GetRandomPoint(transform.position));
    }

    public void SetDestination(Vector3 destination)
    {
        navMeshAgent.destination = destination;
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = isRunning ? runSpeed : walkSpeed;
        animator.SetFloat("Speed", isRunning ? 1 : 0.5f);
        animator.SetInteger("State", (int)PeasantState.Moving);
    }

    public void UpdateTask()
    {
        if (constructionScript != null && constructionScript.constructionType == ConstructionScript.ConstructionType.Ship)
        {
            SetDestination(constructionScript.center.position);
        }
        else
        {
            if (task != null) SetDestination(task.center);
            else
            {
                if (constructionScript != null) SetDestination(constructionScript.center.position);
                else SetDestination(NPCManager.GetRandomPoint(transform.position));
            }
        }
    }

    public void CancelTask()
    {
        task = null;
        StopCharacter();
        StartCoroutine(WaitForNextDestination());
    }

    private void DoTask()
    {
        if(task.taskType == TaskScript.TaskType.Item)
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
    }

    private void TaskProgress()
    {
        if(task.taskType == TaskScript.TaskType.Item)
        {
            ((ItemScript)task).ItemProgress();
        }
    }
}
