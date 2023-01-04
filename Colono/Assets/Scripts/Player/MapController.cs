using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    private GameManager gameManager;
    public GameObject ship;
    public IslandScript closestIsland;
    public float distanceToBoardIsland = 2f;

    public RectTransform arrow;
    private Vector3 destination;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    void Update()
    {
        //Es rota l'agulla de la brúixula
        float xDiff = destination.x - transform.position.x;
        float zDiff = destination.z - transform.position.z;
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
        Vector3 colliderClosestPoint = new Vector3();
        float minDistance = 10;
        foreach(MeshCollider meshCollider in closestIsland.convexColliders.GetComponentsInChildren<MeshCollider>())
        {
            colliderClosestPoint = Physics.ClosestPoint(transform.position, meshCollider, closestIsland.transform.position, closestIsland.transform.rotation);
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

        if (closeToIsland)
        {
            GetComponent<ShipScript>().SetClosestPoint(colliderClosestPoint);
            gameManager.PlayerIsNearIsland(closestIsland);
        }
        else
        {
            gameManager.PlayerIsFarFromIsland();
        }
    }

    public void SetDestination(Vector3 newDestination)
    {
        destination = newDestination;
    }

}
