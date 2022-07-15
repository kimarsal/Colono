using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireScript : MonoBehaviour
{
    public GameObject leftCannonball;
    public GameObject rightCannonball;
    public GameObject[] leftCannons;
    public GameObject[] rightCannons;
    public AudioSource fireSound;
    public ParticleSystem smokePrefab;
    private float timeTillLastShot = 0f;
    private float timeBetweenShots = 2f;

    void Update()
    {
        timeTillLastShot += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Space) && timeTillLastShot > timeBetweenShots)
        {
            fireSound.Play();
            StartCoroutine(FireCannonballs());
            timeTillLastShot = 0;
        }
    }

    IEnumerator FireCannonballs()
    {
        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < 5; i++)
        {
            Instantiate(leftCannonball, leftCannons[i].transform.position, leftCannonball.transform.rotation);
            Instantiate(smokePrefab, leftCannons[i].transform.position, Quaternion.Euler(new Vector3(0, transform.eulerAngles.y - 90, 0)));
            Instantiate(rightCannonball, rightCannons[i].transform.position, rightCannonball.transform.rotation);
            Instantiate(smokePrefab, rightCannons[i].transform.position, Quaternion.Euler(new Vector3(0, transform.eulerAngles.y + 90, 0)));
            yield return new WaitForSeconds(0.2f);
        }
    }
}
