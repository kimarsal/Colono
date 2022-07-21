using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IslandManager : MonoBehaviour
{
    private List<GameObject> islands = new List<GameObject>();
    public GameObject player;
    public GameObject islandComponentsPrefab;
    private MapController mapController;
    public float spotDistance = 10f;
    public float islandDistance = 20f;
    public float playerDistance = 30f;
    public Button boardIslandButton;
    public Button leaveIslandButton;
    private CameraScript cameraScript;

    private struct IslandType
    {
        public Vector3 position;
        public string name;
        public bool hasBeenDiscovered;
    }

    private IslandType nearbyIsland;

    void Start()
    {
        mapController = player.GetComponent<MapController>();
        cameraScript = GameObject.Find("MainCamera").GetComponent<CameraScript>();

        GenerateIsland(player.transform);
    }

    void Update()
    {
        /*foreach(GameObject island in islands)
        {
            if(Vector3.Distance(island.transform.position, player.transform.position) < playerDistance)
            {
                island.SetActive(true);
            }
            else
            {
                island.SetActive(false);
            }
        }*/
    }

    public void GenerateIsland(Transform player)
    {
        float angle = (player.transform.rotation.eulerAngles.y % 360) * Mathf.Deg2Rad;
        float x = Mathf.Sin(angle);
        float z = Mathf.Cos(angle);
        Vector3 pos = player.position + new Vector3(x, 0, z) * spotDistance;
        bool ok = true;
        /*foreach (GameObject island in islands)
        {
            if (Vector3.Distance(island.transform.position, pos) < islandDistance)
            {
                ok = false;
                break;
            }
        }*/
        if (ok)
        {
            //GameObject newIsland = Instantiate(islandPrefab, pos, islandPrefab.transform.rotation);
            GameObject newIsland = GetComponent<IslandGenerator>().GenerateIsland(new Vector2(pos.x, /*pos.z*/200));

            newIsland.AddComponent<IslandScript>();
            newIsland.GetComponent<IslandScript>().islandManagerScript = this;
            mapController.FollowIsland(newIsland);
            islands.Add(newIsland);
        }
        
    }

    public void PlayerIsNearIsland(Vector3 islandPosition, string islandName, bool hasBeenDiscovered)
    {
        boardIslandButton.GetComponentInChildren<TextMeshProUGUI>().text = "Dock onto " + (islandName == "" ? "the island" : islandName);
        boardIslandButton.gameObject.SetActive(true);
        nearbyIsland.position = islandPosition;
        nearbyIsland.name = islandName;
        nearbyIsland.hasBeenDiscovered = hasBeenDiscovered;
    }

    public void BoardIsland()
    {
        cameraScript.SetIslandCamera(nearbyIsland.position + new Vector3(0, 20, -15));
        boardIslandButton.gameObject.SetActive(false);
        leaveIslandButton.gameObject.SetActive(true);
    }

    public void LeaveIsland()
    {
        cameraScript.SetFollowingCamera();
        leaveIslandButton.gameObject.SetActive(false);
        boardIslandButton.gameObject.SetActive(true);
    }

    public void PlayerLeftIsland()
    {
        boardIslandButton.gameObject.SetActive(false);
    }
}
