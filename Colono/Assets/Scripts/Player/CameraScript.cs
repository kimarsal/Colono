using System.Collections;
using TMPro;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public static CameraScript Instance { get; private set; }

    private Vector3 navigationOffset = new Vector3(0, 15, -8);
    private Vector3 islandOffset = new Vector3(0, 8, -5);
    private Vector3 offset;
    private PlayerController playerController;
    private Vector3 targetPosition;
    private float speed = 25f;
    public bool canMove = true;

    //Distància màxima amb els marges
    private float minYIsland = 3f;
    private float maxYIsland = 25f;
    private float minX, maxX, minZ, maxZ;

    private float xLeftMarginPlayer = 10f;
    private float xRightMarginPlayer = 10f;
    private float zLowerMarginPlayer = -10f;
    private float zUpperMarginPlayer = 15f;

    private float xLeftMarginIsland = 10f;
    private float xRightMarginIsland = 10f;
    private float zLowerMarginIsland = 5f;
    private float zUpperMarginIsland = 15f;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        offset = navigationOffset;
        playerController = ShipScript.Instance.GetComponent<PlayerController>();

        minX = playerController.xLeftBounds + xLeftMarginPlayer;
        maxX = playerController.xRightBounds - xRightMarginPlayer;
        minZ = playerController.zLowerBounds + zLowerMarginPlayer;
        maxZ = playerController.zUpperBounds - zUpperMarginPlayer;
    }

    void LateUpdate()
    {
        if (!canMove) return;

        if (!GameManager.Instance.isInIsland)
        {
            //Es mou la càmera amb el jugador
            targetPosition = playerController.transform.position + offset;

            //Es manté la càmera dins els marges
            float x = Mathf.Clamp(targetPosition.x, minX, maxX);
            float y = targetPosition.y;
            float z = Mathf.Clamp(targetPosition.z, minZ, maxZ);

            transform.position = Vector3.MoveTowards(transform.position, new Vector3(x, y, z), speed * Time.deltaTime);
        }
        else
        {
            //Es mou la càmera amb l'entrada
            targetPosition = transform.position + new Vector3(Input.GetAxis("Horizontal"), Input.mouseScrollDelta.y * -2, Input.GetAxis("Vertical")) * speed * Time.deltaTime;
            
            //Es manté la càmera dins els marges
            float x = Mathf.Clamp(targetPosition.x, minX, maxX);
            float y = Mathf.Clamp(targetPosition.y, minYIsland, maxYIsland);
            float z = Mathf.Clamp(targetPosition.z, minZ, maxZ);

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
        SetCameraToConstruction(ShipScript.Instance);
    }

    public void ResetPlayerCamera()
    {
        minX = playerController.xLeftBounds + xLeftMarginPlayer;
        maxX = playerController.xRightBounds - xRightMarginPlayer;
        minZ = playerController.zLowerBounds + zLowerMarginPlayer;
        maxZ = playerController.zUpperBounds - zUpperMarginPlayer;
        StartCoroutine(FastCameraSwipe(null));
    }

    public void SetCameraToConstruction(ConstructionScript constructionScript)
    {
        StartCoroutine(FastCameraSwipe(constructionScript));
    }

    private IEnumerator FastCameraSwipe(ConstructionScript constructionScript)
    {
        if (constructionScript != null)
        {
            float previousSpeed = speed;
            targetPosition = constructionScript.transform.position + islandOffset;
            float distance = Vector3.Distance(transform.position, targetPosition);
            speed = distance / 0.2f; //Fer zoom en 0.2 segons
            offset = islandOffset;
            yield return new WaitForSeconds(0.2f);
            GameManager.Instance.isInIsland = true;
            speed = previousSpeed;
        }
        else
        {
            GameManager.Instance.isInIsland = false;
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
