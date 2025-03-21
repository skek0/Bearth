using UnityEngine;

public class Panel : MonoBehaviour
{
    Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void AdjustPanel()
    {
        if (animator.GetBool("IsOpened"))
        {
            animator.Play("Panel_in");
        }
        else
        {
            animator.Play("Panel_out");
        }
        animator.SetBool("IsOpened", !animator.GetBool("IsOpened"));
    }
}
