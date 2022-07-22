using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FloatObjectScript))]
public class PlayerController : MonoBehaviour
{
	public Vector3 COM;
	[Space(15)]
	public float speed = 1.0f;
	public float steerSpeed = 5.0f;
	public float movementThresold = 5.0f;

	private Transform m_COM;
	private float verticalInput;
	private float movementFactor;
	private float horizontalInput;
	private float steerFactor;

	public ParticleSystem MovingWaves;
	public ParticleSystem StillWaves;
	public AudioSource WavesSound;
	private bool isStopped = true;

	private GameManager gameManagerScript;

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
		gameManagerScript = GameObject.Find("GameManager").GetComponent<GameManager>();

		//S'obtenen les mides del mar
		GameObject sea = GameObject.Find("Sea");
		float seaWidth = sea.GetComponent<Renderer>().bounds.size.x;
		float seaLength = sea.GetComponent<Renderer>().bounds.size.z;

		//Es calculen les posicions màximes i mínimes del jugador dins el mar
		xLeftBounds = sea.transform.position.x - seaWidth / 2 + xLeftMargin;
		xRightBounds = sea.transform.position.x + seaWidth / 2 - xRightMargin;
		zLowerBounds = sea.transform.position.z - seaLength / 2 + zLowerMargin;
		zUpperBounds = sea.transform.position.z + seaLength / 2 - zUpperMargin;
	}

    // Update is called once per frame
    void Update()
	{
        if (!gameManagerScript.isInIsland)
		{
			Balance();
			Movement();
			Steer();
			SoundAndParticles();
		}
	}

	void Balance()
	{
		if (!m_COM)
		{
			m_COM = new GameObject("COM").transform;
			m_COM.SetParent(transform);
		}

		m_COM.position = COM;
		GetComponent<Rigidbody>().centerOfMass = m_COM.position;
	}

	void Movement()
	{
		verticalInput = Input.GetAxis("Vertical");
        movementFactor = Mathf.Lerp(movementFactor, verticalInput, Time.deltaTime / movementThresold);
		transform.Translate(0.0f, 0.0f, movementFactor * speed);

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

	void Steer()
	{
		horizontalInput = Input.GetAxis("Horizontal");
		steerFactor = Mathf.Lerp(steerFactor, horizontalInput * verticalInput, Time.deltaTime / movementThresold);
		transform.Rotate(0.0f, steerFactor * steerSpeed, 0.0f);
	}

	void SoundAndParticles()
    {
		if (Input.GetKey(KeyCode.W))
		{
			MovingWaves.Play();
			StillWaves.Stop();
			if (isStopped)
			{
				isStopped = false;
				StartCoroutine(StartFade(WavesSound, 0.5f, 0.15f));
			}

		}
		else
		{
			MovingWaves.Stop();
			StillWaves.Play();
			if (!isStopped)
			{
				isStopped = true;
				StartCoroutine(StartFade(WavesSound, 1.5f, 0f));
			}
		}
	}

	public static IEnumerator StartFade(AudioSource audioSource, float duration, float targetVolume)
	{
		float currentTime = 0;
		float start = audioSource.volume;
		while (currentTime < duration)
		{
			currentTime += Time.deltaTime;
			audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
			yield return null;
		}
		yield break;
	}
}