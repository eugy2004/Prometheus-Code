using UnityEngine;

[RequireComponent(typeof(Animator))]
public class MinotaurAnimatorController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MinotaurScript minotaur;
    [SerializeField] private SpriteFlipStretch flipStretch;

    private Animator anim;

    // ultima direzione valida (idle / anticipation / windup)
    private Vector3 lastDir = Vector3.forward;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (minotaur == null)
            return;

        if (minotaur.IsDead)
        {
            anim.SetBool("IsDead", true);
            return;
        }

        UpdateSpeed();
        UpdateDirection();
        UpdateView();
        UpdateStates();
        UpdateFlip();
    }

    // -------------------------
    // SPEED (Idle / Move)
    // -------------------------
    private void UpdateSpeed()
    {
        float speed = minotaur.Velocity.magnitude;
        anim.SetFloat("Speed", speed);

        // aggiorna direzione SOLO se:
        // - si sta muovendo
        // - NON è sotto forza esterna
        if (speed > 0.05f)
        {
            lastDir = minotaur.Velocity.normalized;
        }
    }


    // -------------------------
    // DIRECTION MEMORY
    // -------------------------
    private void UpdateDirection()
    {
        // se fermo (anticipation / windup), uso LookDirection
        if (!minotaur.IsMoving &&
            minotaur.LookDirection.sqrMagnitude > 0.001f)
        {
            lastDir = minotaur.LookDirection.normalized;
        }
    }

    // -------------------------
    // VIEW (Front / Back)
    // -------------------------
    private void UpdateView()
    {
        int view = lastDir.z >= 0 ? 1 : 0;
        anim.SetInteger("View", view);
    }

    // -------------------------
    // FSM → Animator Bools
    // -------------------------
    private void UpdateStates()
    {
        // stati principali
        anim.SetBool("IsAnticipation", minotaur.IsAnticipation);
        anim.SetBool("IsCharging", minotaur.IsCharging);
        anim.SetBool("IsWindup", minotaur.IsWindup);
        anim.SetBool("IsWindupHitPlayer", minotaur.IsWindupHitPlayer);


        // combat
        anim.SetBool("TakeDamage", minotaur.IsTakingDamage);
    }

    // -------------------------
    // FLIP + STRETCH
    // -------------------------
    private void UpdateFlip()
    {
        // niente flip durante windup o death
        if (minotaur.IsWindup || minotaur.IsDead)
            return;

        Vector2 flipDir;

        // durante anticipation → guarda il player
        if (minotaur.IsAnticipation)
        {
            flipDir = new Vector2(
                minotaur.LookDirection.x,
                minotaur.LookDirection.z
            );
        }
        //// durante charge (opzionale: stessa logica)
        //else if (minotaur.IsCharging)
        //{
        //    flipDir = new Vector2(
        //        minotaur.LookDirection.x,
        //        minotaur.LookDirection.z
        //    );
        //}
        // stati normali → movimento
        else
        {
            flipDir = new Vector2(lastDir.x, lastDir.z);
        }

        if (flipDir.sqrMagnitude > 0.001f)
            flipStretch.HandleFlip(flipDir);
    }

}
