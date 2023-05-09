using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeechBubbleScript : MonoBehaviour
{
    [SerializeField] private Animator bubbleAnimator;
    [SerializeField] private Image hungryImage;
    [SerializeField] private Image tiredImage;

    void Update()
    {
        transform.rotation = Quaternion.Euler(80, -transform.parent.rotation.y, 0);
    }

    public void IsHungry()
    {
        hungryImage.gameObject.SetActive(true);
        if (!tiredImage.gameObject.activeSelf) bubbleAnimator.Play("ShowBubble");
    }

    public void IsNoLongerHungry()
    {
        hungryImage.gameObject.SetActive(false);
        if (!tiredImage.gameObject.activeSelf) bubbleAnimator.Play("HideBubble");
    }

    public void IsTired()
    {
        tiredImage.gameObject.SetActive(true);
        if (!hungryImage.gameObject.activeSelf) bubbleAnimator.Play("ShowBubble");
    }

    public void IsNoLongerTired()
    {
        tiredImage.gameObject.SetActive(false);
        if (!hungryImage.gameObject.activeSelf) bubbleAnimator.Play("HideBubble");
    }
}
