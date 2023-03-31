using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapController : MonoBehaviour
{
    [SerializeField] private RawImage mapImage;
    [SerializeField] private Transform shipIcon;
    [SerializeField] private RectTransform arrow;
    [SerializeField] private Transform destination;

    void Update()
    {
        //Es rota l'agulla de la brúixula
        float xDiff = destination.position.x - transform.position.x;
        float zDiff = destination.position.z - transform.position.z;
        float angle = Mathf.Atan2(zDiff, xDiff) * Mathf.Rad2Deg - 90;
        arrow.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        shipIcon.rotation = Quaternion.Euler(new Vector3(0, -transform.rotation.eulerAngles.z, 0));

        //ship.GetComponent<SpriteRenderer>().flipX = (transform.rotation.y < 0 && transform.rotation.y > -180);
    }

    public void SetDestination(Vector3 newDestination)
    {
        destination.position = newDestination;
    }

}
