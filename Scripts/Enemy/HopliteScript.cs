using System.Collections;
using UnityEngine;

public class HopliteScript : Enemy
{

    // -------------------------
    // STATS
    // -------------------------

    [Header("Stats")]
    [SerializeField] private HopliteStats hopliteSO;

    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator anim;

    public float maxHP;
    public float currentHP;
    public float atk;

    public float patrolSpeed;
    public float patrolTimeToChangeDirection;

    public float chaseSpeed;
    public float chaseDistance;

    public float windUpTime;

    // runtime value read from SO
    public float chaseTimeToWindup;

    [Header("Wall Collision")]
    [SerializeField] private float wallBounceSpread = 10f;

    // -------------------------
    // INTERNAL
    // -------------------------

    private Vector3 patrolDirection;
    private float patrolTimer;

    private bool isDead;
    private bool isTakingDamage;

    private bool windupRunning;

    private float chaseStateTimer;

    // -------------------------
    // FSM
    // -------------------------

    public enum HopliteState
    {
        PATROL,
        CHASE,
        WINDUP
    }

    public HopliteState CurrentState { get { return currentState; } private set { currentState = value; } }

    public HopliteState currentState;

    // -------------------------
    // RUNTIME DATA (Animator friendly)
    // -------------------------

    public Vector3 Velocity { get; private set; }
    private Vector3 smoothedVelocity; // per smoothing

    public bool IsMoving => Velocity.sqrMagnitude > 0.01f;
    public bool IsWindup => CurrentState == HopliteState.WINDUP;
    public bool IsTakingDamage => isTakingDamage;
    public bool IsDead => isDead;

    private bool isPlayerVisible;
    [SerializeField]
    private LayerMask observableLayers;

    private Vector3 lastPosition;
    private Vector3 lastSeenPlayerPosition;

    private bool recentPlayerHit; // flag per evitare di colpire il player più volte in rapida successione

    private float pushBackForce = 10f;

    private void Start()
    {
        SetupSOStats();
        ChangePatrolDirection();

        if (anim == null)
            anim = GetComponent<Animator>();

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        lastPosition = transform.position;

        // stato iniziale esplicito
        CurrentState = HopliteState.PATROL;
    }

    private void Update()
    {
        StateUpdate();

        // aggiorno animator
        if (anim != null)
        {
            anim.SetFloat("Speed", Velocity.magnitude);
            anim.SetBool("IsWindup", IsWindup);
        }
    }

    private void FixedUpdate()
    {
        PlayerVisibleCheck();
        UpdateVelocity();
    }

    private void OnEnable()
    {
        ChangeState(HopliteState.PATROL);
        ResetBehaviourVariables();
        ResetAnimator();
    }

    private void PlayerVisibleCheck()
    {
        Vector3 playerVector = Player.instance.transform.position;

        if (Physics.SphereCast(transform.position, 1.4f, playerVector - transform.position, out RaycastHit hit, 30, observableLayers))
        {
            if (hit.collider.gameObject.TryGetComponent(out Player _))
            {
                lastSeenPlayerPosition = playerVector;
                isPlayerVisible = true;
            }
            else
            {
                isPlayerVisible = false;
            }
        }
        else
        {
            isPlayerVisible = false;
        }
    }

    // -------------------------
    // FSM UPDATE
    // -------------------------

    private void StateUpdate()
    {
        switch (CurrentState)
        {
            case HopliteState.PATROL: PatrolUpdate(); break;
            case HopliteState.CHASE: ChaseUpdate(); break;
            case HopliteState.WINDUP: WindupUpdate(); break;
        }
    }

    // -------------------------
    // STATE: PATROL
    // -------------------------

    private void PatrolUpdate()
    {
        if (IsPlayerInRange(chaseDistance) && isPlayerVisible)
        {
            ChangeState(HopliteState.CHASE);
            return;
        }

        rb.MovePosition(rb.position + patrolDirection * patrolSpeed * Time.deltaTime);

        patrolTimer -= Time.deltaTime;
        if (patrolTimer <= 0f)
            ChangePatrolDirection();
    }

    // -------------------------
    // STATE: CHASE
    // -------------------------

    private void ChaseUpdate()
    {
        if (!IsPlayerInRange(chaseDistance))
        {
            ChangeState(HopliteState.PATROL);
            return;
        }

        // conto il tempo in CHASE; se supera la soglia -> WINDUP
        chaseStateTimer += Time.deltaTime;
        if (chaseStateTimer >= chaseTimeToWindup)
        {
            ChangeState(HopliteState.WINDUP);
            chaseStateTimer = 0f;
            return;
        }

        if (!isPlayerVisible)
        {
            Vector3 lastSeenDir = (lastSeenPlayerPosition - rb.position).normalized;
            rb.MovePosition(rb.position + chaseSpeed * Time.deltaTime * lastSeenDir);

            Vector3 offset = lastSeenDir;
            if (Vector3.Distance(transform.position, lastSeenPlayerPosition + offset) < 1.5f)
            {
                ChangeState(HopliteState.PATROL);
            }
        }
        else
        {
            lastSeenPlayerPosition = Player.instance.transform.position;
            if (!isTakingDamage)
            {
                Vector3 dir = (Player.instance.transform.position - rb.position).normalized;
                rb.MovePosition(rb.position + chaseSpeed * Time.deltaTime * dir);
            }
        }
    }

    // -------------------------
    // STATE: WINDUP
    // -------------------------

