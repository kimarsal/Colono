using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private Vector3 offset;
    private GameObject player;
    private PlayerController playerController;
    private GameManager gameManagerScript;
    private Vector3 islandPosition;
    private Vector3 targetPosition;
    private float speed = 25f;

    //Distància màxima amb els marges
    private float xLeftMarginPlayer = 10f;
    private float xRightMarginPlayer = 10f;
    private float zLowerMarginPlayer = -10f;
    private float zUpperMarginPlayer = 15f;

    private float xLeftMarginIsland = 40f;
    private float xRightMarginIsland = 40f;
    private float zLowerMarginIsland = 20f;
    private float zUpperMarginIsland = 50f;

    // Start is called before the first frame update
    void Start()
    {
        //S'obté Player i el seu script
        player = GameObject.Find("Player");
        //offset = gameObject.transform.position;
        offset = new Vector3(0, 15, -8);
        playerController = player.GetComponent<PlayerController>();
        gameManagerScript = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!gameManagerScript.isInIsland)
        {
            //Es mou la càmera amb el jugador
            targetPosition = player.transform.position + offset;

            //Es manté la càmera dins els marges
            if (player.transform.position.x < playerController.xLeftBounds + xLeftMarginPlayer)
                targetPosition = new Vector3(playerController.xLeftBounds + xLeftMarginPlayer, targetPosition.y, targetPosition.z);
            else if (targetPosition.x > playerController.xRightBounds - xRightMarginPlayer)
                targetPosition = new Vector3(playerController.xRightBounds - xRightMarginPlayer, targetPosition.y, targetPosition.z);

            if (targetPosition.z < playerController.zLowerBounds + zLowerMarginPlayer)
                targetPosition = new Vector3(targetPosition.x, targetPosition.y, playerController.zLowerBounds + zLowerMarginPlayer);
            else if (targetPosition.z > playerController.zUpperBounds - zUpperMarginPlayer)
                targetPosition = new Vector3(targetPosition.x, targetPosition.y, playerController.zUpperBounds - zUpperMarginPlayer);
        }
        else
        {
            targetPosition = transform.position + new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")) * speed * Time.deltaTime;

            //Es manté la càmera dins els marges
            if (targetPosition.x < islandPosition.x - IslandGenerator.mapChunkSize / 2 + xLeftMarginIsland)
                targetPosition = new Vector3(islandPosition.x - IslandGenerator.mapChunkSize / 2 + xLeftMarginIsland, targetPosition.y, targetPosition.z);
            else if (targetPosition.x > islandPosition.x + IslandGenerator.mapChunkSize / 2 - xRightMarginIsland)
                targetPosition = new Vector3(islandPosition.x + IslandGenerator.mapChunkSize / 2 - xRightMarginIsland, targetPosition.y, targetPosition.z);

            if (targetPosition.z < islandPosition.z - IslandGenerator.mapChunkSize / 2 + zLowerMarginIsland)
                targetPosition = new Vector3(targetPosition.x, targetPosition.y, islandPosition.z - IslandGenerator.mapChunkSize / 2 + zLowerMarginIsland);
            else if (targetPosition.z > islandPosition.z + IslandGenerator.mapChunkSize / 2 - zUpperMarginIsland)
                targetPosition = new Vector3(targetPosition.x, targetPosition.y, islandPosition.z + IslandGenerator.mapChunkSize / 2 - zUpperMarginIsland);
        }
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
    }

    public void SetIslandCamera(Vector3 position)
    {
        islandPosition = position;
    }

    public void ResetPlayerCamera()
    {
        StartCoroutine(FastCameraSwipe());
    }

    private IEnumerator FastCameraSwipe()
    {
        float previousSpeed = speed;
        targetPosition = player.transform.position + offset;
        float distance = Vector3.Distance(transform.position, targetPosition);
        speed = distance / 0.5f; //Tornar al lloc en 0.1 segon
        yield return new WaitForSeconds(0.5f);
        speed = previousSpeed;
    }
}
