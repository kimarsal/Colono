using UnityEngine;

public class MenuShipScript : MonoBehaviour
{
    private float currentTime;

    void Update()
    {
        currentTime = (currentTime + Time.deltaTime) % (Mathf.PI * 2);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, Mathf.Sin(currentTime) * 2f);
    }
}
