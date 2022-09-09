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
    public GameObject chooseBuildingButtons;
    public Button deleteBuildingButton;

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
        cameraScript.SetIslandCamera(nearbyIsland.position);
        boardIslandButton.gameObject.SetActive(false);
        leaveIslandButton.gameObject.SetActive(true);

        buildButton.gameObject.SetActive(true);

        nearbyIsland.islandScript.PlayerEntered();
    }

    public void IslandDefaultButtons()
    {
        leaveIslandButton.gameObject.SetActive(true);

        buildButton.gameObject.SetActive(true);
        chooseBuildingButtons.SetActive(false);
        deleteBuildingButton.gameObject.SetActive(false);

        createOrchardButton.gameObject.SetActive(false);
        createBarnButton.gameObject.SetActive(false);
        plantButton.gameObject.SetActive(false);
        clearPatchButton.gameObject.SetActive(false);

        removeZoneButton.gameObject.SetActive(false);
    }

    public void HideButtons()
    {
        IslandDefaultButtons();
        buildButton.gameObject.SetActive(false);
        leaveIslandButton.gameObject.SetActive(false);
    }

    public void Build()
    {
        buildButton.gameObject.SetActive(false);
        chooseBuildingButtons.SetActive(true);
    }

    public void ChooseBuilding(int type)
    {
        HideButtons();
        buildButton.gameObject.SetActive(false);
        nearbyIsland.islandCellScript.ChooseBuilding((BuildingScript.BuildingType)type);
    }

    public void SelectBuilding(BuildingScript.BuildingType type)
    {
        IslandDefaultButtons();
        buildButton.gameObject.SetActive(false);
        deleteBuildingButton.gameObject.SetActive(true);
    }

    public void DeleteBuilding()
    {
        nearbyIsland.islandCellScript.DeleteBuilding();
        IslandDefaultButtons();
    }

    public void SelectArea()
    {
        IslandDefaultButtons();
        buildButton.gameObject.SetActive(false);
        createOrchardButton.gameObject.SetActive(true);
        createBarnButton.gameObject.SetActive(true);
    }

    public void CreateZone(int type)
    {
        nearbyIsland.islandCellScript.CreateZone((ZoneScript.ZoneType) type);
        IslandDefaultButtons();
    }

    public void SelectZone(ZoneScript.ZoneType type)
    {
        IslandDefaultButtons();
        buildButton.gameObject.SetActive(false);
        removeZoneButton.gameObject.SetActive(true);
    }

    public void DeleteZone()
    {
        nearbyIsland.islandCellScript.DeleteZone();
        IslandDefaultButtons();
    }
    
    public void PatchSelected(bool isPatchEmpty)
    {
        buildButton.gameObject.SetActive(false);
        chooseBuildingButtons.SetActive(false);

        plantButton.gameObject.SetActive(true);
        if (!isPatchEmpty)
        {
            clearPatchButton.gameObject.SetActive(true);
        }
    }

    public void Plant()
    {
        nearbyIsland.islandCellScript.Plant();
    }

    public void ClearPatch()
    {
        nearbyIsland.islandCellScript.ClearPatch();
    }

    public void LeaveIsland()
    {
        isInIsland = false;
        cameraScript.ResetPlayerCamera();
        leaveIslandButton.gameObject.SetActive(false);
        boardIslandButton.gameObject.SetActive(true);
        buildButton.gameObject.SetActive(false);
        chooseBuildingButtons.SetActive(false);
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
