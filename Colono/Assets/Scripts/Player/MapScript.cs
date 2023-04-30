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
        mapImage = GetComponent<RawImage>().rectTransform;
    }

    private Vector3 GetMousePosition()
    {
        Vector2 posInImage = new Vector2(Input.mousePosition.x - mapImage.rect.width / 2, Input.mousePosition.y - mapImage.rect.height / 2);
        Debug.Log(posInImage);
        return new Vector3(posInImage.x, 0, posInImage.y);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        compassScript.SetDestination(GetMousePosition());
    }
}
