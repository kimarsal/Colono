using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonballScript : MonoBehaviour
{
    protected float speed = 15;
    public ShipController shipController;
    protected Rigidbody rb;
    private bool isBeingDestroyed = false;
    [SerializeField] private AudioSource splashAudioSource;
    [SerializeField] private ParticleSystem splashPrefab;

    protected void Setup()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (transform.position.y <= -0.5 && !isBeingDestroyed)
        {
            isBeingDestroyed = true;
            //GetComponent<MeshRenderer>().enabled = false;
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
