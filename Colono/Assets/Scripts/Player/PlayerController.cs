using System.Collections;
using UnityEngine;

public class PlayerController : ShipController
{

	//Distància màxima amb les vores
	private float xLeftMargin = 20f;
	private float xRightMargin = 20f;
	private float zLowerMargin = 5f;
	private float zUpperMargin = 20f;

	//Marges de la zona de joc
	public float xRightBounds;
	public float zLowerBounds;
	public float zUpperBounds;
	public float xLeftBounds;

	private void Start()
    {
        rb = GetComponent<Rigidbody>();

        //S'obtenen les mides del mar
        GameObject sea = GameObject.Find("Sea");
		Bounds seaBounds = sea.GetComponent<Renderer>().bounds;

		//Es calculen les posicions màximes i mínimes del jugador dins el mar
		xLeftBounds = sea.transform.position.x - seaBounds.size.x / 2 + xLeftMargin;
		xRightBounds = sea.transform.position.x + seaBounds.size.x / 2 - xRightMargin;
		zLowerBounds = sea.transform.position.z - seaBounds.size.z / 2 + zLowerMargin;
		zUpperBounds = sea.transform.position.z + seaBounds.size.z / 2 - zUpperMargin;
	}
	
	public void Initialize(PlayerController playerInfo)
	{
        health = new int[4];
		if(playerInfo is null)
		{
            for (int i = 0; i < health.Length; i++)
            {
                health[i] = maxHealth;
            }
        }
		else
		{
            for (int i = 0; i < health.Length; i++)
            {
                health[i] = playerInfo.health[i];
            }
        }
	}

    protected override void ManageInput()
    {
        if (GameManager.Instance.isInIsland || ShipScript.Instance.fishingScript.enabled)
        {
			verticalInput = 0;
			horizontalInput = 0;
			wantsToShoot = false;
			return;
		}


        verticalInput = Mathf.Clamp01(Input.GetAxis("Vertical"));
        horizontalInput = Input.GetAxis("Horizontal");
        wantsToShoot = Input.GetKeyDown(KeyCode.Space);
    }

	//Es manté el jugador dins els marges
	protected override void Movement()
	{
		base.Movement();

		transform.position = new Vector3(Mathf.Clamp(transform.position.x, xLeftBounds, xRightBounds),
										transform.position.y,
										Mathf.Clamp(transform.position.z, zLowerBounds, zUpperBounds));
	}

    protected override void Shoot()
    {
        if(EnemyController.Instance.enemyStatus == EnemyController.EnemyStatus.Trading)
		{
			EnemyController.Instance.enemyStatus = Random.Range(0, 2) % 2 == 0 ? EnemyController.EnemyStatus.Attacking : EnemyController.EnemyStatus.Fleeing;
        }
    }

    protected override int LoseHealth()
    {
        int position = base.LoseHealth();

		return position;
    }

    protected override void Sink()
    {
		CameraScript.Instance.canMove = false;
		GameManager.Instance.isGameOver = true;
		rb.constraints = RigidbodyConstraints.FreezeRotation;
        base.Sink();
		StartCoroutine(SinkCoroutine());
    }

	protected override IEnumerator SinkCoroutine()
	{
		yield return new WaitForSeconds(2);
		GameManager.Instance.GameOver();
		rb.constraints = RigidbodyConstraints.FreezeAll;
	}

	public bool Repair(int position)
	{
		health[position]++;
		return CanRepair(position);
	}

	public bool CanRepair(int position)
    {
        return health[position] < maxHealth;
    }
}