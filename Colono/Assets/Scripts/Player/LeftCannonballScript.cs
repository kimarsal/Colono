using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftCannonballScript : CannonballScript
{
    void Start()
    {
        Setup();
        rb.velocity = speed * (shipController.transform.right + new Vector3(0, -0.2f, 0)) * -1;
    }
}
