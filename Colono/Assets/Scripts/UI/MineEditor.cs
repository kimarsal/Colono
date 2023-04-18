using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineEditor : EditorScript
{
    [SerializeField] private Transform[] buttonPositions;

    public override void UpdateUpgradeButton()
    {
        base.UpdateUpgradeButton();

        upgradeButton.transform.SetParent(buttonPositions[constructionScript.level - 1]);
        upgradeButton.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }
}
