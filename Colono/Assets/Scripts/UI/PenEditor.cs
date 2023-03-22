using UnityEngine;
using TMPro;
using static ResourceScript;
using UnityEngine.UI;

public class PenEditor : EditorScript
{
    private PenScript penScript;
    public PenScript shipPenScript;

    public TextMeshProUGUI penAnimalsText;
    public TextMeshProUGUI shipAnimalsText;

    public Transform rows;
    public ScrollRect scrollRect;
    public PenRowScript[] penRows;
    public GameObject penRowPrefab;

    public override void SetEditor(ConstructionScript constructionScript)
    {
        base.SetEditor(constructionScript);

        penScript = (PenScript)constructionScript;
        shipPenScript = ShipScript.Instance.shipInteriorPen;

        foreach (Transform row in rows)
        {
            Destroy(row.gameObject);
        }

        int animalTypes = System.Enum.GetValues(typeof(AnimalType)).Length;
        penRows = new PenRowScript[animalTypes];
        for (int i = 0; i < animalTypes; i++)
        {
            GameObject gridRow = Instantiate(penRowPrefab, rows);
            PenRowScript penRowScript = gridRow.GetComponent<PenRowScript>();
            penRowScript.penEditor = this;
            penRowScript.animalType = (AnimalType)i;
            penRowScript.animalImage.sprite = IslandEditor.Instance.GetResourceSprite(ResourceType.Animal, i);
            penRowScript.penAnimals = penScript.animals[i];
            penRowScript.shipAnimals = shipPenScript.animals[i];
            penRowScript.desiredAmount = penScript.desiredAmounts[i];
            penRowScript.UpdateValues();
            penRows[i] = penRowScript;
        }

        UpdatePenText();

    }

    public void UpdatePenRow(AnimalType animalType)
    {
        if (penRows.Length != 0)
        {
            PenRowScript penRowScript = penRows[(int)animalType];
            penRowScript.penAnimals = penScript.animals[(int)animalType];
            penRowScript.shipAnimals = shipPenScript.animals[(int)animalType];
            penRowScript.UpdateValues();
            UpdatePenText();
        }
    }

    public void UpdatePenText()
    {
        penAnimalsText.text = penScript.animalList.Count + "/" + penScript.maxPeasants;
        shipAnimalsText.text = shipPenScript.animalList.Count + "/" + shipPenScript.maxPeasants;
    }

    public void MoveAnimal(AnimalType animalType, bool toPen)
    {
        PenScript origin = toPen ? shipPenScript : penScript;
        PenScript destination = toPen ? penScript : shipPenScript;
        
        AnimalScript animalScript = origin.RemoveAnimal(animalType);
        if (animalScript == null) return;
        
        destination.AddAnimal(animalScript);

        UpdatePenText();
    }

    public void ChangeDesiredAmount(AnimalType animalType, int difference)
    {
        penScript.ChangeDesiredAmount(animalType, difference);
    }

}
