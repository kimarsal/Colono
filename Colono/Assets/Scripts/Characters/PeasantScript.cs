using Newtonsoft.Json;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[JsonObject(MemberSerialization.OptIn)]
public abstract class PeasantScript : MonoBehaviour
{
    public enum PeasantType { Adult, Child }
    public enum PeasantGender { Male, Female }
    public enum PeasantAction { Moving, Chopping, Digging, Pulling, Planting, Watering, Gathering, Dancing, Dying };

    public static int lifeExpectancy = 60;
    [JsonProperty] [JsonConverter(typeof(VectorConverter))] public Vector3 position;
    [JsonProperty] public int orientation;

    [Header("Characteristics")]
    [JsonProperty] public PeasantType peasantType;
    [JsonProperty] public PeasantGender peasantGender;
    [JsonProperty] private bool isRunning;
    [JsonProperty] public bool isNative;
    public float walkSpeed;
    public float runSpeed;

    [Header("Appearence")]
    [JsonIgnore] [SerializeField] private GameObject head1;
    [JsonIgnore] [SerializeField] private GameObject head2;
    [JsonIgnore] [SerializeField] private GameObject lowerNaked;
    [JsonIgnore] [SerializeField] private GameObject upperNaked;
    [JsonIgnore] [SerializeField] private GameObject lowerClothed;
    [JsonIgnore] [SerializeField] private GameObject upperClothed;
    [JsonIgnore] [SerializeField] private Material material;

    [JsonProperty] public int headType;
    [JsonProperty] [JsonConverter(typeof(ColorHandler))] public Color _SKINCOLOR;
    [JsonProperty] [JsonConverter(typeof(ColorHandler))] public Color _HAIRCOLOR;
    [JsonProperty] [JsonConverter(typeof(ColorHandler))] public Color _CLOTH3COLOR;
    [JsonProperty] [JsonConverter(typeof(ColorHandler))] public Color _CLOTH4COLOR;
    [JsonProperty] [JsonConverter(typeof(ColorHandler))] public Color _OTHERCOLOR;

    [Header("State")]
    [JsonProperty] public float age;
    [JsonProperty] public float hunger;
    [JsonProperty] public float exhaustion;
    [JsonProperty] public bool isInBuilding;
    [JsonProperty] public bool isInConstruction;
    protected bool isDying;
    private float ageSpeed = 0.1f;
    private float hungerSpeed = 0.005f;
    private float exhaustionSpeed = 0.005f;

    [Header("Scripts")]
    public IslandScript islandScript;
    public ConstructionScript constructionScript;
    public TavernScript tavern;
    [JsonProperty] public int tavernIndex;
    public CabinScript cabin;
    [JsonProperty] public int cabinIndex;
    public SpeechBubbleScript speechBubble;
    public Outline outline;
    protected Animator animator;
    public NavMeshAgent navMeshAgent;
    public PeasantRowScript peasantRowScript;

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

    public virtual void InitializePeasant(PeasantScript peasantInfo = null)
    {
        if (peasantInfo is not null)
        {
            isRunning = peasantInfo.isRunning;
            isNative = peasantInfo.isNative;

            headType = peasantInfo.headType;
            _SKINCOLOR = peasantInfo._SKINCOLOR;
            _HAIRCOLOR = peasantInfo._HAIRCOLOR;
            _CLOTH3COLOR = peasantInfo._CLOTH3COLOR;
            _CLOTH4COLOR = peasantInfo._CLOTH4COLOR;
            _OTHERCOLOR = peasantInfo._OTHERCOLOR;

            age = peasantInfo.age;
            hunger = peasantInfo.hunger;
            exhaustion = peasantInfo.exhaustion;

            if (peasantInfo.tavernIndex != -1)
            {
                tavern = (TavernScript)islandScript.constructionList[peasantInfo.tavernIndex];
            }
            if (peasantInfo.cabinIndex != -1)
            {
                cabin = (CabinScript)islandScript.constructionList[peasantInfo.cabinIndex];
            }
        }
        else
        {
            isNative = false;

            headType = Random.Range(0, 2);
            _SKINCOLOR = ResourceScript.Instance.GetRandomSkinColor();
            _HAIRCOLOR = ResourceScript.Instance.GetRandomHairColor();
            _CLOTH3COLOR = Random.ColorHSV();
            _CLOTH4COLOR = Random.ColorHSV();
            _OTHERCOLOR = Random.ColorHSV();
        }
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
        if (isDying) return;

        UpdateDetails();

        CheckIfArrivedAtDestination();

        position = transform.position;
        orientation = (int)transform.rotation.eulerAngles.y % 360;
    }

    protected void UpdateDetails()
    {
        if (hunger < 1)
        {
            hunger += Time.deltaTime * hungerSpeed;
        }
        else if (tavern == null)
        {
            hunger = 1;
            speechBubble.IsHungry();
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
            speechBubble.IsTired();
            cabin = (CabinScript)islandScript.GetAvailableBuilding(BuildingScript.BuildingType.Cabin, this);
            if (cabin != null)
            {
                cabin.AddPeasant(this);
                UpdateTask();
            }
        }

        age += Time.deltaTime * ageSpeed;
        
        peasantRowScript?.UpdatePeasantDetails();
    }

    protected void CheckIfArrivedAtDestination()
    {
        if (!isInBuilding && navMeshAgent.isActiveAndEnabled
            && navMeshAgent.isOnNavMesh && !navMeshAgent.pathPending
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
        if(navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.SetDestination(destination);
            if (animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Peasant_Idle"
                || animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Peasant_Walking"
                || animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Peasant_Running") MoveCharacter();
        }
    }

    public abstract void UpdateTask();

    protected abstract void ArrivedAtDestination();
}
