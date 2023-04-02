using Newtonsoft.Json;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AnimalScript : MonoBehaviour
{
    public ResourceScript.AnimalType animalType;
    public Vector3 position;
    public int orientation;
    [JsonIgnore] public PenScript penScript;
    [JsonIgnore] private AnimalScript animalToPairUpWith;

    [JsonIgnore] private NavMeshAgent navMeshAgent;
    [JsonIgnore] private Animator animator;
    [JsonIgnore] [SerializeField] private float walkSpeed = 1;
    [JsonIgnore] [SerializeField] private int meatAmount;

    [JsonIgnore] private float ageSpeed = 0.01f;
    public float age;

    [JsonIgnore] private float confortSpeed = 0.05f;
    public float confortLevel;
    private bool isConfortable;
    public bool isInPlaceForPairing;

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

        if ((int)animalType % 2 == 1 && confortLevel < 1)
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

    public void GetReadyForPairing(AnimalScript animalScript)
    {
        animalToPairUpWith = animalScript;
        SetDestination(penScript.entry.position);
    }

    public void EndPairing()
    {
        confortLevel = 0;
        isConfortable = false;
        animalToPairUpWith = null;
        WaitForNextRandomDestination();
    }

    public IEnumerator Die()
    {
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
        navMeshAgent.SetDestination(destination);
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = walkSpeed;
        animator.SetFloat("Speed", 0.5f);
    }
}