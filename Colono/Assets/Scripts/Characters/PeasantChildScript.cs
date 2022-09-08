using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeasantChildScript : PeasantScript
{

    void Update()
    {
        //CheckIfArrivedAtDestination();
        if (Input.GetKeyDown(KeyCode.D))
        {
            StopCharacter();
            animator.SetInteger("State", (int)PeasantState.Dancing);
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            StopCharacter();
            StartCoroutine(WaitForNextDestination());
        }
    }
}
