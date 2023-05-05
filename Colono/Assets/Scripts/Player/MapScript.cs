using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapScript : MonoBehaviour, IPointerClickHandler
{
    private RectTransform mapImage;
    [SerializeField] private CompassScript compassScript;

    private void Start()
    {
        mapImage = GetComponent<RectTransform>();
        float widthRatio = (float)Screen.width / (float)mapImage.rect.width;
        float heightRatio = (float)Screen.height / (float)mapImage.rect.height;
        mapImage.localScale = Mathf.Min(widthRatio, heightRatio) * 0.8f * Vector3.one;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 posInImage = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height) - 0.5f * Vector2.one;
        Vector3 posInMap = new Vector3(posInImage.x * 1150, 0, posInImage.y * 650);
        Debug.Log(posInImage);
        compassScript.SetDestination(posInMap);
    }
}
