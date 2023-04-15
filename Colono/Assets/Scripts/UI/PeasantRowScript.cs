using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PeasantRowScript : MonoBehaviour
{
    private PeasantScript peasantScript;
    [SerializeField] private Transform adultHeadTransform;
    [SerializeField] private Transform childHeadTransform;
    private GameObject head;

    [SerializeField] private Slider ageSlider;
    [SerializeField] private Slider hungerSlider;
    [SerializeField] private Slider exhaustionSlider;
    public bool isInDetails;

    public void SetPeasant(PeasantScript newPeasantScript)
    {
        if(head != null)
        {
            Destroy(head);
        }

        peasantScript = newPeasantScript;
        peasantScript.peasantRowScript = this;

        head = Instantiate(ResourceScript.Instance.GetPeasantHeadPrefab(peasantScript),
            peasantScript.peasantType == PeasantScript.PeasantType.Adult ? adultHeadTransform : childHeadTransform);
        head.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().material = peasantScript.GetHead().GetComponent<SkinnedMeshRenderer>().material;
    }

    public void UpdatePeasantDetails()
    {
        ageSlider.value = peasantScript.age / PeasantScript.lifeExpectancy;
        hungerSlider.value = peasantScript.hunger;
        exhaustionSlider.value = peasantScript.exhaustion;
    }

    public void SelectPeasant()
    {
        if (!isInDetails && !peasantScript.isInBuilding)
        {
            CameraScript.Instance.canMove = true;
            GameManager.Instance.islandSelectionScript.SelectPeasant(peasantScript);
        }
    }

    public void PeasantDies()
    {
        if (!isInDetails)
        {
            CanvasScript.Instance.cabinEditor.RemoveRow(this, peasantScript.constructionScript != null);
        }
        else
        {
            GameManager.Instance.UnselectPeasant();
        }
    }

    public void MovePeasant(bool toConstruction)
    {
        if (!isInDetails)
        {
            CanvasScript.Instance.cabinEditor.MoveRow(this, toConstruction);
        }
    }

    public void PeasantArrivedToBuilding()
    {
        if(isInDetails)
        {
            GameManager.Instance.UnselectPeasant();
        }
    }

}
