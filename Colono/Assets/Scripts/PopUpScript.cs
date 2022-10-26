using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpScript : MonoBehaviour
{
    public void EndAnim()
    {
        if (GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("HidePopUp"))
        {
            gameObject.SetActive(false);
        }
    }
}
