using System.Collections;
using UnityEngine;

public abstract class ShipController : MonoBehaviour
{
    public float health = 1;
    public float healthLossOnImpact = 0.1f;
    public float speed = 1.0f;
    public float steerSpeed = 5.0f;
    private float currentTime;

    protected float verticalInput;
    protected float horizontalInput;

    public ParticleSystem MovingWaves;
    //public ParticleSystem StillWaves;
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
    protected Rigidbody rb;

    private void Update()
    {
        ManageInput();
        Balance();
        Movement();
        Steer();
        SoundAndParticles();
        Shoot();
    }

    protected abstract void ManageInput();

    private void Balance()
    {
        currentTime = (currentTime + Time.deltaTime) % (Mathf.PI * 2);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, Mathf.Sin(currentTime) * 5f);
    }

    protected virtual void Movement()
    {
        rb.AddForce(transform.forward * speed * verticalInput);
    }

    private void Steer()
    {
        rb.AddTorque(transform.up * steerSpeed * horizontalInput * verticalInput);
    }

    void SoundAndParticles()
    {
        if (verticalInput > 0)
        {
            MovingWaves.Play();
            //StillWaves.Stop();
            if (isStopped)
            {
                isStopped = false;
                StartCoroutine(StartFade(WavesSound, 0.5f, 0.15f));
            }

        }
        else
        {
            MovingWaves.Stop();
            //StillWaves.Play();
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
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.useGravity = true;
        yield return new WaitForSeconds(1);
        Instantiate(ResourceScript.Instance.box, transform.position, Quaternion.identity, transform.parent).inventoryScript = GetComponent<EnemyShipScript>().inventoryScript;
        Destroy(gameObject);

    }
}
