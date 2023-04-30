using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CompassScript : MonoBehaviour
{
    private Outline outline;
    [SerializeField] private Camera uiCamera;
    [SerializeField] private RawImage mapImage;
    [SerializeField] private Transform shipIcon;
    [SerializeField] private Transform destination;

    [SerializeField] private RectTransform compass;
    [SerializeField] private Transform coordinates;
    [SerializeField] private Transform dial;
    private Transform shipTransform;

    private void Start()
    {
        outline = GetComponent<Outline>();
        shipTransform = ShipScript.Instance.transform;
    }

    void Update()
    {
        UpdateOrientations();

        RaycastHit raycastHit;
        Ray ray = uiCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out raycastHit))
        {
            outline.enabled = false;
            return;
        }
        Debug.Log(raycastHit.transform.gameObject.tag);
        if (raycastHit.transform.gameObject.CompareTag("Compass"))
        {
            outline.enabled = true;
        }
        else outline.enabled = false;
    }

    private void UpdateOrientations()
    {
        shipIcon.rotation = Quaternion.Euler(new Vector3(0, -shipTransform.rotation.eulerAngles.z, 0));
        //ship.GetComponent<SpriteRenderer>().flipX = (shipTransform.rotation.y < 0 && shipTransform.rotation.y > -180);

        //Es rota l'agulla de la brúixula
        float xDiff = destination.position.x - shipTransform.position.x;
        float zDiff = destination.position.z - shipTransform.position.z;
        float angle = Mathf.Atan2(zDiff, xDiff) * Mathf.Rad2Deg - 90;
        //arrow.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        compass.rotation = Quaternion.Euler(new Vector3(0, shipTransform.rotation.eulerAngles.z, 0));
        coordinates.localRotation = Quaternion.Euler(new Vector3(-90, -shipTransform.rotation.eulerAngles.y, 0));
        dial.localRotation = Quaternion.Euler(new Vector3(-90, -angle, 0));
    }

    public void SetDestination(Vector3 newDestination)
    {
        destination.position = newDestination;
    }

}
