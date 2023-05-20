using UnityEngine;
using UnityEngine.UI;

public class ShipEditor : EditorScript
{
    [SerializeField] private Button[] repairButtons;
    [SerializeField] private Image[] sections;
    private PlayerController playerController;

    private void Start()
    {
        playerController = ShipScript.Instance.GetComponent<PlayerController>();
        for(int i=0;i< repairButtons.Length; i++)
        {
            bool canRepair = playerController.CanRepair(i);
            repairButtons[i].interactable = canRepair;
            sections[i].color = canRepair? new Color(1f, 0.4f, 0.4f, 0.5f) : new Color(0.4f, 1f, 0.4f, 0.5f);
        }
    }

    public void Repair(int position)
    {
        if (constructionScript.islandScript.UseResource(ResourceScript.ResourceType.Material, (int)ResourceScript.MaterialType.Wood))
        {
            bool canRepair = playerController.Repair(position);
            repairButtons[position].interactable = canRepair;
            sections[position].color = canRepair ? new Color(1f, 0.4f, 0.4f, 0.5f) : new Color(0.4f, 1f, 0.4f, 0.5f);
        }
    }
}
