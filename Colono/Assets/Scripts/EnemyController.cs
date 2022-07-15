using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Vector3 COM;
    [Space(15)]
    public float speed = 0.05f;
    public float steerSpeed = 0.5f;
    public float movementThresold = 1f;

    Transform m_COM;
    float verticalInput;
    float movementFactor;
    float horizontalInput;
    float steerFactor;

    private GameObject player;
    private bool isInFront;
    private bool isToTheRight;
    public AudioClip[] cannonballHitSounds;
    public ParticleSystem explosionPrefab;

    void Start()
    {
        player = GameObject.Find("Player");
    }

    void Update()
    {
        Balance();
        Movement();
        Steer();

        /*float xDiff = player.transform.position.x - transform.position.x;
		float zDiff = player.transform.position.z - transform.position.z;
		float angle = Mathf.Atan2(zDiff, xDiff) * Mathf.Rad2Deg % 360;*/
        Vector2 Point_1 = new Vector2(player.transform.position.x, player.transform.position.z);
        Vector2 Point_2 = new Vector2(transform.position.x, transform.position.z);
        float angle = Mathf.Atan2(Point_2.y - Point_1.y, Point_2.x - Point_1.x) * Mathf.Rad2Deg;
        float angleDiff = (transform.rotation.eulerAngles.y - angle) % 360 - 180;

        isInFront = angleDiff < 0;
        isToTheRight = Mathf.Abs(angleDiff) < 90;
        //Debug.Log(angleDiff);

        Debug.Log("Player is " + (isInFront ? "in front" : "behind") + " and to the " + (isToTheRight ? "right" : "left"));
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
        verticalInput = isInFront ? 1 : 0;
        movementFactor = Mathf.Lerp(movementFactor, verticalInput, Time.deltaTime / movementThresold);
        transform.Translate(0.0f, 0.0f, movementFactor * speed);
    }

    void Steer()
    {
        horizontalInput = isToTheRight ? 1 : -1;
        steerFactor = Mathf.Lerp(steerFactor, horizontalInput * verticalInput, Time.deltaTime / movementThresold);
        transform.Rotate(0.0f, steerFactor * steerSpeed, 0.0f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Cannonball"))
        {
            Instantiate(explosionPrefab, collision.gameObject.transform.position, Quaternion.Euler(new Vector3(-90, 0, 0)));
            Destroy(collision.gameObject);
            StartCoroutine(CannonballHit());
        }

    }

    private IEnumerator CannonballHit()
    {
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        AudioClip audioClip = cannonballHitSounds[Random.Range(0, cannonballHitSounds.Length)];
        audioSource.PlayOneShot(audioClip, 0.2f);
        yield return new WaitForSeconds(audioClip.length);
        Destroy(audioSource);
    }
}
