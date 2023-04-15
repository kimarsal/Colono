using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PeasantDetailsScript : MonoBehaviour
{
    [SerializeField] private PeasantRowScript peasantRowScript;

    public void SetPeasantDetails(PeasantScript peasantScript)
    {
        peasantRowScript.SetPeasant(peasantScript);
    }
}
