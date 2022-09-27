using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarriorScript : MonoBehaviour
{
    public float walkSpeed;
    public float runSpeed;
    private bool isRunning = false;

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

    public GameObject sword;

    public Material material;
    private GameObject head;
    private GameObject hair;
    private GameObject lower;
    private GameObject upper;

    public bool isEnemy = false;
    public bool isMale = false;
    private enum PeasantState { Idle, Attack};
    private PeasantState state;
    private Animator animator;

    void Start()
    {
        SetAppearence();

        state = PeasantState.Attack;
        animator = GetComponent<Animator>();
        animator.SetInteger("State", (int)state);
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
        newMaterial.SetColor("_SKINCOLOR", NPCManager.GetRandomSkinColor()); //Color pell
        newMaterial.SetColor("_HAIRCOLOR", NPCManager.GetRandomHairColor()); //Color cabell
        newMaterial.SetColor("_CLOTH1COLOR", Random.ColorHSV()); //Color roba 1
        newMaterial.SetColor("_CLOTH2COLOR", Random.ColorHSV()); //Color roba 2
        newMaterial.SetColor("_CLOTH3COLOR", Random.ColorHSV()); //Color roba 3
        //newMaterial.SetTexture("_COATOFARMSMASK", GameManager.coatOfArms);
        head.GetComponent<SkinnedMeshRenderer>().material = newMaterial;
        hair.GetComponent<SkinnedMeshRenderer>().material = newMaterial;
        lower.GetComponent<SkinnedMeshRenderer>().material = newMaterial;
        upper.GetComponent<SkinnedMeshRenderer>().material = newMaterial;
    }

    public void ToggleSword()
    {
        sword.SetActive(!sword.activeSelf);
    }
}
