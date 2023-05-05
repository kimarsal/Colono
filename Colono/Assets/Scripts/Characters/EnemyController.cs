using System.Collections;
using UnityEngine;

public class EnemyController : ShipController
{
    public static EnemyController Instance { get; private set; }
    public enum EnemyStatus { Trading, Attacking, Fleeing, StandBy }
    public EnemyStatus enemyStatus = EnemyStatus.Attacking;
    public float distanceToShoot = 12;
    public float angleToShoot = 10;

    private bool isInFront;
    private bool isToTheRight;
    private bool isTooFar;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        health = new int[4];
        Initialize();
    }

    public void Initialize()
    {
        for (int i = 0; i < health.Length; i++)
        {
            health[i] = 2;
        }
        enemyStatus = Random.Range(0, 2) % 2 == 0 ? EnemyStatus.Trading : EnemyStatus.Attacking;
        GetComponent<EnemyShipScript>().RandomizeInventory();
        currentTime = 0;
    }

    protected override void ManageInput()
    {
        if(enemyStatus == EnemyStatus.Trading)
        {
            verticalInput = 0;
            horizontalInput = 0;
            wantsToShoot = false;
            return;
        }

        float xDiff = transform.position.x - ShipScript.Instance.transform.position.x;
		float zDiff = transform.position.z - ShipScript.Instance.transform.position.z;
        float angle = 180 - Mathf.Atan2(zDiff, xDiff) * Mathf.Rad2Deg;
        float angleDiff = angle - transform.rotation.eulerAngles.y;
        if (angleDiff < 0) angleDiff = 360 + angleDiff;
        angleDiff = (angleDiff) % 360 - 180;

        isInFront = angleDiff > 0;
        isToTheRight = Mathf.Abs(angleDiff) > 90;
        //Debug.Log("Player is " + (isInFront ? "in front" : "behind") + " and to the " + (isToTheRight ? "right" : "left"));

        if(enemyStatus == EnemyStatus.Fleeing)
        {
            verticalInput = 1;
            horizontalInput = isToTheRight ? -1 : 1;
            wantsToShoot = false;
            return;
        }

        float distance = Vector3.Distance(transform.position, ShipScript.Instance.transform.position);
        isTooFar = distance > distanceToShoot;

        if (isTooFar)
        {
            verticalInput = 1;
            horizontalInput = isToTheRight ? 1 : -1;
            wantsToShoot = false;
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
                wantsToShoot = true;
            }
            else
            {
                verticalInput = 1;
                wantsToShoot = false;
            }
        }
    }

    protected override IEnumerator Sink()
    {
        base.Sink();
        HideFromMap();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
        return null;
    }

    public void HideFromMap()
    {
        transform.position = new Vector3(0, -0.5f, -500);
        enemyStatus = EnemyStatus.StandBy;
    }
}
