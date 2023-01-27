using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    public GameObject ship;

    public RectTransform arrow;
    private Vector3 destination;

    void Update()
    {
        //Es rota l'agulla de la br�ixula
        float xDiff = destination.x - transform.position.x;
        float zDiff = destination.z - transform.position.z;
        float angle = Mathf.Atan2(zDiff, xDiff) * Mathf.Rad2Deg - 90;
        arrow.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        ship.transform.rotation = Quaternion.Euler(new Vector3(90, 0, -transform.rotation.z));

        //ship.GetComponent<SpriteRenderer>().flipX = (transform.rotation.y < 0 && transform.rotation.y > -180);

        //Es comprova la dist�ncia amb les illes
        /*if (enteredIslandRadius)
        {
            enteredIslandRadius = false;
        }
        else if (exitedIslandRadius)
        {
            if (!isWithinIslandRadius)
            {
                islandGenerator.GenerateIsland(transform);
                exitedIslandRadius = false;
            }
        }*/
    }

    public void SetDestination(Vector3 newDestination)
    {
        destination = newDestination;
    }

}
