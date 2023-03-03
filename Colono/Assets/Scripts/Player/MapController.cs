using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    public GameObject ship;

    public RectTransform arrow;
    public Transform destination;

    void Update()
    {
        //Es rota l'agulla de la brúixula
        float xDiff = destination.position.x - transform.position.x;
        float zDiff = destination.position.z - transform.position.z;
        float angle = Mathf.Atan2(zDiff, xDiff) * Mathf.Rad2Deg - 90;
        arrow.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        ship.transform.rotation = Quaternion.Euler(new Vector3(90, 0, -transform.rotation.z));

        //ship.GetComponent<SpriteRenderer>().flipX = (transform.rotation.y < 0 && transform.rotation.y > -180);
    }

    public void SetDestination(Transform newDestination)
    {
        destination = newDestination;
    }

}
