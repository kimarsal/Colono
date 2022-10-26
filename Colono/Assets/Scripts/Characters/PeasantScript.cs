using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public abstract class PeasantScript : MonoBehaviour
{
    [Header("Characteristics")]
    public PeasantType peasantType;
    public enum PeasantType { Adult, Child }
    public float walkSpeed;
    public float runSpeed;
    private bool isRunning = false;

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

    [Header("State")]
    public float age;
    public float hunger;
    public float exhaustion;
    private float ageSpeed = 0.1f;
    private float hungerSpeed = 0.05f;
    private float exhaustionSpeed = 0.01f;

    public PeasantAction action;
    public enum PeasantAction { Moving, Chopping, Digging, Pulling, Planting, Watering, Gathering, Milking, Dancing };
    protected Animator animator;
    protected NavMeshAgent navMeshAgent;

    [Header("Scripts")]
    public GameManager gameManager;
    public NPCManager npcManager;
    public ConstructionScript constructionScript;
    public SpeechBubbleScript speechBubble;
    public PeasantDetailsScript peasantDetailsScript;
    private Outline outline;

    public CabinScript cabin;
    public TavernScript tavern;

    public void InitializePeasant()
    {
        outline = GetComponent<Outline>();
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator.SetInteger("State", (int)PeasantAction.Moving);

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

    private void Update()
    {
        age += Time.deltaTime * ageSpeed;

        peasantDetailsScript?.UpdateDetails();

        if (hunger < 1)
        {
            hunger += Time.deltaTime * hungerSpeed;
        }
        else if (tavern == null)
        {
            hunger = 1;
            tavern = (TavernScript)npcManager.islandScript.GetAvailableBuilding(BuildingScript.BuildingType.Tavern);
            if(tavern != null) UpdateTask();
        }

        if (exhaustion < 1)
        {
            exhaustion += Time.deltaTime * exhaustionSpeed;
        }
        else if (cabin == null)
        {
            exhaustion = 1;
            cabin = (CabinScript)npcManager.islandScript.GetAvailableBuilding(BuildingScript.BuildingType.Cabin);
            if(cabin != null) UpdateTask();
        }

        if (gameManager.isInIsland && gameManager.buttonState != GameManager.ButtonState.PopUp && peasantDetailsScript == null)
        {
            RaycastHit raycastHit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out raycastHit, 100f))
            {
                if (raycastHit.transform.gameObject.GetComponent<PeasantScript>() == this)
                {
                    outline.enabled = true;
                    if (Input.GetMouseButtonDown(0))
                    {
                        gameManager.SelectPeasant(this);
                    }
                }
                else
                {
                    outline.enabled = false;
                }
            }
        }

        CheckIfArrivedAtDestination();
    }

    protected void CheckIfArrivedAtDestination()
    {
        if (!navMeshAgent.isStopped)
        {
            float d = Vector3.Distance(transform.position, navMeshAgent.destination);
            if (d < 0.8f) //Ha arribat al destí
            {
                StopCharacter();
                ArrivedAtDestination();
            }
        }
    }

    protected void StopCharacter()
    {
        navMeshAgent.isStopped = true;
        animator.SetFloat("Speed", 0);
        animator.SetInteger("State", (int)PeasantAction.Moving);
    }

    public void MoveCharacter()
    {
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = isRunning ? runSpeed : walkSpeed;
        animator.SetFloat("Speed", isRunning ? 1 : 0.5f);
        animator.SetInteger("State", (int)PeasantAction.Moving);
    }

    protected IEnumerator WaitForNextRandomDestination()
    {
        yield return new WaitForSeconds(1f);
        SetDestination(NPCManager.GetRandomPoint(transform.position));
    }

    public void SetDestination(Vector3 destination)
    {
        navMeshAgent.destination = destination;
        if (animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Peasant_Idle"
            || animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Peasant_Walking"
            || animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Peasant_Running") MoveCharacter();
    }

    public abstract void UpdateTask();

    public abstract void ArrivedAtDestination();

}
