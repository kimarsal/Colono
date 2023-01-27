using System.Collections;
using TMPro;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private Vector3 navigationOffset = new Vector3(0, 15, -8);
    private Vector3 islandOffset = new Vector3(0, 8, -5);
    private Vector3 offset;
    private PlayerController playerController;
    private GameManager gameManagerScript;
    private Vector3 targetPosition;
    private float speed = 25f;

    //Dist�ncia m�xima amb els marges
    private float minYIsland = 3f;
    private float maxYIsland = 25f;
    private float minX, maxX, minZ, maxZ;

    private float xLeftMarginPlayer = 10f;
    private float xRightMarginPlayer = 10f;
    private float zLowerMarginPlayer = -10f;
    private float zUpperMarginPlayer = 15f;

    private float xLeftMarginIsland = 40f;
    private float xRightMarginIsland = 40f;
    private float zLowerMarginIsland = 20f;
    private float zUpperMarginIsland = 50f;

    void Start()
    {
        offset = navigationOffset;
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        gameManagerScript = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();

        minX = playerController.xLeftBounds + xLeftMarginPlayer;
        maxX = playerController.xRightBounds - xRightMarginPlayer;
        minZ = playerController.zLowerBounds + zLowerMarginPlayer;
        maxZ = playerController.zUpperBounds - zUpperMarginPlayer;
    }

    void LateUpdate()
    {
        if (!gameManagerScript.isInIsland)
        {
            //Es mou la c�mera amb el jugador
            targetPosition = playerController.transform.position + offset;

            //Es mant� la c�mera dins els marges
            float x = Mathf.Clamp(minX, targetPosition.x, maxX);
            float y = targetPosition.y;
            float z = Mathf.Clamp(minZ, targetPosition.z, maxZ);

            transform.position = Vector3.MoveTowards(transform.position, new Vector3(x, y, z), speed * Time.deltaTime);
        }
        else
        {
            //Es mou la c�mera amb l'entrada
            targetPosition = transform.position + new Vector3(Input.GetAxis("Horizontal"), Input.mouseScrollDelta.y * -2, Input.GetAxis("Vertical")) * speed * Time.deltaTime;
            
            //Es mant� la c�mera dins els marges
            float x = Mathf.Clamp(minX, targetPosition.x, maxX);
            float y = Mathf.Clamp(minYIsland, targetPosition.y, maxYIsland);
            float z = Mathf.Clamp(minZ, targetPosition.z, maxZ);

            Vector3 positionChange = new Vector3(x, y, z) - transform.position;
            float time = positionChange.magnitude / speed;
            GetComponent<Rigidbody>().velocity = positionChange / Mathf.Max(time, Time.fixedDeltaTime);
        }
    }

    public void SetIslandCamera(Vector3 islandPosition)
    {
        minX = islandPosition.x - IslandGenerator.mapChunkSize / 2 + xLeftMarginIsland;
        maxX = islandPosition.x + IslandGenerator.mapChunkSize / 2 - xRightMarginIsland;
        minZ = islandPosition.z - IslandGenerator.mapChunkSize / 2 + zLowerMarginIsland;
        maxZ = islandPosition.z + IslandGenerator.mapChunkSize / 2 - zUpperMarginIsland;
        StartCoroutine(FastCameraSwipe(true));
    }

    public void ResetPlayerCamera()
    {
        minX = playerController.xLeftBounds + xLeftMarginPlayer;
        maxX = playerController.xRightBounds - xRightMarginPlayer;
        minZ = playerController.zLowerBounds + zLowerMarginPlayer;
        maxZ = playerController.zUpperBounds - zUpperMarginPlayer;
        StartCoroutine(FastCameraSwipe(false));
    }

    private IEnumerator FastCameraSwipe(bool intoIsland)
    {
        if (intoIsland)
        {
            float previousSpeed = speed;
            targetPosition = playerController.transform.position + islandOffset;
            float distance = Vector3.Distance(transform.position, targetPosition);
            speed = distance / 0.2f; //Fer zoom en 0.2 segons
            offset = islandOffset;
            yield return new WaitForSeconds(0.2f);
            gameManagerScript.isInIsland = true;
            speed = previousSpeed;
        }
        else
        {
            gameManagerScript.isInIsland = false;
            float previousSpeed = speed;
            targetPosition = playerController.transform.position + navigationOffset;
            float distance = Vector3.Distance(transform.position, targetPosition);
            speed = distance / 0.5f; //Tornar al lloc en 0.5 segons
            offset = navigationOffset;
            yield return new WaitForSeconds(0.5f);
            speed = previousSpeed;
        }
    }
}
