using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeasantAdultScript : PeasantScript
{
    public GameObject axe;
    public GameObject shovel;
    public GameObject basket;
    public GameObject wateringCan;

    void Update()
    {
        CheckIfArrivedAtDestination();
        if (Input.GetKeyDown(KeyCode.T))
        {
            StopCharacter();
            animator.SetInteger("State", (int)PeasantState.Chopping);
        }
        else if (Input.GetKeyDown(KeyCode.Y))
        {
            StopCharacter();
            animator.SetInteger("State", (int)PeasantState.Digging);
        }
        else if (Input.GetKeyDown(KeyCode.U))
        {
            StopCharacter();
            animator.SetInteger("State", (int)PeasantState.Pulling);
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            StopCharacter();
            animator.SetInteger("Pick", 2);
            animator.SetInteger("State", (int)PeasantState.Gathering);
        }
        else if (Input.GetKeyDown(KeyCode.O))
        {
            StopCharacter();
            animator.SetInteger("Pick", 2);
            animator.SetInteger("State", (int)PeasantState.Watering);
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            StopCharacter();
            StartCoroutine(WaitForNextDestination());
        }


    }

    public void ToggleAxe()
    {
        axe.SetActive(!axe.activeSelf);
    }

    public void ToggleShovel()
    {
        shovel.SetActive(!shovel.activeSelf);
    }

    public void ToggleBasket()
    {
        basket.SetActive(!basket.activeSelf);
    }

    public void ToggleWateringCan()
    {
        wateringCan.SetActive(!wateringCan.activeSelf);
    }
}
