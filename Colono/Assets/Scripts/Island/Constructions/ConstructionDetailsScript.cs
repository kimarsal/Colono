using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConstructionDetailsScript : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI peasantsOnTheirWayText;
    public TextMeshProUGUI peasantNumText;
    public GameObject peasantButtons;
    public Button minusButton;
    public Button plusButton;

    public Button editGardenButton;
    public Button manageInventoryButton;

    public GameManager gameManager;
    public ConstructionScript constructionScript;

    public void SetConstructionDetails(ConstructionScript newConstructionScript)
    {
        if(constructionScript != null) constructionScript.constructionDetailsScript = null;
        constructionScript = newConstructionScript;
        constructionScript.constructionDetailsScript = this;

        switch (constructionScript.constructionType)
        {
            case ConstructionScript.ConstructionType.Building: title.text = ((BuildingScript)constructionScript).buildingType.ToString(); break;
            case ConstructionScript.ConstructionType.Enclosure: title.text = ((EnclosureScript)constructionScript).enclosureType.ToString(); break;
            case ConstructionScript.ConstructionType.Ship: title.text = constructionScript.constructionType.ToString(); break;
        }
        
        if (constructionScript.constructionType == ConstructionScript.ConstructionType.Building && ((BuildingScript)constructionScript).buildingType != BuildingScript.BuildingType.Mine)
        {
            peasantButtons.SetActive(false);
            if (((BuildingScript)constructionScript).buildingType == BuildingScript.BuildingType.Warehouse)
            {
                peasantNumText.enabled = false;
                manageInventoryButton.gameObject.SetActive(true);
            }
            else
            {
                peasantNumText.enabled = true;
                manageInventoryButton.gameObject.SetActive(false);
            }
        }
        else
        {
            peasantButtons.SetActive(true);
            peasantNumText.enabled = true;
            editGardenButton.gameObject.SetActive(constructionScript.constructionType == ConstructionScript.ConstructionType.Enclosure
                && ((EnclosureScript)constructionScript).enclosureType == EnclosureScript.EnclosureType.Garden);
            manageInventoryButton.gameObject.SetActive(constructionScript.constructionType == ConstructionScript.ConstructionType.Ship);
            UpdatePeasantNum();
        }
    }

    public void ManagePawns(bool adding)
    {
        if (constructionScript.constructionType == ConstructionScript.ConstructionType.Ship) gameManager.islandScript.npcManager.SendPeasantToIsland((ShipScript)constructionScript, adding);
        else gameManager.islandScript.npcManager.SendPeasantToArea(constructionScript, adding);
        UpdatePeasantNum();
    }

    public void UpdatePeasantNum()
    {
        if (constructionScript.peasantsOnTheirWay != 0)
        {
            peasantsOnTheirWayText.gameObject.SetActive(true);
            peasantsOnTheirWayText.text = (constructionScript.peasantsOnTheirWay > 0 ? "+" : "") + constructionScript.peasantsOnTheirWay;
        }
        else
        {
            peasantsOnTheirWayText.gameObject.SetActive(false);
        }
        peasantNumText.text = (constructionScript.peasantList.Count - constructionScript.peasantsOnTheirWay).ToString() + "/" + constructionScript.maxPeasants;
        minusButton.enabled = constructionScript.peasantList.Count > 0;
        plusButton.enabled = constructionScript.peasantList.Count < constructionScript.maxPeasants && gameManager.islandScript.npcManager.peasantList.Count > 0;
    }
}
