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
    public Material hoverMaterial;
    public Material selectingMaterial;
    public Material selectingFirstCellMaterial;
    public Material selectedMaterial;
    public Material invalidSelectionMaterial;
    public Material selectedHoverMaterial;
    private bool isPlayerNearIsland = false;

    private struct IslandType
    {
        public Vector3 position;
        public string name;
        public bool hasBeenDiscovered;
    }

    private IslandType nearbyIsland;

    private CameraScript cameraScript;

    private void Start()
    {
        cameraScript = GameObject.Find("MainCamera").GetComponent<CameraScript>();
    }

    public void PlayerIsNearIsland(Vector3 islandPosition, string islandName, bool hasBeenDiscovered)
    {
        if (!isPlayerNearIsland)
        {
            isPlayerNearIsland = true;
            boardIslandButton.GetComponentInChildren<TextMeshProUGUI>().text = "Dock onto the island";
            boardIslandButton.gameObject.SetActive(true);
            nearbyIsland.position = islandPosition;
            nearbyIsland.name = islandName;
            nearbyIsland.hasBeenDiscovered = hasBeenDiscovered;
        }
    }

    public void BoardIsland()
    {
        isInIsland = true;
        cameraScript.SetIslandCamera(nearbyIsland.position);
        boardIslandButton.gameObject.SetActive(false);
        leaveIslandButton.gameObject.SetActive(true);
    }

    public void LeaveIsland()
    {
        isInIsland = false;
        cameraScript.ResetPlayerCamera();
        leaveIslandButton.gameObject.SetActive(false);
        boardIslandButton.gameObject.SetActive(true);
    }

    public void PlayerLeftIsland()
    {
        boardIslandButton.gameObject.SetActive(false);
        isPlayerNearIsland = false;
    }
}
