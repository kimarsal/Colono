using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class EditorScript : MonoBehaviour
{
    [SerializeField] private UpgradeButtonScript upgradeButton;
    private ConstructionScript constructionScript;

    public virtual void SetEditor(ConstructionScript constructionScript)
    {
        this.constructionScript = constructionScript;
        UpdateUpgradeButton();
    }

    public void UpdateUpgradeButton()
    {
        if (upgradeButton != null)
        {
            upgradeButton.UpdateButton(constructionScript.level,
                constructionScript.islandScript.GetResourceAmount(ResourceScript.ResourceType.Material, (int)ResourceScript.MaterialType.Gem));
        }
    }
}