    private void WindupUpdate()
    {
        recentPlayerHit = false;
        if (windupRunning)
            return;

        windupRunning = true;
        StartCoroutine(WindupCO());
    }

    private IEnumerator WindupCO()
    {
        rb.linearVelocity = Vector3.zero;

        windUpTime = hopliteSO.windUpTime;

        yield return new WaitForSeconds(windUpTime);

        // reset guard flag così la prossima volta la coroutine può partire di nuovo
        windupRunning = false;

        if (IsPlayerInRange(chaseDistance))
        {
            ChangeState(HopliteState.CHASE);
        }
        else
        {
            ChangeState(HopliteState.PATROL);
        }
    }

    private void ChangeState(HopliteState newState)
    {
        CurrentState = newState;

        // reset timer quando si entra/esce dallo stato CHASE
        if (newState == HopliteState.CHASE)
            chaseStateTimer = 0f;
        else
            chaseStateTimer = 0f;
    }

    private void ChangePatrolDirection()
    {
        patrolDirection = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        ).normalized;

        patrolTimer = patrolTimeToChangeDirection;
    }

    private bool IsPlayerInRange(float range)
    {
        return Vector3.Distance(Player.instance.transform.position, transform.position) <= range;
    }

    // -------------------------
    // COLLISIONS
    // -------------------------

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            HandleWallCollision(collision);
        }

        if (collision.gameObject.TryGetComponent(out Player player))
        {
            if (!recentPlayerHit)
            {
                recentPlayerHit = true;

                player.TakeDamage(atk);
                player.OnPush();
                PushSomething(collision, pushBackForce);

                ChangeState(HopliteState.WINDUP);
            }
        }
    }

    private void HandleWallCollision(Collision collision)
    {
        Vector3 currentDir = Velocity;
        currentDir.y = 0f;

        if (currentDir.sqrMagnitude < 0.0001f)
        {
            currentDir = patrolDirection;
        }

        if (currentDir.sqrMagnitude < 0.0001f)
        {
            currentDir = transform.forward;
            currentDir.y = 0f;
        }

        Vector3 newDir = -currentDir.normalized;
        newDir.y = 0f;
        newDir.Normalize();

        float halfSpread = Mathf.Clamp(wallBounceSpread, 0f, 180f) * 0.5f;
        float angle = Random.Range(-halfSpread, halfSpread);
        newDir = Quaternion.AngleAxis(angle, Vector3.up) * newDir;
        newDir.y = 0f;
        newDir.Normalize();

        patrolDirection = newDir;
        patrolTimer = patrolTimeToChangeDirection;

        ChangeState(HopliteState.PATROL);
    }

    // -------------------------
    // DAMAGE
    // -------------------------

    public override void TakeDamage(float amount)
    {
        if (isDead)
            return;
        
        currentHP -= amount;

        if (!isTakingDamage)
            StartCoroutine(TakeDamageCO());

        if (currentHP <= 0)
            Die();
    }


    private IEnumerator TakeDamageCO()
    {
        isTakingDamage = true;

        if (anim != null)
            anim.SetBool("TakeDamage", true);

        // durata animazione hit (meglio se uguale al clip)
        yield return new WaitForSeconds(.8f);

        isTakingDamage = false;

        if (anim != null)
            anim.SetBool("TakeDamage", false);

        rb.linearVelocity = Vector3.zero;
    }


    private void ReadjustMovement()
    {
        if (CurrentState == HopliteState.PATROL)
            rb.MovePosition(rb.position + patrolSpeed * Time.deltaTime * patrolDirection);
    }

    protected override void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (anim != null)
            anim.SetBool("IsDead", true);

        // stop FSM
        enabled = false;

        // stop fisica
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.detectCollisions = false;

        base.Die();
    }


    private void UpdateVelocity()
    {
        Vector3 rawVelocity = transform.position - lastPosition;
        rawVelocity.y = 0f;

        float delta = Time.deltaTime > 0 ? Time.deltaTime : 0.0001f;
        rawVelocity /= delta;

        smoothedVelocity = Vector3.Lerp(smoothedVelocity, rawVelocity, 0.5f);
        Velocity = smoothedVelocity;

        lastPosition = transform.position;
    }

    private void SetupSOStats()
    {
        maxHP = hopliteSO.maxHP;
        currentHP = hopliteSO.currentHP;
        atk = hopliteSO.atk;
        patrolSpeed = hopliteSO.patrolSpeed;
        patrolTimeToChangeDirection = hopliteSO.patrolTimeToChangeDirection;
        chaseSpeed = hopliteSO.chaseSpeed;
        chaseDistance = hopliteSO.chaseDistance;
        windUpTime = hopliteSO.windUpTime;
        chaseTimeToWindup = hopliteSO.chaseTimeToWindup;
        pushBackForce = hopliteSO.pushBackForce;
        pushBackResistance = hopliteSO.pushBackResistance;
    }

    public override void ResetBehaviourVariables()
    {
        currentHP = maxHP;
        isDead = false;
        isTakingDamage = false;
        windupRunning = false;
        recentPlayerHit = false;
        rb.isKinematic = false;
        rb.detectCollisions = true;
    }
    
    private void ResetAnimator()
    {
        if (anim == null)
            return;
        anim.SetBool("IsDead", false);
        anim.SetBool("TakeDamage", false);
        anim.SetFloat("Speed", 0f);
        anim.SetBool("IsWindup", false);
        anim.SetBool("Attack", false);
        anim.Play("Idle", 0, 0f);
    }
}