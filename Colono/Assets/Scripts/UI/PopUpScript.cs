using UnityEngine;

public class PopUpScript : MonoBehaviour
{
    private Animator animator;

    public void ShowPopUp()
    {
        animator = GetComponent<Animator>();
        animator.Play("ShowPopUp");
    }

    public void EndAnim()
    {
        if (GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("HidePopUp"))
        {
            gameObject.SetActive(false);
        }
    }

    public void HidePopUp()
    {
        animator.Play("HidePopUp");
    }
}
