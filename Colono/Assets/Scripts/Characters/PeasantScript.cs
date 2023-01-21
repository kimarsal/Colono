using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public abstract class PeasantScript : MonoBehaviour
{
    public enum PeasantType { Adult, Child }
    public enum PeasantGender { Male, Female }
    public enum PeasantAction { Moving, Chopping, Digging, Pulling, Planting, Watering, Gathering, Milking, Dancing };

    [Header("Characteristics")]
    public PeasantType peasantType;
    public PeasantGender peasantGender;
    public float walkSpeed;
    public float runSpeed;
    private bool isRunning = false;
    public bool isNative = false;

    [Header("Appearence")]
    public GameObject head1;
    public GameObject head2;
    public GameObject lowerNaked;
    public GameObject upperNaked;
    public GameObject lowerClothed;
    public GameObject upperClothed;
    public Material material;

    private int headType;
    private Color _SKINCOLOR;
    private Color _HAIRCOLOR;
    private Color _CLOTH3COLOR;
    private Color _CLOTH4COLOR;
    private Color _OTHERCOLOR;

    [Header("State")]
    public float age;
    public float hunger;
    public float exhaustion;
    private float ageSpeed = 0.1f;
    private float hungerSpeed = 0.001f;
    private float exhaustionSpeed = 0.001f;

    [Header("Scripts")]
    public NPCManager npcManager;
    public ConstructionScript constructionScript;
    public SpeechBubbleScript speechBubble;
    public PeasantDetailsScript peasantDetailsScript;
    public CabinScript cabin;
    public TavernScript tavern;
    private GameManager gameManager;

    private Outline outline;
    protected Animator animator;
    protected NavMeshAgent navMeshAgent;

    private void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        outline = GetComponent<Outline>();
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator.SetInteger("State", (int)PeasantAction.Moving);
    }

    public void InitializePeasant(PeasantInfo peasantInfo)
    {
        transform.position = peasantInfo.position.UnityVector;
        isNative = peasantInfo.isNative;

        headType = peasantInfo.headType;
        _SKINCOLOR = peasantInfo._SKINCOLOR.UnityColor;
        _HAIRCOLOR = peasantInfo._HAIRCOLOR.UnityColor;
        _CLOTH3COLOR = peasantInfo._CLOTH3COLOR.UnityColor;
        _CLOTH4COLOR = peasantInfo._CLOTH4COLOR.UnityColor;
        _OTHERCOLOR = peasantInfo._OTHERCOLOR.UnityColor;

        GameObject head, lower, upper;
        if (headType == 0)
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

        if (gameManager.CanSelect() && peasantDetailsScript == null)
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
            if (d < 0.5f) //Ha arribat al destí
            {
                StopCharacter();
                ArrivedAtDestination();
            }
        }
    }

    protected void StopCharacter()
    {
        if (navMeshAgent == null)
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

    public void SetDestination(Vector3 destination)
    {
        navMeshAgent.SetDestination(destination);
        if (animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Peasant_Idle"
            || animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Peasant_Walking"
            || animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Peasant_Running") MoveCharacter();
    }

    public abstract void UpdateTask();

    public abstract void ArrivedAtDestination();

    public PeasantInfo GetPeasantInfo()
    {
        PeasantInfo peasantInfo = new PeasantInfo();
        peasantInfo.peasantType = peasantType;
        peasantInfo.peasantGender = peasantGender;
        peasantInfo.isNative = isNative;

        peasantInfo.headType = headType;
        peasantInfo._SKINCOLOR = new SerializableColor(_SKINCOLOR);
        peasantInfo._HAIRCOLOR = new SerializableColor(_HAIRCOLOR);
        peasantInfo._CLOTH3COLOR = new SerializableColor(_CLOTH3COLOR);
        peasantInfo._CLOTH4COLOR = new SerializableColor(_CLOTH4COLOR);
        peasantInfo._OTHERCOLOR = new SerializableColor(_OTHERCOLOR);

        peasantInfo.position = new SerializableVector3(transform.position);
        peasantInfo.orientation = Mathf.RoundToInt(transform.rotation.eulerAngles.y);

        peasantInfo.age = age;
        peasantInfo.hunger = hunger;
        peasantInfo.exhaustion = exhaustion;

        return peasantInfo;
    }
}

[System.Serializable]
public class PeasantInfo
{
    public PeasantScript.PeasantType peasantType;
    public PeasantScript.PeasantGender peasantGender;
    public bool isNative;

    public int headType;
    public SerializableColor _SKINCOLOR;
    public SerializableColor _HAIRCOLOR;
    public SerializableColor _CLOTH3COLOR;
    public SerializableColor _CLOTH4COLOR;
    public SerializableColor _OTHERCOLOR;

    public SerializableVector3 position;
    public int orientation;

    public float age;
    public float hunger;
    public float exhaustion;
}
