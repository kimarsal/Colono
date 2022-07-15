using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IslandManager : MonoBehaviour
{
    private List<GameObject> islands = new List<GameObject>();
    public GameObject islandPrefab;
    public GameObject player;
    private MapController mapController;
    public float spotDistance = 10f;
    public float islandDistance = 20f;
    public float playerDistance = 30f;
    public Button boardIslandButton;
    public Button leaveIslandButton;
    public CameraScript cameraScript;

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
        foreach (GameObject island in islands)
        {
            if (Vector3.Distance(island.transform.position, pos) < islandDistance)
            {
                ok = false;
                break;
            }
        }
        if (ok)
        {
            /*GameObject newIsland = Instantiate(islandPrefab, pos, islandPrefab.transform.rotation);
            mapController.FollowIsland(newIsland);
            islands.Add(newIsland);*/
        }
        
    }

    public void PlayerIsNearIsland(Vector3 islandPosition, string islandName, bool hasBeenDiscovered)
    {
        boardIslandButton.GetComponentInChildren<TextMeshProUGUI>().text = "Board " + (islandName == "" ? "this island" : islandName);
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
