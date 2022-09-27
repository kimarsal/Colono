using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    private ShipScript shipScript;

    private enum ButtonState { Idle, BuildingButtons, EnclosureButtons, ConstructionDetails };

    private bool isPlayerNearIsland = false;
    public bool isInIsland = false;

    private ButtonState buttonState = ButtonState.Idle;
    public Animator canvasAnimator;
    public Animator buildingButtonsAnimator;
    public Animator enclosureButtonsAnimator;
    public Animator constructionDetailsAnimator;
    private ConstructionScript constructionScript;
    public TextMeshProUGUI constructionTitle;
    public TextMeshProUGUI constructionPeasantNum;

    public Button boardIslandButton;
    public GameObject peasantButtons;
    public Button peasantMinusButton;
    public Button peasantPlusButton;
    public Button removeEnclosureButton;
    public Button removeBuildingButton;
    /*public Button plantButton;
    public Button clearPatchButton;*/
    
    private IslandScript islandScript;

    private CameraScript cameraScript;

    private void Start()
    {
        cameraScript = GameObject.Find("MainCamera").GetComponent<CameraScript>();
        shipScript = GameObject.FindGameObjectWithTag("Player").GetComponent<ShipScript>();
    }

    public void PlayerIsNearIsland(IslandScript islandScript)
    {
        if (!isPlayerNearIsland)
        {
            isPlayerNearIsland = true;
            boardIslandButton.GetComponentInChildren<TextMeshProUGUI>().text = "Dock onto the island";
            canvasAnimator.Play("GetCloseToIsland");
            this.islandScript = islandScript;
        }
    }

    public void BoardIsland()
    {
        islandScript.PlayerEntered();
        cameraScript.SetIslandCamera(islandScript.transform.position);
        canvasAnimator.Play("BoardIsland");
        buildingButtonsAnimator.Play("ShowBuildingButtons");

        buttonState = ButtonState.Idle;
    }

    public void HideButtons()
    {
        canvasAnimator.Play("HideLeaveIslandButton");
        switch (buttonState)
        {
            case ButtonState.Idle: buildingButtonsAnimator.Play("HideBuildingButtons"); break;
            case ButtonState.BuildingButtons: buildingButtonsAnimator.Play("HideAllBuildingButtons"); break;
            case ButtonState.EnclosureButtons: enclosureButtonsAnimator.Play("HideEnclosureButtons"); break;
            case ButtonState.ConstructionDetails: constructionDetailsAnimator.Play("HideConstructionDetails"); break;
        }

        buttonState = ButtonState.Idle;
    }

    public void ShowButtons()
    {
        canvasAnimator.Play("ShowLeaveIslandButton");
        buildingButtonsAnimator.Play("ShowBuildingButtons");

        buttonState = ButtonState.Idle;
    }

    public void SelectShip()
    {
        removeEnclosureButton.gameObject.SetActive(false);
        removeBuildingButton.gameObject.SetActive(false);
        constructionTitle.text = "Ship";
        constructionScript = shipScript;

        SetConstructionDetails();
    }

    public void Build()
    {
        if(buttonState == ButtonState.Idle)
        {
            buildingButtonsAnimator.Play("ShowBuildingOptionsButtons");
            buttonState = ButtonState.BuildingButtons;
        }
        else
        {
            buildingButtonsAnimator.Play("HideBuildingOptionsButtons");
            buttonState = ButtonState.Idle;
        }
    }

    public void ChooseBuilding(int type)
    {
        islandScript.islandCellScript.ChooseBuilding((BuildingScript.BuildingType)type);
        canvasAnimator.Play("HideLeaveIslandButton");
        buildingButtonsAnimator.Play("HideAllBuildingButtons");
    }

    public void SelectBuilding(BuildingScript buildingScript)
    {
        removeEnclosureButton.gameObject.SetActive(false);
        removeBuildingButton.gameObject.SetActive(true);
        constructionTitle.text = buildingScript.type.ToString();
        constructionScript = buildingScript;

        SetConstructionDetails();
    }

    public void RemoveBuilding()
    {
        islandScript.npcManager.SendPeasantsBack(constructionScript);
        islandScript.islandCellScript.RemoveBuilding();
        canvasAnimator.Play("ShowLeaveIslandButton");
        buildingButtonsAnimator.Play("ShowBuildingButtons");
        constructionDetailsAnimator.Play("HideConstructionDetails");

        buttonState = ButtonState.Idle;
    }

    public void SelectArea()
    {
        canvasAnimator.Play("ShowLeaveIslandButton");
        enclosureButtonsAnimator.Play("ShowEnclosureButtons");

        buttonState = ButtonState.EnclosureButtons;
    }

    public void UnselectArea()
    {
        islandScript.islandCellScript.DestroyAllCells();
        enclosureButtonsAnimator.Play("HideEnclosureButtons");
        buildingButtonsAnimator.Play("ShowBuildingButtons");

        buttonState = ButtonState.Idle;
    }

    public void CreateEnclosure(int type)
    {
        islandScript.islandCellScript.CreateEnclosure((EnclosureScript.EnclosureType) type);
    }

    public void SelectEnclosure(EnclosureScript enclosureScript)
    {
        removeEnclosureButton.gameObject.SetActive(true);
        removeBuildingButton.gameObject.SetActive(false);
        constructionTitle.text = enclosureScript.type.ToString();
        constructionScript = enclosureScript;

        SetConstructionDetails();
    }

    public void RemoveEnclosure()
    {
        islandScript.npcManager.SendPeasantsBack(constructionScript);
        islandScript.islandCellScript.RemoveEnclosure();
        canvasAnimator.Play("ShowLeaveIslandButton");
        buildingButtonsAnimator.Play("ShowBuildingButtons");
        constructionDetailsAnimator.Play("HideConstructionDetails");

        buttonState = ButtonState.Idle;
    }

    public void HideConstructionDetails()
    {
        canvasAnimator.Play("ShowLeaveIslandButton");
        buildingButtonsAnimator.Play("ShowBuildingButtons");
        constructionDetailsAnimator.Play("HideConstructionDetails");
        islandScript.islandCellScript.DestroyAllCells();

        buttonState = ButtonState.Idle;
    }

    private void SetConstructionDetails()
    {
        if(constructionScript.isBuilding && ((BuildingScript)constructionScript).type != BuildingScript.BuildingType.Mine)
        {
            peasantButtons.SetActive(false);
        }
        else
        {
            peasantButtons.SetActive(true);
            UpdatePeasantNum();
        }

        canvasAnimator.Play("HideLeaveIslandButton");
        switch (buttonState)
        {
            case ButtonState.Idle: buildingButtonsAnimator.Play("HideBuildingButtons"); break;
            case ButtonState.BuildingButtons: buildingButtonsAnimator.Play("HideAllBuildingButtons"); break;
            case ButtonState.EnclosureButtons: enclosureButtonsAnimator.Play("HideEnclosureButtons"); break;
        }
        constructionDetailsAnimator.Play("ShowConstructionDetails");

        buttonState = ButtonState.ConstructionDetails;
    }

    public void ManagePawns(bool adding)
    {
        if (adding && islandScript.npcManager.GetAvailablePeasants() > 0 || !adding && constructionScript.numPeasants > 0)
        {
            if (constructionScript == shipScript) islandScript.npcManager.SendPeasantToIsland(shipScript, adding);
            else islandScript.npcManager.SendPeasantToArea(constructionScript, adding);
            UpdatePeasantNum();
        }
    }

    public void UpdatePeasantNum()
    {
        constructionPeasantNum.text = constructionScript.numPeasants.ToString();
        peasantMinusButton.enabled = constructionScript.numPeasants > 0;
        peasantPlusButton.enabled = islandScript.npcManager.GetAvailablePeasants() > 0;
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
        switch (buttonState)
        {
            case ButtonState.Idle: buildingButtonsAnimator.Play("HideBuildingButtons"); break;
            case ButtonState.BuildingButtons: buildingButtonsAnimator.Play("HideAllBuildingButtons"); break;
            case ButtonState.EnclosureButtons: enclosureButtonsAnimator.Play("HideEnclosureButtons"); break;
        }

        islandScript.PLayerLeft();
        islandScript.islandCellScript.DestroyAllCells();
    }

    public void PlayerIsFarFromIsland()
    {
        canvasAnimator.Play("GetFarFromIsland");
        isPlayerNearIsland = false;
    }
}
