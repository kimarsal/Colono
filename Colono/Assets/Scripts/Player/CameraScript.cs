using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private Vector3 offset;
    private GameObject player;
    private PlayerController playerController;
    private bool isFollowingPlayer = true;
    private Vector3 targetPosition;
    private float speed = 50f;

    //Distància màxima amb els marges
    private float xLeftMargin = 10f;
    private float xRightMargin = 10f;
    private float zLowerMargin = -10f;
    private float zUpperMargin = 15f;

    // Start is called before the first frame update
    void Start()
    {
        //S'obté Player i el seu script
        player = GameObject.Find("Player");
        //offset = gameObject.transform.position;
        offset = new Vector3(0, 15, -8);
        playerController = player.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (isFollowingPlayer)
        {
            //Es mou la càmera amb el jugador
            targetPosition = player.transform.position + offset;

            //Es manté la càmera dins els marges
            if (player.transform.position.x < playerController.xLeftBounds + xLeftMargin)
                targetPosition = new Vector3(playerController.xLeftBounds + xLeftMargin, targetPosition.y, targetPosition.z);
            else if (targetPosition.x > playerController.xRightBounds - xRightMargin)
                targetPosition = new Vector3(playerController.xRightBounds - xRightMargin, targetPosition.y, targetPosition.z);

            if (targetPosition.z < playerController.zLowerBounds + zLowerMargin)
                targetPosition = new Vector3(targetPosition.x, targetPosition.y, playerController.zLowerBounds + zLowerMargin);
            else if (targetPosition.z > playerController.zUpperBounds - zUpperMargin)
                targetPosition = new Vector3(targetPosition.x, targetPosition.y, playerController.zUpperBounds - zUpperMargin);
        }
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
    }

    public void SetIslandCamera(Vector3 position)
    {
        isFollowingPlayer = false;
        targetPosition = position;
    }

    public void SetFollowingCamera()
    {
        isFollowingPlayer = true;
    }
}
