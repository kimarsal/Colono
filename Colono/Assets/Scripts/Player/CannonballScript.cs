using System.Collections;
using UnityEngine;

public class CannonballScript : MonoBehaviour
{
    public bool isRightCannonball;
    protected float speed = 15;
    public ShipController shipController;
    protected Rigidbody rb;
    private bool isBeingDestroyed = false;
    [SerializeField] private AudioSource splashAudioSource;
    [SerializeField] private ParticleSystem splashPrefab;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = speed * ((isRightCannonball ? 1 : -1) * shipController.transform.right + new Vector3(0, 0.2f, 0));
    }

    private void Update()
    {
        if (transform.position.y <= -0.5 && !isBeingDestroyed)
        {
            isBeingDestroyed = true;
            GetComponent<SphereCollider>().enabled = false;
            StartCoroutine(CannonballMiss());
        }
    }

    public IEnumerator CannonballMiss()
    {
        Instantiate(splashPrefab, transform.position, splashPrefab.transform.rotation);
        splashAudioSource.Play();
        yield return new WaitForSeconds(splashAudioSource.clip.length);
        Destroy(gameObject);
    }
}
