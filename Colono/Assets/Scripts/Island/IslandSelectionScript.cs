using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandSelectionScript : MonoBehaviour
{
    private PeasantScript hoveredPeasant;
    public PeasantScript selectedPeasant;
    private ConstructionScript hoveredConstruction;
    public ConstructionScript selectedConstruction;

    private void Update()
    {
        RaycastHit raycastHit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out raycastHit, 100f))
        {
            return;
        }

        if (raycastHit.transform.gameObject.CompareTag("NPC"))
        {
            PeasantScript peasantScript = raycastHit.transform.GetComponent<PeasantScript>();
            if (peasantScript != hoveredPeasant)
            {
                if (hoveredPeasant != null && hoveredPeasant != selectedPeasant) hoveredPeasant.outline.enabled = false;
                hoveredPeasant = peasantScript;
                peasantScript.outline.enabled = true;
            }
            if (Input.GetMouseButtonDown(0))
            {
                SelectPeasant(peasantScript);
            }
        }
        else
        {
            if (hoveredPeasant != null && hoveredPeasant != selectedPeasant)
            {
                hoveredPeasant.outline.enabled = false;
                hoveredPeasant = null;
            }
        }

        if (raycastHit.transform.gameObject.CompareTag("Construction"))
        {
            ConstructionScript constructionScript = raycastHit.transform.GetComponentInParent<ConstructionScript>();
            if (constructionScript != hoveredConstruction)
            {
                if (hoveredConstruction != null && hoveredConstruction != selectedConstruction) hoveredConstruction.outline.enabled = false;
                hoveredConstruction = constructionScript;
                constructionScript.outline.enabled = true;
            }
            if (Input.GetMouseButtonDown(0))
            {
                SelectConstruction(constructionScript);
            }
        }
        else if (raycastHit.transform.gameObject.CompareTag("Player"))
        {
            ConstructionScript constructionScript = raycastHit.transform.GetComponent<ConstructionScript>();
            if (constructionScript != hoveredConstruction)
            {
                if (hoveredConstruction != null && hoveredConstruction != selectedConstruction) hoveredConstruction.outline.enabled = false;
                hoveredConstruction = constructionScript;
                constructionScript.outline.enabled = true;
            }
            if (Input.GetMouseButtonDown(0))
            {
                SelectConstruction(constructionScript);
            }
        }
        else
        {
            if (hoveredConstruction != null && hoveredConstruction != selectedConstruction)
            {
                hoveredConstruction.outline.enabled = false;
                hoveredConstruction = null;
            }
        }
    }

    private void SelectPeasant(PeasantScript peasantScript)
    {

        if (selectedConstruction != null)
        {
            selectedConstruction.constructionDetailsScript = null;
            selectedConstruction.outline.enabled = false;
            selectedConstruction.outline.OutlineWidth = 2;
        }
        if (selectedPeasant != null)
        {
            selectedPeasant.peasantDetailsScript = null;
            selectedPeasant.outline.enabled = false;
            selectedPeasant.outline.OutlineWidth = 2;
        }
        selectedPeasant = peasantScript;
        selectedPeasant.outline.enabled = true;
        selectedPeasant.outline.OutlineWidth = 8;

        GameManager.Instance.SelectPeasant(peasantScript);
    }

    public void UnselectPeasant()
    {
        selectedPeasant.peasantDetailsScript = null;
        selectedPeasant.outline.enabled = false;
        selectedPeasant.outline.OutlineWidth = 2;
        selectedPeasant = null;
    }

    public void SelectConstruction(ConstructionScript constructionScript)
    {
        if (selectedPeasant != null)
        {
            selectedPeasant.peasantDetailsScript = null;
            selectedPeasant.outline.enabled = false;
            selectedPeasant.outline.OutlineWidth = 2;
        }
        if (selectedConstruction != null)
        {
            selectedConstruction.constructionDetailsScript = null;
            selectedConstruction.outline.enabled = false;
            selectedConstruction.outline.OutlineWidth = 2;
        }
        selectedConstruction = constructionScript;
        selectedConstruction.outline.enabled = true;
        selectedConstruction.outline.OutlineWidth = 8;

        GameManager.Instance.SelectConstruction(constructionScript);
    }

    public void UnselectConstruction()
    {
        selectedConstruction.constructionDetailsScript = null;
        selectedConstruction.outline.enabled = false;
        selectedConstruction.outline.OutlineWidth = 2;
        selectedConstruction = null;
    }
}
