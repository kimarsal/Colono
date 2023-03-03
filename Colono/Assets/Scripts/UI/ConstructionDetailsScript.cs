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

        title.text = constructionScript.title;
        if (constructionScript.canManagePeasants)
        {
            peasantButtons.SetActive(true);
            peasantsOnTheirWayText.enabled = true;
        }
        else
        {
            peasantButtons.SetActive(false);
            peasantsOnTheirWayText.enabled = false;
        }
        UpdatePeasantNum();

        removeConstructionButton.gameObject.SetActive(constructionScript.canBeRemoved);
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
            peasantsOnTheirWayText.enabled = true;
            peasantsOnTheirWayText.text = (constructionScript.peasantsOnTheirWay > 0 ? "+" : "") + constructionScript.peasantsOnTheirWay;
        }
        else
        {
            peasantsOnTheirWayText.enabled = false;
        }

        int peasantCount = constructionScript.peasantCount;
        peasantNumText.text = (peasantCount - constructionScript.peasantsOnTheirWay).ToString() + "/" + constructionScript.maxPeasants;
        minusButton.interactable = peasantCount > 0;
        plusButton.interactable = peasantCount < constructionScript.maxPeasants;
    }

}
