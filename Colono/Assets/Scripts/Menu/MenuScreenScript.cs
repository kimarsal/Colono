using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstructionDetailsScript;

public class MenuScreenScript : MonoBehaviour
{
    public enum MenuScreenState { Down, Middle, Up };
    public MenuScreenState state;
    private RectTransform rectTransform;
    [SerializeField] private RectTransform canvasRectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if(state != MenuScreenState.Middle) rectTransform.anchoredPosition = new Vector2(0, -canvasRectTransform.rect.height);
    }

    public void Move(bool up = true)
    {
        state += up ? 1 : -1;
        StartCoroutine(ChangeStateCoroutine());
    }

    private IEnumerator ChangeStateCoroutine()
    {
        float duration = 0.3f;
        float currentTime = 0;
        float startPos = rectTransform.anchoredPosition.y;
        float targetPos = 0;

        switch (state)
        {
            case MenuScreenState.Down: targetPos = -canvasRectTransform.rect.height; break;
            case MenuScreenState.Up: targetPos = canvasRectTransform.rect.height; break;
        }

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            rectTransform.anchoredPosition = new Vector2(0, Mathf.Lerp(startPos, targetPos, currentTime / duration));
            yield return null;
        }

        yield break;
    }

}
