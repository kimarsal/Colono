using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeasantAdultScript : PeasantScript
{
    public GameObject axe;

    void Update()
    {
        //CheckIfArrivedAtDestination();
        if (Input.GetKeyDown(KeyCode.C))
        {
            StopCharacter();
            animator.SetInteger("State", (int)PeasantState.Chopping);
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            StopCharacter();
            animator.SetInteger("State", (int)PeasantState.Planting);
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            StopCharacter();
            StartCoroutine(WaitForNextDestination());
        }


    }

    public void ToggleAxe()
    {
        axe.SetActive(!axe.activeSelf);
    }
}
