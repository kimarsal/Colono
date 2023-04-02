using System.Collections;
using UnityEngine;

public class EnemyController : ShipController
{
    public float distanceToShoot = 12;
    public float angleToShoot = 10;

    private bool isInFront;
    private bool isToTheRight;
    private bool isTooFar;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    protected override void ManageInput()
    {
        float xDiff = transform.position.x - ShipScript.Instance.transform.position.x;
		float zDiff = transform.position.z - ShipScript.Instance.transform.position.z;
        float angle = 180 - Mathf.Atan2(zDiff, xDiff) * Mathf.Rad2Deg;
        float angleDiff = angle - transform.rotation.eulerAngles.y;
        if (angleDiff < 0) angleDiff = 360 + angleDiff;
        angleDiff = (angleDiff) % 360 - 180;

        isInFront = angleDiff > 0;
        isToTheRight = Mathf.Abs(angleDiff) > 90;
        //Debug.Log("Player is " + (isInFront ? "in front" : "behind") + " and to the " + (isToTheRight ? "right" : "left"));

        float distance = Vector3.Distance(transform.position, ShipScript.Instance.transform.position);
        isTooFar = distance > distanceToShoot;

        if (isTooFar)
        {
            verticalInput = 1;
            horizontalInput = isToTheRight ? 1 : -1;
            tryToShoot = false;
        }
        else
        {
            bool canShoot = false;
            if (isToTheRight)
            {
                if (180 - Mathf.Abs(angleDiff) > angleToShoot)
                {
                    horizontalInput = -1;
                }
                else canShoot = true;
            }
            else
            {
                if (Mathf.Abs(angleDiff) > angleToShoot)
                {
                    horizontalInput = 1;
                }
                else canShoot = true;
            }
            
            if(canShoot)
            {
                verticalInput = 0;
                tryToShoot = true;
            }
            else
            {
                verticalInput = 1;
                tryToShoot = false;
            }
        }
    }
}
