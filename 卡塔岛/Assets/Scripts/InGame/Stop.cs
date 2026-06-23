using UnityEngine;
using UnityEngine.UI;

public class Stop : MonoBehaviour
{
    [SerializeField] private Image blockImage;
    public Animator animator;

    public void stopgame()
    {
        animator.SetBool("Stop", true);
        if (blockImage != null) blockImage.raycastTarget = true;
    }

    public void begingame()
    {
        animator.SetBool("Stop", false);
        if (blockImage != null) blockImage.raycastTarget = false;
    }
}
