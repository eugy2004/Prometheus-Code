using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CyclopsAnimatorController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CyclopsScript cyclops;
    [SerializeField] private SpriteFlipStretch flipStretch;

    private Animator anim;
    private Vector3 lastDir = Vector3.forward;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (cyclops.IsDead)
        {
            anim.SetBool("IsDead", true);
            return;
        }
        UpdateMovement();
        UpdateDirection();
        UpdateView();
        UpdateFlip();
        UpdateStates();
    }

    // --------------------
    // MOVEMENT
    // --------------------
    private void UpdateMovement()
    {
        float speed = cyclops.Velocity.magnitude;
        anim.SetFloat("Speed", speed);
    }

    // --------------------
    // DIRECTION PRIORITY
    // Look > Velocity
    // --------------------
    private void UpdateDirection()
    {
        Vector3 dir = Vector3.zero;

        float lookX = anim.GetFloat("LookX");
        float lookY = anim.GetFloat("LookY");

        bool hasLookDir =
            Mathf.Abs(lookX) > 0.01f ||
            Mathf.Abs(lookY) > 0.01f;

        if (hasLookDir)
        {
            dir = new Vector3(lookX, 0f, lookY);
        }
        else if (cyclops.Velocity.sqrMagnitude > 0.001f)
        {
            dir = cyclops.Velocity;
        }

        if (dir.sqrMagnitude > 0.001f)
            lastDir = dir.normalized;
    }

    // --------------------
    // VIEW (Front / Back)
    // --------------------
    private void UpdateView()
    {
        int view = lastDir.z >= 0 ? 1 : 0;
        anim.SetInteger("View", view);
    }

    // --------------------
    // FLIP
    // --------------------
    private void UpdateFlip()
    {
        flipStretch.HandleFlip(new Vector2(lastDir.x, lastDir.z));
    }

    // --------------------
    // STATES → Animator
    // --------------------
    private void UpdateStates()
    {
        anim.SetBool("IsShooting", cyclops.IsShooting);
        anim.SetBool("TakeDamage", cyclops.IsTakingDamage);
    }

    // --------------------
    // LOOK DIRECTION (called by CyclopsScript)
    // --------------------
    public void SetLookDirection(Vector3 dir)
    {
        anim.SetFloat("LookX", dir.x);
        anim.SetFloat("LookY", dir.z);
    }
}
