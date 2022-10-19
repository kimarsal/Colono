using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PeasantDetailsScript : MonoBehaviour
{
    public Slider ageSlider;
    public Slider hungerSlider;
    public Slider exhaustionSlider;
    public GameObject resourcesList;

    public GameManager gameManager;
    public PeasantScript peasantScript;

    public void SetPeasantDetails(PeasantScript newPeasantScript)
    {
        if(peasantScript != null) peasantScript.peasantDetailsScript = null;
        peasantScript = newPeasantScript;
        peasantScript.peasantDetailsScript = this;

        UpdateDetails();
    }

    public void UpdateDetails()
    {
        ageSlider.value = peasantScript.age / 60;
        hungerSlider.value = peasantScript.hunger;
        exhaustionSlider.value = peasantScript.exhaustion;

        if(peasantScript.peasantType == PeasantScript.PeasantType.Adult)
        {
            PeasantAdultScript peasantAdultScript = (PeasantAdultScript)peasantScript;
            /*foreach (KeyValuePair<ResourceScript.MaterialType, int> materials in peasantAdultScript.materials)
            {

            }
            foreach (KeyValuePair<ResourceScript.CropType, int> crops in peasantAdultScript.crops)
            {

            }*/
        }
    }
}
