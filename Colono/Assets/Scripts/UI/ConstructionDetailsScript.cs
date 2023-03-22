using System.Collections;
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

    public Button editConstructionButton;
    public Button removeConstructionButton;

    public ConstructionScript constructionScript;

    public enum ConstructionDetailsState { Closed, Details, Editor };
    public ConstructionDetailsState state;
    private RectTransform rectTransform;
    private RectTransform canvasRectTransform;
    private EditorScript editorScript;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasRectTransform = CanvasScript.Instance.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0, -canvasRectTransform.rect.height);
        rectTransform.anchoredPosition = new Vector2(0, -canvasRectTransform.rect.height);
    }

    public void ShowDetails()
    {
        state = ConstructionDetailsState.Details;
        StartCoroutine(ChangeStateCoroutine());
    }

    public void EditConstruction()
    {
        if(state == ConstructionDetailsState.Details)
        {
            editorScript = constructionScript.editorScript;
            editorScript.gameObject.SetActive(true);
            editorScript.SetEditor(constructionScript);
        }
        else
        {
            StartCoroutine(HideEditor());
        }

        state = state == ConstructionDetailsState.Details ? ConstructionDetailsState.Editor : ConstructionDetailsState.Details;
        StartCoroutine(ChangeStateCoroutine());
    }

    private IEnumerator HideEditor()
    {
        yield return new WaitForSeconds(0.3f);
        if(constructionScript.editorScript != null) constructionScript.editorScript.gameObject.SetActive(false);
    }

    public void HideDetails()
    {
        state = ConstructionDetailsState.Closed;
        StartCoroutine(ChangeStateCoroutine());
        StartCoroutine(HideEditor());
    }

    private IEnumerator ChangeStateCoroutine()
    {
        float duration = 0.3f;
        float currentTime = 0;
        float startPos = rectTransform.anchoredPosition.y;
        float targetPos = 0;
        switch (state)
        {
            case ConstructionDetailsState.Closed: targetPos = -canvasRectTransform.rect.height; break;
            case ConstructionDetailsState.Details: targetPos = -canvasRectTransform.rect.height + 100; break;
        }

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            rectTransform.anchoredPosition = new Vector2(0, Mathf.Lerp(startPos, targetPos, currentTime / duration));
            yield return null;
        }
        yield break;
    }

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

        removeConstructionButton.enabled = constructionScript.canBeRemoved;
        editConstructionButton.enabled = constructionScript.editorScript != null;
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

    public void UpdateUpgradeButton()
    {
        if (editorScript != null) editorScript.UpdateUpgradeButton();
    }

    public void UpgradeConstruction()
    {
        constructionScript.islandScript.UseResource(ResourceScript.ResourceType.Material, (int)ResourceScript.MaterialType.Gem, constructionScript.level);
        constructionScript.level++;
        editorScript.UpdateUpgradeButton();
    }

}
