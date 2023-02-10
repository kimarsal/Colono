using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class AnimalScript : MonoBehaviour
{
    public ResourceScript.AnimalType animalType;
    public Vector3 position;
    public int orientation;
    [JsonIgnore] public PenScript penScript;
    [JsonIgnore] private PairingScript pairingScript;

    [JsonIgnore] private NavMeshAgent navMeshAgent;
    [JsonIgnore] private Animator animator;
    [JsonIgnore] public float walkSpeed = 1;
    [JsonIgnore] public float runSpeed = 2;
    public bool isRunning;

    [JsonIgnore] public float ageSpeed = 0.05f;
    public float age;

    [JsonIgnore] public float lustSpeed = 0.05f;
    public float timeSinceLastPairing;
    private bool isReadyForPairing;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        animator.SetFloat("Speed", 0);
        SetDestination(NPCManager.GetRandomPointWithinRange(penScript.minPos, penScript.maxPos));
    }

    public void InitializeAnimal(AnimalScript animalScript)
    {
        isRunning = animalScript.isRunning;
        age = animalScript.age;
        timeSinceLastPairing = animalScript.timeSinceLastPairing;
        isReadyForPairing = animalScript.isReadyForPairing;
    }

    void Update()
    {
        if (!navMeshAgent.pathPending
            && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance
            && (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f))
        {
            animator.SetFloat("Speed", 0);
            if (pairingScript != null)
            {
                pairingScript.ParticipantHasArrived();
            }
            else
            {
                StartCoroutine(WaitForNextRandomDestination());
            }
        }

        if (age < 1)
        {
            age += Time.deltaTime * ageSpeed;
        }
        else if ((int)animalType % 2 == 0)
        {
            age = 1;
            penScript.AgeUpAnimal(this);
        }

        if (timeSinceLastPairing < 1)
        {
            timeSinceLastPairing += Time.deltaTime * lustSpeed;
        }
        else if(!isReadyForPairing)
        {
            isReadyForPairing = true;
            timeSinceLastPairing = 1;
            penScript.AnimalIsReadyForPairing(this);
        }

        /*if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(Die());
        }*/
        position = transform.position;
        orientation = (int)transform.rotation.y % 360;
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
        navMeshAgent.SetDestination(destination);
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = isRunning ? runSpeed : walkSpeed;
        animator.SetFloat("Speed", isRunning ? 1 : 0.5f);
    }

    public void GetReadyForPairing(PairingScript pairingScript)
    {
        this.pairingScript = pairingScript;
        SetDestination(pairingScript.center);
    }

    public void EndPairing()
    {
        isReadyForPairing = false;
        pairingScript = null;
        WaitForNextRandomDestination();
    }

    private IEnumerator Die()
    {
        navMeshAgent.isStopped = true;
        animator.SetTrigger("Death");
        yield return new WaitForSeconds(animator.GetCurrentAnimatorClipInfo(0)[0].clip.length);
        //Destroy(gameObject);
    }
}