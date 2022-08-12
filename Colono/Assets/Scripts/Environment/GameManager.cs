using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    private bool isPlayerNearIsland = false;
    public bool isInIsland = false;
    public Button boardIslandButton;
    public Button leaveIslandButton;

    public Button buildButton;
    public GameObject selectBuildingButtons;

    public Button createOrchardButton;
    public Button createBarnButton;
    public Button plantButton;
    public Button clearPatchButton;
    public Button removeZoneButton;


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

        buildButton.gameObject.SetActive(true);

        nearbyIsland.islandScript.PlayerEntered();
    }

    public void Build()
    {
        buildButton.gameObject.SetActive(false);
        selectBuildingButtons.SetActive(true);
    }

    public void SelectBuilding(int type)
    {
        selectBuildingButtons.SetActive(false);

        nearbyIsland.islandCellScript.SelectBuilding((BuildingScript.BuildingType)type);
    }

    public void AreaSelected()
    {
        buildButton.gameObject.SetActive(false);
        selectBuildingButtons.SetActive(false);

        createOrchardButton.gameObject.SetActive(true);
        createOrchardButton.onClick.AddListener(nearbyIsland.islandCellScript.CreateOrchard);
        createOrchardButton.onClick.AddListener(AreaUnselected);

        createBarnButton.gameObject.SetActive(true);
        createBarnButton.onClick.AddListener(nearbyIsland.islandCellScript.CreateBarn);
        createBarnButton.onClick.AddListener(AreaUnselected);
    }

    public void AreaUnselected()
    {
        buildButton.gameObject.SetActive(true);

        createOrchardButton.gameObject.SetActive(false);
        createOrchardButton.onClick.RemoveAllListeners();
        createBarnButton.gameObject.SetActive(false);
        createBarnButton.onClick.RemoveAllListeners();
    }

    public void ZoneSelected(ZoneScript.ZoneType type)
    {
        buildButton.gameObject.SetActive(false);
        selectBuildingButtons.SetActive(false);

        removeZoneButton.gameObject.SetActive(true);
        if (type == ZoneScript.ZoneType.Orchard)
        {
            removeZoneButton.onClick.AddListener(nearbyIsland.islandCellScript.DeleteOrchard);
        }
        else
        {
            removeZoneButton.onClick.AddListener(nearbyIsland.islandCellScript.DeleteBarn);
        }
        removeZoneButton.onClick.AddListener(ZoneUnselected);
    }

    public void ZoneUnselected()
    {
        buildButton.gameObject.SetActive(true);

        removeZoneButton.gameObject.SetActive(false);
        removeZoneButton.onClick.RemoveAllListeners();
    }

    public void PatchSelected(bool isPatchEmpty)
    {
        buildButton.gameObject.SetActive(false);
        selectBuildingButtons.SetActive(false);

        plantButton.gameObject.SetActive(true);
        plantButton.onClick.AddListener(nearbyIsland.islandCellScript.Plant);
        plantButton.onClick.AddListener(PatchUnselected);

        if (!isPatchEmpty)
        {
            clearPatchButton.gameObject.SetActive(true);
            clearPatchButton.onClick.AddListener(nearbyIsland.islandCellScript.ClearPatch);
            clearPatchButton.onClick.AddListener(PatchUnselected);
        }
    }

    public void PatchUnselected()
    {
        buildButton.gameObject.SetActive(true);

        plantButton.gameObject.SetActive(false);
        plantButton.onClick.RemoveAllListeners();
        clearPatchButton.gameObject.SetActive(false);
        clearPatchButton.onClick.RemoveAllListeners();
    }

    public void LeaveIsland()
    {
        isInIsland = false;
        cameraScript.ResetPlayerCamera();
        leaveIslandButton.gameObject.SetActive(false);
        boardIslandButton.gameObject.SetActive(true);
        buildButton.gameObject.SetActive(false);
        selectBuildingButtons.SetActive(false);
        createOrchardButton.gameObject.SetActive(false);
        createBarnButton.gameObject.SetActive(false);

        nearbyIsland.islandScript.PLayerLeft();
        nearbyIsland.islandCellScript.DestroyAllCells();
    }

    public void PlayerIsFarFromIsland()
    {
        boardIslandButton.gameObject.SetActive(false);
        isPlayerNearIsland = false;
    }
}
