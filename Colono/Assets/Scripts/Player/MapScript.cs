using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapScript : MonoBehaviour, IPointerMoveHandler, IPointerClickHandler
{
    private RectTransform mapRect;
    private RectTransform mapContainerRect;

    private void Start()
    {
        mapRect = GetComponent<RawImage>().rectTransform;
        mapContainerRect = transform.parent.GetComponent<RectTransform>();
    }

    private Vector3 GetMousePosition()
    {
        Vector2 posInImage = Input.mousePosition - mapRect.position;
        Rect imageRect = mapRect.rect;
        return new Vector3(posInImage.x / imageRect.width, 0, posInImage.y / imageRect.height);
    }

    public void OnPointerMove(PointerEventData eventData)
    {

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ShipScript.Instance.GetComponent<MapController>().SetDestination(GetMousePosition() * 1000 + new Vector3(0, 0, 95));
    }
}
