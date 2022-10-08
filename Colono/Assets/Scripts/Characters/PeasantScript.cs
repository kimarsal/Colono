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
    public ConstructionScript constructionScript;
    public SpeechBubbleScript speechBubble;

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

        tag = "NPC";
    }

    protected void CheckIfArrivedAtDestination()
    {
        if (!navMeshAgent.isStopped)
        {
            float d = Vector3.Distance(transform.position, navMeshAgent.destination);
            if (d < 0.8f) //Ha arribat al destí
            {
                StopCharacter();
                if (peasantType == PeasantType.Adult)
                {
                    PeasantAdultScript peasantAdultScript = (PeasantAdultScript)this;
                    if (constructionScript == null) //Si no té destí
                    {
                        if (peasantAdultScript.task != null) peasantAdultScript.DoTask(); //Si té item pendent de treure
                        else StartCoroutine(WaitForNextRandomDestination()); //Sinó esperar al següent destí
                    }
                    else
                    {
                        if(constructionScript.constructionType == ConstructionScript.ConstructionType.Enclosure) //Si el destí és exterior
                        {
                            peasantAdultScript.DoTask();
                        }
                        else
                        {
                            gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    if (constructionScript == null) //Si no té destí esperar al següent
                    {
                        StartCoroutine(WaitForNextRandomDestination());
                    }
                    else if (constructionScript.constructionType != ConstructionScript.ConstructionType.Enclosure) //Si té destí i aquest és interior desaparèixer
                    {
                        gameObject.SetActive(false);
                    }
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

    public void MoveCharacter()
    {
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = isRunning ? runSpeed : walkSpeed;
        animator.SetFloat("Speed", isRunning ? 1 : 0.5f);
        animator.SetInteger("State", (int)PeasantState.Moving);
    }

    protected IEnumerator WaitForNextRandomDestination()
    {
        yield return new WaitForSeconds(1f);
        SetDestination(NPCManager.GetRandomPoint(transform.position));
    }

    public void SetDestination(Vector3 destination)
    {
        navMeshAgent.destination = destination;
        if (animator.GetInteger("State") == (int)PeasantState.Moving) MoveCharacter();
    }

    public void UpdateTask()
    {
        if (constructionScript != null && constructionScript.constructionType == ConstructionScript.ConstructionType.Ship)
        {
            SetDestination(constructionScript.center.position);
        }
        else
        {
            if (peasantType == PeasantType.Adult && ((PeasantAdultScript)this).task != null)
            {
                SetDestination(((PeasantAdultScript)this).task.center);
            }
            else
            {
                if (constructionScript != null) SetDestination(constructionScript.center.position);
                else SetDestination(NPCManager.GetRandomPoint(transform.position));
            }
        }
    }
}
