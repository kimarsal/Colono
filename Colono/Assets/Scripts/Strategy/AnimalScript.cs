using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static PeasantScript;

public class AnimalScript : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    private Animator animator;
    public float walkSpeed = 1;
    public float runSpeed = 2;
    public bool isRunning = false;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
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
        SetDestination(AnimalManager.GetRandomPointWithinRange());
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
