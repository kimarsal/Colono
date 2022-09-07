using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeasantScript : MonoBehaviour
{
    public float speed;
    public float rotateSpeed;
    private Vector3 nextPos;
    private Rigidbody rb;

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

    public bool isNaked = false;
    private enum PeasantState { Idle, Walking, Chopping, Gathering};
    private Animator animator;

    void Start()
    {
        SetAppearence();

        rb = GetComponent<Rigidbody>();

        animator = GetComponent<Animator>();
        animator.SetInteger("State", (int)PeasantState.Chopping);
        nextPos = transform.position;
    }

    public void SetAppearence()
    {
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

        if (isNaked)
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

    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.W))
            animator.SetInteger("State", (int)PeasantState.Walking);*/
        if (Input.GetKeyDown(KeyCode.C))
            animator.SetInteger("State", (int)PeasantState.Chopping);
        if (Input.GetKeyDown(KeyCode.G))
            animator.SetInteger("State", (int)PeasantState.Gathering);
        /*if (Vector3.Distance(transform.position, nextPos) < 0.01f)
        {
            nextPos = new Vector3(Random.Range(NPCManager.minX, NPCManager.maxX), 0f, Random.Range(NPCManager.minZ, NPCManager.maxZ));
            //animator.SetInteger("State", (int)PeasantState.Chopping);
        }
        transform.position = Vector3.MoveTowards(transform.position, nextPos, speed * Time.deltaTime);

        Vector3 targetDirection = nextPos - transform.position;
        if (targetDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetDirection), rotateSpeed * Time.deltaTime);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        }*/

        animator.SetFloat("Speed", rb.velocity.magnitude);
    }
}
