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

    public Button removeConstructionButton;

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

        if (constructionScript.constructionType == ConstructionScript.ConstructionType.Building
            && ((BuildingScript)constructionScript).buildingType != BuildingScript.BuildingType.Mine)
        {
            peasantButtons.SetActive(false);
            peasantNumText.enabled = ((BuildingScript)constructionScript).buildingType != BuildingScript.BuildingType.Warehouse;
        }
        else
        {
            peasantButtons.SetActive(true);
            peasantNumText.enabled = true;
            UpdatePeasantNum();
        }
        removeConstructionButton.gameObject.SetActive(constructionScript.constructionType != ConstructionScript.ConstructionType.Ship);
    }

    public void EditConstruction()
    {
        constructionScript.EditConstruction();
    }

    public void ManagePeasants(bool adding)
    {
        if (constructionScript.islandScript.ManagePeasants(constructionScript, adding))
        {
            UpdatePeasantNum();
        }
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

        if(constructionScript.constructionType == ConstructionScript.ConstructionType.Ship)
        {
            ShipScript shipScript = (ShipScript)constructionScript;
            peasantNumText.text = shipScript.peasantInfoList.Count.ToString() + "/" + constructionScript.maxPeasants;
            minusButton.interactable = shipScript.peasantInfoList.Count > 0;
            plusButton.interactable = shipScript.peasantInfoList.Count < shipScript.maxPeasants;
        }
        else
        {
            peasantNumText.text = (constructionScript.peasantList.Count - constructionScript.peasantsOnTheirWay).ToString() + "/" + constructionScript.maxPeasants;
            minusButton.interactable = constructionScript.peasantList.Count > 0;
            plusButton.interactable = constructionScript.peasantList.Count < constructionScript.maxPeasants;
        }
    }

}
