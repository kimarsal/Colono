using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonballScript : MonoBehaviour
{
    protected float speed = 15;
    protected GameObject player;
    protected Rigidbody rb;
    private CannonballMissScript script;
    private bool isBeingDestroyed = false;

    protected void Setup()
    {
        player = GameObject.Find("Player");
        script = GameObject.Find("GameManager").GetComponent<CannonballMissScript>();
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Sea") && !isBeingDestroyed)
        {
            isBeingDestroyed = true;
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<SphereCollider>().enabled = false;
            StartCoroutine(CannonballMiss(transform.position));
        }
    }

    public IEnumerator CannonballMiss(Vector3 position)
    {
        Instantiate(script.splashPrefab, position, script.splashPrefab.transform.rotation);
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.PlayOneShot(script.splashAudioClip, 0.1f);
        yield return new WaitForSeconds(script.splashAudioClip.length);
        Destroy(gameObject);
    }
}
