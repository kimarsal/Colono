using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    private IslandManager islandGenerator;
    public GameObject nextIsland;
    public GameObject ship;
    public RectTransform arrow;

    private bool isWithinIslandRadius = false;
    private bool enteredIslandRadius = false;
    private bool exitedIslandRadius = false;

    // Start is called before the first frame update
    void Start()
    {
        //S'obté el IslandGenerator
        islandGenerator = GameObject.Find("GameManager").GetComponent<IslandManager>();
    }

    // Update is called once per frame
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
        if (enteredIslandRadius)
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
        }
    }

    public void FollowIsland(GameObject island)
    {
        nextIsland = island;
    }


    private void OnTriggerEnter(Collider other)
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
    }
}
