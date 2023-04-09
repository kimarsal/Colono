using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting;

public class CabinEditor : EditorScript
{
    [SerializeField] private PeasantRowScript peasantRowPrefab;
    [SerializeField] private Transform rowsTransform;
    [SerializeField] private ScrollRect scrollRect;
    private List<PeasantRowScript>[] peasantRows;

    [SerializeField] private Button[] tabButtons;
    private int selectedTab;

    private void SetRow(PeasantScript peasantScript, int tab)
    {
        PeasantRowScript rowScript = Instantiate(peasantRowPrefab, rowsTransform);
        rowScript.peasantScript = peasantScript;
        peasantRows[tab].Add(rowScript);
    }

    public override void SetEditor(ConstructionScript constructionScript)
    {
        foreach (Transform row in rowsTransform)
        {
            Destroy(row.gameObject);
        }

        peasantRows = new List<PeasantRowScript>[2];
        peasantRows[0] = new List<PeasantRowScript>();
        foreach(PeasantScript peasantScript in constructionScript.islandScript.peasantList)
        {
            SetRow(peasantScript, 0);
        }

        peasantRows[1] = new List<PeasantRowScript>();
        foreach (ConstructionScript construction in constructionScript.islandScript.constructionList)
        {
            foreach(PeasantScript peasantScript in construction.peasantList)
            {
                SetRow(peasantScript, 1);
            }
        }

        SelectTab(0);

    }

    public void SelectTab(int tabIndex)
    {
        selectedTab = tabIndex;

        for (int i = 0; i < tabButtons.Length; i++)
        {
            tabButtons[i].interactable = tabIndex != i;

            foreach (PeasantRowScript row in peasantRows[i])
            {
                row.gameObject.SetActive(tabIndex == i);
            }
        }

        scrollRect.normalizedPosition = new Vector2(0, 1);
    }
}
