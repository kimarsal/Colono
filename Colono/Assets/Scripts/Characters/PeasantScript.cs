using Newtonsoft.Json;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public abstract class PeasantScript : MonoBehaviour
{
    public enum PeasantType { Adult, Child }
    public enum PeasantGender { Male, Female }
    public enum PeasantAction { Moving, Chopping, Digging, Pulling, Planting, Watering, Gathering, Feeding, Milking, Dancing };

    [Header("Characteristics")]
    public PeasantType peasantType;
    public PeasantGender peasantGender;
    public Vector3 position;
    public int orientation;
    [JsonIgnore] public float walkSpeed;
    [JsonIgnore] public float runSpeed;
    private bool isRunning;
    public bool isNative;

    [Header("Appearence")]
    [JsonIgnore] [SerializeField] private GameObject head1;
    [JsonIgnore] [SerializeField] private GameObject head2;
    [JsonIgnore] [SerializeField] private GameObject lowerNaked;
    [JsonIgnore] [SerializeField] private GameObject upperNaked;
    [JsonIgnore] [SerializeField] private GameObject lowerClothed;
    [JsonIgnore] [SerializeField] private GameObject upperClothed;
    [JsonIgnore] [SerializeField] private Material material;

    public int headType;
    public Color _SKINCOLOR;
    public Color _HAIRCOLOR;
    public Color _CLOTH3COLOR;
    public Color _CLOTH4COLOR;
    public Color _OTHERCOLOR;

    [Header("State")]
    public float age;
    public float hunger;
    public float exhaustion;
    public bool isInBuilding;
    [JsonIgnore] private float ageSpeed = 0.1f;
    [JsonIgnore] private float hungerSpeed = 0.01f;
    [JsonIgnore] private float exhaustionSpeed = 0.01f;

    [Header("Scripts")]
    [JsonIgnore] public IslandScript islandScript;
    [JsonIgnore] public ConstructionScript constructionScript;
    [JsonIgnore] public SpeechBubbleScript speechBubble;
    [JsonIgnore] public PeasantDetailsScript peasantDetailsScript;
    [JsonIgnore] public CabinScript cabin;
    [JsonIgnore] public TavernScript tavern;

    [JsonIgnore] public Outline outline;
    [JsonIgnore] protected Animator animator;
    [JsonIgnore] public NavMeshAgent navMeshAgent;

    private void Start()
    {
        outline = GetComponent<Outline>();
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator.SetInteger("State", (int)PeasantAction.Moving);
        animator.SetFloat("Speed", 0);

        transform.localScale = Vector3.one * 0.45f;

        SetAppearence();
        UpdateTask();
    }

    public void InitializePeasant(PeasantScript peasantScript)
    {
        isRunning = peasantScript.isRunning;
        isNative = peasantScript.isNative;

        headType = peasantScript.headType;
        _SKINCOLOR = peasantScript._SKINCOLOR;
        _HAIRCOLOR = peasantScript._HAIRCOLOR;
        _CLOTH3COLOR = peasantScript._CLOTH3COLOR;
        _CLOTH4COLOR = peasantScript._CLOTH4COLOR;
        _OTHERCOLOR = peasantScript._OTHERCOLOR;

        age = peasantScript.age;
        hunger = peasantScript.hunger;
        exhaustion = peasantScript.exhaustion;
    }

    private void SetAppearence()
    {
        GameObject head, lower, upper;
        if (headType == 0)
        {
            head2.SetActive(false);
            head = head1;
        }
        else
        {
            head1.SetActive(false);
            head = head2;
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
        newMaterial.SetColor("_SKINCOLOR", _SKINCOLOR); //Color pell
        newMaterial.SetColor("_HAIRCOLOR", _HAIRCOLOR); //Color cabell
        newMaterial.SetColor("_CLOTH3COLOR", _CLOTH3COLOR); //Pantalons, blusa, samarreta
        newMaterial.SetColor("_CLOTH4COLOR", _CLOTH4COLOR); //Camisa, faldilla, pantalons + roba interior
        newMaterial.SetColor("_OTHERCOLOR", _OTHERCOLOR); //Top roba interior dona
        //newMaterial.SetColor("_LEATHER1COLOR", Random.ColorHSV()); //Sabates home i dona
        //newMaterial.SetColor("_LEATHER2COLOR", Random.ColorHSV()); //Bossa, armilla, cordes i sabates
        //newMaterial.SetColor("_LEATHER3COLOR", Random.ColorHSV()); //Cordes dona
        //newMaterial.SetColor("_LEATHER4COLOR", Random.ColorHSV()); //Cordes home
        head.GetComponent<SkinnedMeshRenderer>().material = newMaterial;
        lower.GetComponent<SkinnedMeshRenderer>().material = newMaterial;
        upper.GetComponent<SkinnedMeshRenderer>().material = newMaterial;
    }

    public GameObject GetHead()
    {
        return headType == 0 ? head1 : head2;
    }

    private void Update()
    {
        UpdateDetails();

        CheckIfArrivedAtDestination();

        position = transform.position;
        orientation = (int)transform.rotation.eulerAngles.y % 360;
    }

    protected void UpdateDetails()
    {
        age += Time.deltaTime * ageSpeed;

        if (hunger < 1)
        {
            hunger += Time.deltaTime * hungerSpeed;
        }
        else if (tavern == null)
        {
            hunger = 1;
            tavern = (TavernScript)islandScript.GetAvailableBuilding(BuildingScript.BuildingType.Tavern, this);
            if (tavern != null)
            {
                tavern.AddPeasant(this);
                UpdateTask();
            }
        }

        if (exhaustion < 1)
        {
            exhaustion += Time.deltaTime * exhaustionSpeed;
        }
        else if (cabin == null)
        {
            exhaustion = 1;
            cabin = (CabinScript)islandScript.GetAvailableBuilding(BuildingScript.BuildingType.Cabin, this);
            if (cabin != null)
            {
                cabin.AddPeasant(this);
                UpdateTask();
            }
        }
    }

    protected void CheckIfArrivedAtDestination()
    {
        if (!isInBuilding && !navMeshAgent.pathPending
            && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance
            && (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f))
        {
            animator.SetFloat("Speed", 0);
            ArrivedAtDestination();
        }
    }

    protected void StopCharacter()
    {
        if(navMeshAgent == null)
        {
            animator = GetComponent<Animator>();
            navMeshAgent = GetComponent<NavMeshAgent>();
        }
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

    protected void SetDestination(Vector3 destination)
    {
        navMeshAgent.SetDestination(destination);
        if (animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Peasant_Idle"
            || animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Peasant_Walking"
            || animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Peasant_Running") MoveCharacter();
    }

    public abstract void UpdateTask();

    protected abstract void ArrivedAtDestination();
}
