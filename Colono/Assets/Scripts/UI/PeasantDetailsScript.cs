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

    public PeasantScript peasantScript;

    public void SetPeasantDetails(PeasantScript newPeasantScript)
    {
        if(peasantScript != null) peasantScript.peasantDetailsScript = null;
        peasantScript = newPeasantScript;
        peasantScript.peasantDetailsScript = this;
    }

    private void Update()
    {
        if (peasantScript == null) return;

        ageSlider.value = peasantScript.age / 60;
        hungerSlider.value = peasantScript.hunger;
        exhaustionSlider.value = peasantScript.exhaustion;
    }
}
