using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public bool isInIsland = false;
    public Button boardIslandButton;
    public Button leaveIslandButton;
    public Button createZoneButton;
    public Button removeZoneButton;
    private bool isPlayerNearIsland = false;

    private struct IslandType
    {
        public Vector3 position;
        public IslandScript islandScript;
        public IslandCellScript islandCellScript;
    }

    private IslandType nearbyIsland;

    private CameraScript cameraScript;

    private void Start()
    {
        cameraScript = GameObject.Find("MainCamera").GetComponent<CameraScript>();
    }

    public void PlayerIsNearIsland(Vector3 islandPosition, IslandScript islandScript)
    {
        if (!isPlayerNearIsland)
        {
            isPlayerNearIsland = true;
            boardIslandButton.GetComponentInChildren<TextMeshProUGUI>().text = "Dock onto the island";
            boardIslandButton.gameObject.SetActive(true);
            nearbyIsland.position = islandPosition;
            nearbyIsland.islandScript = islandScript;
            nearbyIsland.islandCellScript = islandScript.islandCellScript;
        }
    }

    public void BoardIsland()
    {
        isInIsland = true;
        cameraScript.SetIslandCamera(nearbyIsland.position);
        boardIslandButton.gameObject.SetActive(false);
        leaveIslandButton.gameObject.SetActive(true);

        nearbyIsland.islandScript.PlayerEntered();
    }

    public void AreaSelected()
    {
        createZoneButton.gameObject.SetActive(true);
        createZoneButton.onClick.AddListener(nearbyIsland.islandCellScript.CreateZone);
        createZoneButton.onClick.AddListener(AreaUnelected);
    }

    public void AreaUnelected()
    {
        createZoneButton.gameObject.SetActive(false);
        createZoneButton.onClick.RemoveAllListeners();
        ZoneUnselected();
    }

    public void ZoneSelected()
    {
        removeZoneButton.gameObject.SetActive(true);
        removeZoneButton.onClick.AddListener(nearbyIsland.islandCellScript.DeleteZone);
        removeZoneButton.onClick.AddListener(ZoneUnselected);
    }

    public void ZoneUnselected()
    {
        removeZoneButton.gameObject.SetActive(false);
        removeZoneButton.onClick.RemoveAllListeners();
    }

    public void LeaveIsland()
    {
        isInIsland = false;
        cameraScript.ResetPlayerCamera();
        leaveIslandButton.gameObject.SetActive(false);
        boardIslandButton.gameObject.SetActive(true);
        createZoneButton.gameObject.SetActive(false);

        nearbyIsland.islandScript.PLayerLeft();
    }

    public void PlayerIsFarFromIsland()
    {
        boardIslandButton.gameObject.SetActive(false);
        isPlayerNearIsland = false;
    }
}
