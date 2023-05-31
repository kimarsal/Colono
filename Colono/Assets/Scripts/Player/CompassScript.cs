using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CompassScript : MonoBehaviour
{
    private Outline outline;
    [SerializeField] private Camera uiCamera;
    [SerializeField] private SpriteRenderer shipIconImage;
    [SerializeField] private Transform destination;

    [SerializeField] private RectTransform compass;
    [SerializeField] private Transform coordinates;
    [SerializeField] private Transform dial;

    private void Start()
    {
        outline = GetComponent<Outline>();
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

        if (raycastHit.transform.gameObject.CompareTag("Compass"))
        {
            outline.enabled = true;
            if (Input.GetMouseButtonDown(0))
            {
                CanvasScript.Instance.OpenMap();
            }
        }
        else outline.enabled = false;
    }

    private void UpdateOrientations()
    {
        shipIconImage.transform.parent.rotation = Quaternion.Euler(new Vector3(0, -ShipScript.Instance.transform.rotation.eulerAngles.z, 0));
        shipIconImage.flipX = ShipScript.Instance.orientation > 180;

        //Es rota l'agulla de la brúixula
        float xDiff = destination.position.x - ShipScript.Instance.position.x;
        float zDiff = destination.position.z - ShipScript.Instance.position.z;
        float angle = Mathf.Atan2(zDiff, xDiff) * Mathf.Rad2Deg - 90;
        //arrow.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        compass.rotation = Quaternion.Euler(new Vector3(0, ShipScript.Instance.transform.rotation.eulerAngles.z, 0));
        coordinates.localRotation = Quaternion.Euler(new Vector3(-90, -ShipScript.Instance.orientation, 0));
        dial.localRotation = Quaternion.Euler(new Vector3(-90, -angle, 0));
    }

    public void SetDestination(Vector3 newDestination)
    {
        destination.position = newDestination;
    }
}
