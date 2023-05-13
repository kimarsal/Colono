using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : ShipController
{
	[SerializeField] private Button[] repairButtons;

	//Distància màxima amb les vores
	private float xLeftMargin = 20f;
	private float xRightMargin = 20f;
	private float zLowerMargin = 5f;
	private float zUpperMargin = 20f;

	//Marges de la zona de joc
	public float xLeftBounds;
	public float xRightBounds;
	public float zLowerBounds;
	public float zUpperBounds;

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
		if(playerInfo == null)
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

	protected override void Movement()
	{
		base.Movement();

		//Es manté el jugador dins els marges
		if (transform.position.x < xLeftBounds)
			transform.position = new Vector3(xLeftBounds, transform.position.y, transform.position.z);
		else if (transform.position.x > xRightBounds)
			transform.position = new Vector3(xRightBounds, transform.position.y, transform.position.z);

		if (transform.position.z < zLowerBounds)
			transform.position = new Vector3(transform.position.x, transform.position.y, zLowerBounds);
		else if (transform.position.z > zUpperBounds)
			transform.position = new Vector3(transform.position.x, transform.position.y, zUpperBounds);
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

		repairButtons[position].interactable = true;

		return position;
    }

    protected override void Sink()
    {
        base.Sink();
		rb.constraints = RigidbodyConstraints.FreezeAll;
		GameManager.Instance.GameOver();
    }

	public void Repair(int position)
	{
		health[position]++;
		repairButtons[position].interactable = health[position] < maxHealth;
	}

}