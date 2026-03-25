using UnityEngine;

[RequireComponent(typeof(Animator))]
public class HopliteAnimatorController : MonoBehaviour
{
    [SerializeField] private HopliteScript hoplite;
    [SerializeField] private SpriteFlipStretch flipStretch;

    private Animator anim;
    private Vector3 lastDir = Vector3.forward;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (hoplite == null)
            return;

        if (hoplite.IsDead)
        {
            anim.SetBool("IsDead", true);
            return;
        }

        UpdateSpeed();
        UpdateDirectionMemory();
        UpdateView();
        UpdateStates();
        if (!hoplite.IsTakingDamage)
        {
            UpdateFlip();
        }
    }

    private void UpdateSpeed()
    {
        float speed = hoplite.Velocity.magnitude;
        anim.SetFloat("Speed", speed);

        if (speed > 0.05f)
            lastDir = hoplite.Velocity.normalized;
}

    private void UpdateDirectionMemory()
    {
        if (!hoplite.IsMoving)
            return;
    }

    private void UpdateView()
    {
        int view = lastDir.z >= 0 ? 1 : 0;
        anim.SetInteger("View", view);
    }

    private void UpdateStates()
    {
        anim.SetBool("IsWindup", hoplite.IsWindup);
        anim.SetBool("TakeDamage", hoplite.IsTakingDamage);
        anim.SetBool("Attack", hoplite.IsWindup);
    }

    private void UpdateFlip()
    {
        if (hoplite.IsDead)
            return;

        Vector2 flipDir = new (lastDir.x, lastDir.z);

        if (flipDir.sqrMagnitude > 0.001f)
            flipStretch.HandleFlip(flipDir);
    }
}