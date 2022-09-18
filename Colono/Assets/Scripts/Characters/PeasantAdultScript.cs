using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeasantAdultScript : PeasantScript
{
    public GameObject axe;
    public GameObject basket;

    void Update()
    {
        //CheckIfArrivedAtDestination();
        if (Input.GetKeyDown(KeyCode.Y))
        {
            StopCharacter();
            animator.SetInteger("State", (int)PeasantState.Chopping);
        }
        else if (Input.GetKeyDown(KeyCode.U))
        {
            StopCharacter();
            animator.SetInteger("State", (int)PeasantState.Planting);
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            StopCharacter();
            animator.SetInteger("Pick", 2);
            animator.SetInteger("State", (int)PeasantState.Gathering);
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

    public void ToggleBasket()
    {
        basket.SetActive(!basket.activeSelf);
    }
}
