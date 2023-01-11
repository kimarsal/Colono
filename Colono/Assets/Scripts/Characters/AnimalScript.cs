using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class AnimalScript : MonoBehaviour
{
    public PenScript penScript;
    public ResourceScript.AnimalType animalType;

    private NavMeshAgent navMeshAgent;
    private Animator animator;
    public float walkSpeed = 1;
    public float runSpeed = 2;
    public bool isRunning = false;

    public float ageSpeed = 0.05f;
    public float age;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        navMeshAgent.enabled = true;
    }

    public void SetNewPen(bool isAnimalNew = true)
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        if (isAnimalNew) transform.position = NPCManager.GetClosestPointInNavMesh(penScript.entry.position);
        navMeshAgent.enabled = true;
        SetDestination(NPCManager.GetRandomPointWithinRange(penScript.minPos, penScript.maxPos));
    }

    void Update()
    {
        if (!navMeshAgent.isStopped)
        {
            float d = Vector3.Distance(transform.position, navMeshAgent.destination);
            if (d < 0.3f) //Ha arribat al destí
            {
                StopCharacter();
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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(Die());
        }
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

    private IEnumerator Die()
    {
        navMeshAgent.isStopped = true;
        animator.SetTrigger("Death");
        yield return new WaitForSeconds(animator.GetCurrentAnimatorClipInfo(0)[0].clip.length);
        //Destroy(gameObject);
    }
}

[System.Serializable]
public class AnimalInfo
{
    public int animalType;
    public int age;
    public int island;
    public int construction;
}
