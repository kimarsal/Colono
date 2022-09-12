using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    private enum ButtonState { Idle, BuildingButtons, ZoneButtons, ConstructionDetails };

    private bool isPlayerNearIsland = false;
    public bool isInIsland = false;

    private ButtonState buttonState = ButtonState.Idle;
    public Animator canvasAnimator;
    private ConstructionScript constructionScript;
    public TextMeshProUGUI constructionTitle;
    public TextMeshProUGUI constructionPeasantNum;

    public Button boardIslandButton;
    public Button deleteZoneButton;
    public Button deleteBuildingButton;
    /*public Button plantButton;
    public Button clearPatchButton;*/


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
            canvasAnimator.Play("GetCloseToIsland");
            nearbyIsland.position = islandPosition;
            nearbyIsland.islandScript = islandScript;
            nearbyIsland.islandCellScript = islandScript.islandCellScript;
        }
    }

    public void BoardIsland()
    {
        nearbyIsland.islandScript.PlayerEntered();
        cameraScript.SetIslandCamera(nearbyIsland.position);
        canvasAnimator.Play("BoardIsland");

        buttonState = ButtonState.Idle;
    }

    public void HideButtons()
    {
        switch (buttonState)
        {
            case ButtonState.Idle: canvasAnimator.Play("HideButtons"); break;
            case ButtonState.BuildingButtons: canvasAnimator.Play("HideBuildingButtonsForSelection"); break;
            case ButtonState.ZoneButtons: canvasAnimator.Play("HideZoneButtonsForSelection"); break;
            case ButtonState.ConstructionDetails: canvasAnimator.Play("HideConstructionDetailsForSelection"); break;
        }

        buttonState = ButtonState.Idle;
    }

    public void ShowButtons()
    {
        canvasAnimator.Play("ShowButtons");

        buttonState = ButtonState.Idle;
    }

    public void Build()
    {
        if(buttonState == ButtonState.Idle)
        {
            canvasAnimator.Play("ShowBuildingButtons");
            buttonState = ButtonState.BuildingButtons;
        }
        else
        {
            canvasAnimator.Play("HideBuildingButtons");
            buttonState = ButtonState.Idle;
        }
    }

    public void ChooseBuilding(int type)
    {
        nearbyIsland.islandCellScript.ChooseBuilding((BuildingScript.BuildingType)type);
        canvasAnimator.Play("HideBuildingButtonsForSelection");
    }

    public void SelectBuilding(BuildingScript buildingScript, bool wasJustCreated)
    {
        deleteZoneButton.gameObject.SetActive(false);
        deleteBuildingButton.gameObject.SetActive(true);
        constructionTitle.text = buildingScript.type.ToString();
        constructionScript = buildingScript;
        SetConstructionDetails();

        if (wasJustCreated) canvasAnimator.Play("ShowConstructionDetailsAfterCreateBuilding");
        else
        {
            switch (buttonState)
            {
                case ButtonState.Idle: canvasAnimator.Play("ShowConstructionDetails"); break;
                case ButtonState.BuildingButtons: canvasAnimator.Play("ShowConstructionDetailsFromBuilding"); break;
                case ButtonState.ZoneButtons: canvasAnimator.Play("ShowConstructionDetailsFromZone"); break;
            }
            
        }

        buttonState = ButtonState.ConstructionDetails;
    }

    public void DeleteBuilding()
    {
        nearbyIsland.islandCellScript.DeleteBuilding();
        canvasAnimator.Play("HideConstructionDetails");

        buttonState = ButtonState.Idle;
    }

    public void SelectArea()
    {
        canvasAnimator.Play("ShowZoneButtons");

        buttonState = ButtonState.ZoneButtons;
    }

    public void UnselectArea()
    {
        nearbyIsland.islandCellScript.DestroyAllCells();
        canvasAnimator.Play("HideZoneButtons");

        buttonState = ButtonState.Idle;
    }

    public void CreateZone(int type)
    {
        nearbyIsland.islandCellScript.CreateZone((ZoneScript.ZoneType) type);
    }

    public void SelectZone(ZoneScript zoneScript, bool wasJustCreated)
    {
        deleteZoneButton.gameObject.SetActive(true);
        deleteBuildingButton.gameObject.SetActive(false);
        constructionTitle.text = zoneScript.type.ToString();
        constructionScript = zoneScript;
        SetConstructionDetails();

        if (wasJustCreated) canvasAnimator.Play("ShowConstructionDetailsAfterCreateZone");
        else
        {
            switch (buttonState)
            {
                case ButtonState.Idle: canvasAnimator.Play("ShowConstructionDetails"); break;
                case ButtonState.BuildingButtons: canvasAnimator.Play("ShowConstructionDetailsFromBuilding"); break;
                case ButtonState.ZoneButtons: canvasAnimator.Play("ShowConstructionDetailsFromZone"); break;
            }

        }

        buttonState = ButtonState.ConstructionDetails;
    }

    public void DeleteZone()
    {
        nearbyIsland.islandCellScript.DeleteZone();
        canvasAnimator.Play("HideConstructionDetails");

        buttonState = ButtonState.Idle;
    }

    public void HideConstructionDetails()
    {
        canvasAnimator.Play("HideConstructionDetails");
        nearbyIsland.islandCellScript.DestroyAllCells();

        buttonState = ButtonState.Idle;
    }

    private void SetConstructionDetails()
    {
        UpdatePeasantNumText();
    }

    public void ManagePawns(bool adding)
    {
        if (adding && nearbyIsland.islandScript.GetAvailablePeasants() > 0 || !adding && constructionScript.peasantNum > 0)
        {
            nearbyIsland.islandScript.SendPeasantToArea(constructionScript, adding);
            UpdatePeasantNumText();
        }
    }

    public void UpdatePeasantNumText()
    {
        constructionPeasantNum.text = constructionScript.peasantNum.ToString();
    }

    public void PatchSelected(bool isPatchEmpty)
    {
        /*buildButton.gameObject.SetActive(false);
        chooseBuildingButtons.SetActive(false);

        plantButton.gameObject.SetActive(true);
        if (!isPatchEmpty)
        {
            clearPatchButton.gameObject.SetActive(true);
        }*/
    }

    public void Plant()
    {
        //nearbyIsland.islandCellScript.Plant();
    }

    public void ClearPatch()
    {
        //nearbyIsland.islandCellScript.ClearPatch();
    }

    public void LeaveIsland()
    {
        isInIsland = false;
        cameraScript.ResetPlayerCamera();
        canvasAnimator.Play("LeaveIsland");

        nearbyIsland.islandScript.PLayerLeft();
        nearbyIsland.islandCellScript.DestroyAllCells();
    }

    public void PlayerIsFarFromIsland()
    {
        canvasAnimator.Play("GetFarFromIsland");
        isPlayerNearIsland = false;
    }
}
