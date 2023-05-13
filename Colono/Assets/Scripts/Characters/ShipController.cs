using Newtonsoft.Json;
using System.Collections;
using UnityEngine;

[JsonObject(MemberSerialization.OptIn)]
public abstract class ShipController : MonoBehaviour
{
    [JsonProperty] protected int[] health;
    protected const int maxHealth = 3;
    public float speed = 1.0f;
    public float steerSpeed = 5.0f;
    protected float currentTime;

    protected float verticalInput;
    protected float horizontalInput;

    private bool isStopped = true;
    [SerializeField] private ParticleSystem MovingWaves;
    //[SerializeField] private ParticleSystem StillWaves;
    [SerializeField] private AudioSource WavesSound;
    [SerializeField] private AudioClip[] cannonballHitSounds;
    [SerializeField] private ParticleSystem explosionPrefab;

    protected bool wantsToShoot;
    [SerializeField] private LeftCannonballScript leftCannonball;
    [SerializeField] private RightCannonballScript rightCannonball;
    [SerializeField] private Transform[] leftCannons;
    [SerializeField] private Transform[] rightCannons;
    [SerializeField] private AudioSource soundSource;
    [SerializeField] private AudioClip cannonClip;
    [SerializeField] private ParticleSystem smokePrefab;
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
        TryToShoot();
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
        if (verticalInput > 0 && !GameManager.Instance.isInIsland)
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

    private void TryToShoot()
    {
        timeTillLastShot += Time.deltaTime;
        if (wantsToShoot && timeTillLastShot > timeBetweenShots)
        {
            soundSource.PlayOneShot(cannonClip);
            StartCoroutine(FireCannonballs());
            timeTillLastShot = 0;

            Shoot();
        }
    }

    protected virtual void Shoot()
    {

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
            Instantiate(explosionPrefab, collision.gameObject.transform.position, Quaternion.Euler(new Vector3(-90, 0, 0)));
            Destroy(collision.gameObject);
            StartCoroutine(CannonballHit());

            LoseHealth();
        }

    }

    protected virtual int LoseHealth()
    {
        int i = Random.Range(0, health.Length);
        int j = (i + 1) % health.Length;
        while (i != j)
        {
            if (health[j] > 0)
            {
                health[j]--;
                break;
            }
            j = (j + 1) % health.Length;
        }

        if (i == j)
        {
            Sink();
        }

        return j;
    }

    private IEnumerator CannonballHit()
    {
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        AudioClip audioClip = cannonballHitSounds[Random.Range(0, cannonballHitSounds.Length)];
        audioSource.PlayOneShot(audioClip, 0.2f);
        yield return new WaitForSeconds(audioClip.length);
        Destroy(audioSource);
    }

    protected virtual void Sink()
    {
        rb.useGravity = true;
    }
}
