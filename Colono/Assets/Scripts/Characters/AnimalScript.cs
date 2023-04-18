using Newtonsoft.Json;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[JsonObject(MemberSerialization.OptIn)]
public class AnimalScript : MonoBehaviour
{
    [JsonProperty] public ResourceScript.AnimalType animalType;
    [JsonProperty] public Vector3 position;
    [JsonProperty] public int orientation;
    
    [JsonProperty] public float age;
    [JsonProperty] public float confortLevel;
    private bool isConfortable;
    public bool isInPlaceForPairing;
    private AnimalScript animalToPairUpWith;
    private bool isDying;

    public PenScript penScript;

    private NavMeshAgent navMeshAgent;
    private Animator animator;

    [JsonIgnore] [SerializeField] private float walkSpeed = 1;
    [JsonIgnore] [SerializeField] private int meatAmount;

    [JsonIgnore] [SerializeField] private float ageSpeed = 0.01f;
    [JsonIgnore] [SerializeField] private float confortSpeed = 0.05f;

    private void Start()
    {
        if (animalType == ResourceScript.AnimalType.Chicken)
        {
            transform.localScale = Vector3.one * 0.1f;
        }
        else if (animalType == ResourceScript.AnimalType.Chick)
        {
            transform.localScale = Vector3.one * 0.05f;
        }

        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        animator.SetFloat("Speed", 0);
        SetDestination(NPCManager.GetRandomPointWithinRange(penScript.minPos, penScript.maxPos));
    }

    public void InitializeAnimal(AnimalScript animalScript)
    {
        age = animalScript.age;
        confortLevel = animalScript.confortLevel;
        isConfortable = animalScript.isConfortable;
    }

    void Update()
    {
        UpdateDetails();

        CheckIfArrivedAtDestination();

        position = transform.position;
        orientation = (int)transform.rotation.eulerAngles.y % 360;
    }

    private void UpdateDetails()
    {
        if (age < 1)
        {
            age += Time.deltaTime * ageSpeed;
        }
        else if ((int)animalType % 2 == 0)
        {
            age = 1;
            penScript.AgeUpAnimal(this);
        }
        else if (!isDying && animalToPairUpWith == null)
        {
            age = 1;
            isDying = true;
            if (isConfortable)
            {
                penScript.confortableAnimals[(int)animalType].Remove(this);
            }
            StartCoroutine(Die());
        }

        if ((int)animalType % 2 == 0) return;

        if (confortLevel < 1)
        {
            confortLevel += Time.deltaTime * confortSpeed * penScript.level;
        }
        else if (!isConfortable)
        {
            isConfortable = true;
            confortLevel = 1;
            penScript.AnimalIsConfortable(this);
        }
    }

    private void CheckIfArrivedAtDestination()
    {
        if (navMeshAgent.isActiveAndEnabled && !navMeshAgent.pathPending
            && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance
            && (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f))
        {
            animator.SetFloat("Speed", 0);
            if (animalToPairUpWith == null)
            {
                StartCoroutine(WaitForNextRandomDestination());
            }
            else
            {
                isInPlaceForPairing = true;
                if (animalToPairUpWith.isInPlaceForPairing)
                {
                    penScript.BreedAnimals(this, animalToPairUpWith);
                }
            }
        }
    }

    public void GetReadyForPairing(Vector3 pairingSpot, AnimalScript animalScript)
    {
        animalToPairUpWith = animalScript;
        SetDestination(pairingSpot);
    }

    public void EndPairing()
    {
        isInPlaceForPairing = false;
        confortLevel = 0;
        isConfortable = false;
        animalToPairUpWith = null;
        WaitForNextRandomDestination();
    }

    public IEnumerator Die()
    {
        penScript.animals[(int)animalType]--;
        penScript.animalList.Remove(this);
        navMeshAgent.isStopped = true;
        animator.SetTrigger("Death");
        yield return new WaitForSeconds(animator.GetCurrentAnimatorClipInfo(0)[0].clip.length);
        penScript.islandScript.AddResource(ResourceScript.ResourceType.Meat, (int)animalType / 2, meatAmount + penScript.level);
        Destroy(gameObject);
    }

    protected void StopCharacter()
    {
        navMeshAgent.isStopped = true;
        animator.SetFloat("Speed", 0);
    }

    protected IEnumerator WaitForNextRandomDestination()
    {
        yield return new WaitForSeconds(1f);
        if (penScript != null)
        {
            SetDestination(NPCManager.GetRandomPointWithinRange(penScript.minPos, penScript.maxPos));
        }
        else
        {
            SetDestination(NPCManager.GetRandomPoint(transform.position));
        }
    }

    public void SetDestination(Vector3 destination)
    {
        if (navMeshAgent == null)
        {
            animator = GetComponent<Animator>();
            navMeshAgent = GetComponent<NavMeshAgent>();
        }
        if (navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.SetDestination(destination);
            navMeshAgent.isStopped = false;
            navMeshAgent.speed = walkSpeed;
            animator.SetFloat("Speed", 0.5f);
        }
    }
}