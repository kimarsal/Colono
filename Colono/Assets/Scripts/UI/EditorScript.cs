using Newtonsoft.Json;
using UnityEngine;

public abstract class EditorScript : MonoBehaviour
{
    [SerializeField] protected UpgradeButtonScript upgradeButton;
    protected ConstructionScript constructionScript;

    public virtual void SetEditor(ConstructionScript constructionScript)
    {
        this.constructionScript = constructionScript;
        UpdateUpgradeButton();
    }

    public virtual void UpdateUpgradeButton()
    {
        upgradeButton?.UpdateButton(constructionScript.level, constructionScript.islandScript.GetResourceAmount(ResourceScript.ResourceType.Material, (int)ResourceScript.MaterialType.Gem));
    }
}
