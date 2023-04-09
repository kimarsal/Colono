using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PeasantRowScript : MonoBehaviour
{
    public PeasantScript peasantScript;
    [SerializeField] private TextMeshProUGUI peasantText;

    private void Start()
    {
        peasantText.text = peasantScript.name;
    }

    private void Update()
    {

    }

    public void SelectPeasant()
    {

    }
}
