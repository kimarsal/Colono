using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    private IslandManager islandManager;
    public GameObject nextIsland;
    public GameObject ship;
    public RectTransform arrow;

    private float distanceToBoardIsland = 2f;
    /*private bool isWithinIslandRadius = false;
    private bool enteredIslandRadius = false;
    private bool exitedIslandRadius = false;*/

    void Start()
    {
        //S'obté el IslandGenerator
        islandManager = GameObject.Find("GameManager").GetComponent<IslandManager>();
    }

    void Update()
    {
        //Es rota l'agulla de la brúixula
        float xDiff = nextIsland.transform.position.x - transform.position.x;
        float zDiff = nextIsland.transform.position.z - transform.position.z;
        float angle = Mathf.Atan2(zDiff, xDiff) * Mathf.Rad2Deg - 90;
        arrow.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        ship.transform.rotation = Quaternion.Euler(new Vector3(90, 0, -transform.rotation.z));

        //ship.GetComponent<SpriteRenderer>().flipX = (transform.rotation.y < 0 && transform.rotation.y > -180);

        //Es comprova la distància amb les illes
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

        bool closeToIsland = false;
        float minDistance = 10;
        foreach(MeshCollider meshCollider in nextIsland.transform.GetChild(0).GetComponentsInChildren<MeshCollider>())
        {
            Vector3 colliderClosestPoint = Physics.ClosestPoint(transform.position, meshCollider, nextIsland.transform.position, nextIsland.transform.rotation);
            float distanceToClosestPoint = Vector3.Distance(transform.position, colliderClosestPoint);
            if (distanceToClosestPoint < minDistance)
            {
                minDistance = distanceToClosestPoint;
            }
            if(distanceToClosestPoint < distanceToBoardIsland)
            {
                closeToIsland = true;
                break;
            }
            
        }
        Debug.Log(minDistance);
        if (closeToIsland)
        {
            nextIsland.GetComponent<IslandScript>().PlayerIsNear();
        }
        else
        {
            nextIsland.GetComponent<IslandScript>().PlayerLeft();
        }
    }

    public void FollowIsland(GameObject island)
    {
        nextIsland = island;
    }


    /*private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Island")
        {
            enteredIslandRadius = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Island")
        {
            isWithinIslandRadius = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Island")
        {
            isWithinIslandRadius = false;
            exitedIslandRadius = true;
        }
    }*/
}
