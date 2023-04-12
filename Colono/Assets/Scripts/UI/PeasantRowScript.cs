using TMPro;
using UnityEngine;

public class PeasantRowScript : MonoBehaviour
{
    public PeasantScript peasantScript;
    [SerializeField] private TextMeshProUGUI peasantText;
    [SerializeField] private Transform adultHeadTransform;
    [SerializeField] private Transform childHeadTransform;

    private void Start()
    {
        peasantText.text = peasantScript.name;
        GameObject head = Instantiate(ResourceScript.Instance.GetPeasantHeadPrefab(peasantScript),
            peasantScript.peasantType == PeasantScript.PeasantType.Adult ? adultHeadTransform : childHeadTransform);
        head.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().material = peasantScript.GetHead().GetComponent<SkinnedMeshRenderer>().material;
    }

    private void Update()
    {

    }

    public void SelectPeasant()
    {

    }
}
