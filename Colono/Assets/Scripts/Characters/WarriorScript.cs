using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarriorScript : MonoBehaviour
{
    public float speed;
    public float rotateSpeed;
    private Vector3 nextPos;
    private Rigidbody rb;

    public GameObject head1;
    public GameObject head2;
    public GameObject hair1;
    public GameObject hair2;
    public GameObject beard1;
    public GameObject beard2;

    public GameObject lowerAlly;
    public GameObject upperAlly;
    public GameObject bootsAlly;
    public GameObject capeAlly;
    public GameObject gauntletsAlly;
    public GameObject helmetAlly;

    public GameObject lowerEnemy;
    public GameObject upperEnemy;
    public GameObject bootsEnemy;
    public GameObject capeEnemy;
    public GameObject gauntletsEnemy;
    public GameObject helmetEnemy;

    public Material material;
    private GameObject head;
    private GameObject hair;
    private GameObject lower;
    private GameObject upper;

    public bool isEnemy = false;
    public bool isMale = false;
    private enum PeasantState { Idle, Attack};
    private Animator animator;

    void Start()
    {
        SetAppearence();

        rb = GetComponent<Rigidbody>();

        animator = GetComponent<Animator>();
        animator.SetInteger("State", (int)PeasantState.Attack);
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

        if (Random.Range(0, 2) == 0)
        {
            hair1.SetActive(false);
            hair = hair2;
        }
        else
        {
            hair2.SetActive(false);
            hair = hair1;
        }

        if (isMale)
        {
            if (Random.Range(0, 2) == 0) hair.SetActive(false);
            if (Random.Range(0, 2) == 0) beard1.SetActive(false);
            if (Random.Range(0, 2) == 0) beard2.SetActive(false);
        }

        if (isEnemy)
        {
            lowerAlly.SetActive(false);
            upperAlly.SetActive(false);
            bootsAlly.SetActive(false);
            capeAlly.SetActive(false);
            gauntletsAlly.SetActive(false);
            helmetAlly.SetActive(false);
            lower = lowerEnemy;
            upper = upperEnemy;
        }
        else
        {
            lowerEnemy.SetActive(false);
            upperEnemy.SetActive(false);
            bootsEnemy.SetActive(false);
            capeEnemy.SetActive(false);
            gauntletsEnemy.SetActive(false);
            helmetEnemy.SetActive(false);
            lower = lowerAlly;
            upper = upperAlly;
        }

        Material newMaterial = new Material(material);
        //newMaterial.SetColor("_OTHERCOLOR", Random.ColorHSV());
        newMaterial.SetColor("_SKINCOLOR", NPCManager.GetRandomSkinColor()); //Color pell
        newMaterial.SetColor("_HAIRCOLOR", NPCManager.GetRandomHairColor()); //Color cabell
        head.GetComponent<SkinnedMeshRenderer>().material = newMaterial;
        hair.GetComponent<SkinnedMeshRenderer>().material = newMaterial;
        lower.GetComponent<SkinnedMeshRenderer>().material = newMaterial;
        upper.GetComponent<SkinnedMeshRenderer>().material = newMaterial;
    }

    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.W))
            animator.SetInteger("State", (int)PeasantState.Walking);
        if (Input.GetKeyDown(KeyCode.C))
            animator.SetInteger("State", (int)PeasantState.Chopping);
        if (Input.GetKeyDown(KeyCode.G))
            animator.SetInteger("State", (int)PeasantState.Gathering);*/
        if (Vector3.Distance(transform.position, nextPos) < 0.01f)
        {
            nextPos = new Vector3(Random.Range(NPCManager.minX, NPCManager.maxX), 0f, Random.Range(NPCManager.minZ, NPCManager.maxZ));
            //animator.SetInteger("State", (int)PeasantState.Chopping);
        }
        if(animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Warrior_Run")
        {
            transform.position = Vector3.MoveTowards(transform.position, nextPos, speed * Time.deltaTime);
        }

        Vector3 targetDirection = nextPos - transform.position;
        if (targetDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetDirection), rotateSpeed * Time.deltaTime);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        }
    }
}
