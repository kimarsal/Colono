using System.Collections;
using UnityEngine;

public abstract class ShipController : MonoBehaviour
{
    public float health = 1;
    public float healthLossOnImpact = 0.1f;
    public Vector3 COM;
    public float speed = 1.0f;
    public float steerSpeed = 5.0f;
    public float movementThresold = 5.0f;

    protected Transform m_COM;
    protected float verticalInput;
    protected float horizontalInput;
    protected float movementFactor;
    protected float steerFactor;

    public ParticleSystem MovingWaves;
    public ParticleSystem StillWaves;
    public AudioSource WavesSound;
    private bool isStopped = true;
    public AudioClip[] cannonballHitSounds;
    public ParticleSystem explosionPrefab;

    protected bool tryToShoot;
    public LeftCannonballScript leftCannonball;
    public RightCannonballScript rightCannonball;
    public Transform[] leftCannons;
    public Transform[] rightCannons;
    public AudioSource fireSound;
    public ParticleSystem smokePrefab;
    private float timeTillLastShot = 0f;
    private float timeBetweenShots = 2f;

    private void Update()
    {
        ManageInput();
        Balance();
        Movement();
        Steer();
        //SoundAndParticles();
        Shoot();
    }

    protected abstract void ManageInput();

    private void Balance()
    {
        if (!m_COM)
        {
            m_COM = new GameObject("COM").transform;
            m_COM.SetParent(transform);
        }

        m_COM.position = COM;
        GetComponent<Rigidbody>().centerOfMass = m_COM.position;
    }

    protected virtual void Movement()
    {
        movementFactor = Mathf.Lerp(movementFactor, verticalInput, Time.deltaTime / movementThresold);
        transform.Translate(0.0f, 0.0f, movementFactor * speed);
    }

    private void Steer()
    {
        steerFactor = Mathf.Lerp(steerFactor, horizontalInput * verticalInput, Time.deltaTime / movementThresold);
        transform.Rotate(0.0f, steerFactor * steerSpeed, 0.0f);
    }

    void SoundAndParticles()
    {
        if (verticalInput > 0)
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

    private void Shoot()
    {
        timeTillLastShot += Time.deltaTime;
        if (tryToShoot && timeTillLastShot > timeBetweenShots)
        {
            fireSound.Play();
            StartCoroutine(FireCannonballs());
            timeTillLastShot = 0;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Cannonball"))
        {
            health -= healthLossOnImpact;
            Instantiate(explosionPrefab, collision.gameObject.transform.position, Quaternion.Euler(new Vector3(-90, 0, 0)));
            Destroy(collision.gameObject);
            StartCoroutine(CannonballHit());
            
            if(health <= 0)
            {
                StartCoroutine(Sink());
            }
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

    private IEnumerator Sink()
    {
        GetComponent<FloatObjectScript>().enabled = false;
        yield return new WaitForSeconds(1);
        Instantiate(IslandEditor.Instance.box, transform.position, Quaternion.identity, transform.parent)
            .inventoryScript = GetComponent<ShipScript>().inventoryScript;
        Destroy(gameObject);

    }

    private IEnumerator FireCannonballs()
    {
        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < 5; i++)
        {
            Instantiate(leftCannonball, leftCannons[i].transform.position, leftCannonball.transform.rotation).shipController = this;
            Instantiate(smokePrefab, leftCannons[i].transform.position, Quaternion.Euler(new Vector3(0, transform.eulerAngles.y - 90, 0)));
            Instantiate(rightCannonball, rightCannons[i].transform.position, rightCannonball.transform.rotation).shipController = this;
            Instantiate(smokePrefab, rightCannons[i].transform.position, Quaternion.Euler(new Vector3(0, transform.eulerAngles.y + 90, 0)));
            yield return new WaitForSeconds(0.2f);
        }
    }
}
