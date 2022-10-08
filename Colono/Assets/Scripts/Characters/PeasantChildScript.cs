using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeasantChildScript : PeasantScript
{
    void Update()
    {
        CheckIfArrivedAtDestination();
        if (Input.GetKeyDown(KeyCode.O))
        {
            StopCharacter();
            animator.SetInteger("State", (int)PeasantState.Dancing);
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            StopCharacter();
            StartCoroutine(WaitForNextRandomDestination());
        }
    }
}
