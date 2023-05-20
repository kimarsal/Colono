using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineEditor : EditorScript
{
    [SerializeField] private Transform[] depths;

    public override void UpdateUpgradeButton()
    {
        base.UpdateUpgradeButton();

        if (constructionScript.level < 10)
        {
            upgradeButton.transform.SetParent(depths[constructionScript.level]);
            upgradeButton.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
        else upgradeButton.enabled = false;
    }
}
