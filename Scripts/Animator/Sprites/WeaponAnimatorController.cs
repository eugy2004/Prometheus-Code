using UnityEngine;

public class WeaponAnimatorController : MonoBehaviour
{

    [SerializeField] private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void PlayThrowAnimation()
    {
        anim.SetTrigger("Throw");
    }
}
