using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrategyCamera : MonoBehaviour
{
    public float speed;

    void Update()
    {
        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");
        transform.Translate(horizontalInput * speed * Time.deltaTime, 0f, verticalInput * speed * Time.deltaTime, Space.World);
    }
}
