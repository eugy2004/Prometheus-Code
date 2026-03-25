using UnityEngine;

public class PlayerAnimatorControllerOLD : MonoBehaviour
{
    private Animator anim;

    [SerializeField] private SpriteFlipStretch flipStretch;

    private Vector2 move;
    private Vector2 lastViewDirection = Vector2.down;

    private float speed;
    private int view;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        ResolveDirection();
        UpdateAnimator();
        UpdateStates();
        flipStretch.HandleFlip(move);
    }

    // -------------------------
    // ANIMATOR
    // -------------------------

    private void UpdateAnimator()
    {
        speed = Player.instance.MoveInput.sqrMagnitude;
        anim.SetFloat("Speed", speed);

        UpdateView();
    }

    private void UpdateView()
    {
        if (move.y < -0.4f)
            view = 0; // front
        else if (Mathf.Abs(move.x) > Mathf.Abs(move.y))
            view = 1; // side
        else if (move.y > 0.4f)
            view = 2; // back

        anim.SetInteger("View", view);
    }

    // -------------------------
    // DIRECTION
    // -------------------------

    private void ResolveDirection()
    {
        // 1️⃣ sto sparando → guarda SOLO la mira
        if (Player.instance.IsHoldingShoot)
        {
            move = Player.instance.AimDirection;
            lastViewDirection = move;
            return;
        }

        // 2️⃣ aim lock senza sparo
        if (Player.instance.IsAimingLocked)
        {
            move = Player.instance.LockedDirection;
            lastViewDirection = move;
            return;
        }

        // 3️⃣ movimento normale
        if (Player.instance.MoveInput != Vector2.zero)
        {
            move = Player.instance.MoveInput;
            lastViewDirection = move;
            return;
        }

        // 4️⃣ fallback
        move = lastViewDirection;
    }

    private void UpdateStates()
    {
        anim.SetBool("TakeDamage", Player.instance.IsBeingPushed);
    }
}