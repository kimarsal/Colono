using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PeasantScript : MonoBehaviour
{
    public NPCManager npcManager;
    public float walkSpeed;
    public float runSpeed;
    public float rotateSpeed;
    private bool isRunning = false;

    public enum PeasantState { Moving, Chopping, Planting, Gathering, Dancing };
    protected Animator animator;
    public PeasantState state;
    private NavMeshAgent navMeshAgent;
    public BuildingScript settlementScript;
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

    public bool isNaked = false;

    void Start()
    {
        SetAppearence();
        //StartCoroutine(WaitForNextDestination());
    }

    public void SetAppearence()
    {
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

        if (isNaked)
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
        if (!navMeshAgent.isStopped && Vector3.Distance(transform.position, navMeshAgent.destination) < 0.01f)
        {
            StopCharacter();
            if (constructionScript == null) gameObject.SetActive(false);
        }
    }

    protected void StopCharacter()
    {
        navMeshAgent.isStopped = true;
        animator.SetFloat("Speed", 0);
    }

    protected IEnumerator WaitForNextDestination()
    {
        yield return new WaitForSeconds(1f);
        SetDestination(npcManager.GetRandomPoint(transform.position));
        //SetDestination(new Vector3(Random.Range(NPCManager.minX, NPCManager.maxX), 0f, Random.Range(NPCManager.minZ, NPCManager.maxZ)));
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
        if(constructionScript == null)
        {
            SetDestination(settlementScript.center.position);
        }
        else
        {
            animator = GetComponent<Animator>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            animator.SetInteger("State", (int)PeasantState.Moving);

            SetDestination(constructionScript.center.position);
        }
    }
}
