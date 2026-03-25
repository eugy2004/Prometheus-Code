using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.VirtualTexturing;
using Random = UnityEngine.Random;

public class MinotaurScript : Enemy
{
    [Header("Stats")]
    [SerializeField] private MinotaurStats minotaurSO;

    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider col;
    [SerializeField] private Animator anim;
    [Header("Charge AfterImage")]
    [SerializeField] private GameObject afterImagePrefab;
    [SerializeField] private float afterImageRate = 0.06f;

    private float afterImageTimer;


    // -------------------------
    // STATS
    // -------------------------

    private float maxHP;
    private float currentHP;
    private float atk;

    private float patrolSpeed;
    private float patrolTimeToChangeDirection;
    private float chaseDistance;
    private float chaseSpeed;

    private float anticipationTime;
    private float chargeForce;
    private float currentChargeDistance;
    private float maxChargeDistance;
    private float minChargeDistance;
    private float windupTime;
    private float pushBackForce;

    // -------------------------
    // FSM
    // -------------------------

    public enum MinotaurState
    {
        PATROL,
        CHASE,
        ANTICIPATION,
        CHARGE,
        WINDUP
    }

    public MinotaurState CurrentState { get { return currentState; } private set { currentState = value; } }

    public MinotaurState currentState;

    // -------------------------
    // RUNTIME DATA (Animator friendly)
    // -------------------------

    public Vector3 Velocity { get; private set; }
    private Vector3 smoothedVelocity; // per smoothing

    public bool IsMoving => Velocity.sqrMagnitude > 0.01f;
    public bool IsAnticipation => CurrentState == MinotaurState.ANTICIPATION;
    public bool IsCharging => CurrentState == MinotaurState.CHARGE;
    public bool IsWindup => CurrentState == MinotaurState.WINDUP;
    public bool IsTakingDamage => isTakingDamage;
    public bool IsDead => isDead;
    public bool IsWindupHitPlayer => windupHitPlayer;


    public Vector3 LookDirection { get; private set; }

    // -------------------------
    // INTERNAL
    // -------------------------

    [SerializeField]
    private LayerMask observableLayers;

    private Vector3 patrolDirection;
    private float patrolTimer;
    private float bounceLockTimer;
    private float timeToChangeDirection;

    public bool isCharging;
    private bool isDead;
    private bool isTakingDamage;
    private bool isPlayerVisible;
    private bool anticipationCanceled;

    private float chargeTimer;
    private float chargeDuration;
    private bool recentPlayerHit;

    private bool anticipationRunning;
    private bool windupRunning;
    private bool windupHitPlayer;

    private Vector3 lastPosition;
    private Vector3 lastSeenPlayerPosition;
    private Vector3 moveDirection;

    // -------------------------
    // UNITY
    // -------------------------

    private void Awake()
    {
        col = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        StopAllCoroutines();
        ResetBehaviourVariables();
        ResetAnimator();
        SetupSOStats();
        ChangeState(MinotaurState.PATROL);
        ChangePatrolDirection();
        chargeDuration = maxChargeDistance / chargeForce;
        lastPosition = transform.position;
    }

    private void ResetAnimator()
    {
        anim.SetBool("IsDead", false);
        anim.SetBool("TakeDamage", false);
        anim.SetFloat("Speed", 0);
        anim.SetBool("IsAnticipation", false);
        anim.SetBool("IsCharging", false);
        anim.SetBool("IsWindup", false);
        anim.SetBool("IsWindupHitPlayer", false);
        anim.SetInteger("View", 0);
    }

    private void Update()
    {
        if (!isDead)
        {
            StateUpdate();
            UpdateLookDirection();
        }

        // aggiorno animator
        if (anim != null)
        {
            anim.SetFloat("Speed", Velocity.magnitude);
            anim.SetBool("IsAnticipation", IsAnticipation);
            anim.SetBool("IsCharging", IsCharging);
            anim.SetBool("IsWindup", IsWindup);
        }
    }

    private void FixedUpdate()
    {
        UpdateVelocity();
        PlayerVisibleCheck();

        if (CurrentState == MinotaurState.CHARGE)
        {
            HandleAfterImage();
        }
    }

    private void PlayerVisibleCheck()
    {
        Vector3 playerVector = Player.instance.transform.position;

        if (Physics.SphereCast(transform.position, 1.7f, playerVector - transform.position, out RaycastHit hit, 30, observableLayers))
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

    // -------------------------
    // FSM UPDATE
    // -------------------------

    private void StateUpdate()
    {
        switch (CurrentState)
        {
            case MinotaurState.PATROL: PatrolUpdate(); break;
            case MinotaurState.CHASE: ChaseUpdate(); break;
            case MinotaurState.ANTICIPATION: AnticipationUpdate(); break;
            case MinotaurState.CHARGE: ChargeUpdate(); break;
            case MinotaurState.WINDUP: WindupUpdate(); break;
        }
    }

    private void ChangeState(MinotaurState newState)
    {
        if (newState == MinotaurState.CHASE)
            currentChargeDistance = Random.Range(minChargeDistance, maxChargeDistance);
        CurrentState = newState;
    }

    // -------------------------
    // STATE: PATROL
    // -------------------------

    private void PatrolUpdate()
    {
        if (IsPlayerInRange(chaseDistance) && isPlayerVisible)
        {
            ChangeState(MinotaurState.CHASE);
            return;
        }

        rb.MovePosition(rb.position + patrolSpeed * Time.deltaTime * patrolDirection);

        patrolTimer -= Time.deltaTime;
        if (patrolTimer <= 0f)
            ChangePatrolDirection();
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

    // -------------------------
    // STATE: CHASE
    // -------------------------

    private void ChaseUpdate()
    {
        if (!IsPlayerInRange(chaseDistance))
        {
            ChangeState(MinotaurState.PATROL);
            return;
        }

        if (IsPlayerInRange(currentChargeDistance) && isPlayerVisible)
        {
            ChangeState(MinotaurState.ANTICIPATION);
            return;
        }

        if (!isPlayerVisible)
        {
            Vector3 lastSeenDir = (lastSeenPlayerPosition - rb.position).normalized;
            rb.MovePosition(rb.position + chaseSpeed * Time.deltaTime * lastSeenDir);

            Vector3 offset = lastSeenDir;
            if (Vector3.Distance(transform.position, lastSeenPlayerPosition + offset) < 1.5f)
            {
                ChangeState(MinotaurState.PATROL);
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
    // STATE: ANTICIPATION (ONE-SHOT)
    // -------------------------

    private void AnticipationUpdate()
    {
        if (anticipationRunning)
            return;

        anticipationRunning = true;
        StartCoroutine(AnticipationCO());
    }

    private IEnumerator AnticipationCO()
    {
        rb.linearVelocity = Vector3.zero;

        IncreaseRigidbodyStillness();

        yield return new WaitForSeconds(anticipationTime * 2 / 3);

        if (!isPlayerVisible)
        {
            anticipationCanceled = true;
            ChangeState(MinotaurState.CHASE);
        }

        yield return new WaitForSeconds(anticipationTime * 1 / 3);

        RevertRigidbodyStillness();

        anticipationRunning = false;
        if (!anticipationCanceled)
            ChangeState(MinotaurState.CHARGE);

        anticipationCanceled = false;
    }

    // -------------------------
    // STATE: CHARGE
    // -------------------------

    private void ChargeUpdate()
    {
        if (!isCharging)
            StartCharge();

        chargeTimer += Time.deltaTime;

        if (chargeTimer >= chargeDuration)
            EndCharge();
    }

    private void StartCharge()
    {
        AudioManager.instance.CreateSound()
                .WithSoundData(AudioManager.instance.soundLibrary.minotaurSounds.chargeSound)
                .WithPosition(transform.position)
                .WithRandomPitch(true)
                .BuildAndPlay(transform);

        afterImageTimer = 0f;
        isCharging = true;
        chargeTimer = 0f;
        recentPlayerHit = false;

        rb.mass = 10;
        float newChargeForce = chargeForce * rb.mass;

        Vector3 dir = (lastSeenPlayerPosition - transform.position).normalized;
        rb.AddForce(dir * newChargeForce, ForceMode.Impulse);
    }

    private void HandleAfterImage()
    {
        afterImageTimer -= Time.deltaTime;

        if (afterImageTimer <= 0f)
        {
            GameObject img = Instantiate(
                afterImagePrefab,
                transform.position,
                Quaternion.identity
            );

            // flip orizzontale in base alla direzione
            float xDir = LookDirection.x != 0 ? Mathf.Sign(LookDirection.x) : 1f;

            img.transform.localScale = new Vector3(
                xDir,
                1f,
                1f
            );

            afterImageTimer = afterImageRate;
        }
    }


    private void EndCharge()
    {
        rb.linearVelocity = Vector3.zero;
        isCharging = false;
        ChangeState(MinotaurState.WINDUP);
    }

    // -------------------------
    // STATE: WINDUP
    // -------------------------

    private void WindupUpdate()
    {
        if (windupRunning)
            return;

        windupRunning = true;
        StartCoroutine(WindupCO());
    }

    private IEnumerator WindupCO()
    {
        rb.linearVelocity = Vector3.zero;
        IncreaseRigidbodyStillness();

        if (recentPlayerHit)
            windupTime = minotaurSO.reducedWindUpTime;
        else
            windupTime = minotaurSO.windUpTime;

        yield return new WaitForSeconds(windupTime);

        RevertRigidbodyStillness();

        windupRunning = false;
        recentPlayerHit = false;
        windupHitPlayer = false; // 👈 RESET QUI

        ChangeState(
            IsPlayerInRange(chaseDistance)
                ? MinotaurState.CHASE
                : MinotaurState.PATROL
        );
    }


    // -------------------------
    // UTILS
    // -------------------------

    private bool IsPlayerInRange(float range)
    {
        return Vector3.Distance(Player.instance.transform.position, transform.position) <= range;
    }

    private void BounceFromWall(Collision collision)
    {
        int otherLayer = collision.gameObject.layer;
        if (otherLayer == LayerMask.NameToLayer("Wall") ||
        otherLayer == LayerMask.NameToLayer("Obstacle"))
        {
            Vector3 dir = moveDirection;

            if (dir.sqrMagnitude < 0.001f)
                dir = transform.forward;

            Vector3 toCenter = GameManager.instance.CurrentRoomInfo.roomCenter.position - transform.position;
            toCenter.y = 0f;

            if (toCenter.sqrMagnitude < 0.001f)
                toCenter = -dir;

            dir = (-dir.normalized * 0.5f + toCenter.normalized * 0.5f).normalized;

            moveDirection = dir;
            transform.forward = moveDirection;
            transform.position += moveDirection * 0.25f;

            bounceLockTimer = 0.35f;
            timeToChangeDirection = minotaurSO.timeToChangeDirection;
        }
    }

    private void UpdateLookDirection()
    {
        // durante anticipation (e anche charge se vuoi)
        if (CurrentState == MinotaurState.ANTICIPATION ||
            CurrentState == MinotaurState.CHARGE)
        {
            Vector3 dir = Player.instance.transform.position - transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.001f)
            {
                LookDirection = dir.normalized;
            }

            return; // 🔒 IMPORTANTISSIMO
        }

        // comportamento normale (chase / idle)
        Vector3 normalDir = Player.instance.transform.position - transform.position;
        normalDir.y = 0f;

        if (normalDir.sqrMagnitude > 0.001f)
            LookDirection = normalDir.normalized;
    }


    private void IncreaseRigidbodyStillness()
    {
        rb.mass = 10;
        rb.linearDamping = 50;
    }

    private void RevertRigidbodyStillness()
    {
        rb.mass = 1;
        rb.linearDamping = 0;
    }

    private void SetupSOStats()
    {
        maxHP = minotaurSO.maxHP;
        currentHP = minotaurSO.currentHP;
        atk = minotaurSO.atk;

        patrolSpeed = minotaurSO.patrolSpeed;
        patrolTimeToChangeDirection = minotaurSO.patrolTimeToChangeDirection;
        chaseDistance = minotaurSO.chaseDistance;
        chaseSpeed = minotaurSO.chaseSpeed;

        anticipationTime = minotaurSO.anticipationTime;
        chargeForce = minotaurSO.chargeForce;
        minChargeDistance = minotaurSO.chargeInitiationDistance;
        maxChargeDistance = minotaurSO.chargeDistance;
        pushBackForce = minotaurSO.pushBackForce;
        pushBackResistance = minotaurSO.pushBackResistance;
        windupTime = minotaurSO.windUpTime;
    }

    public override void ResetBehaviourVariables()
    {
        currentHP = maxHP;
        patrolTimer = 1;

        rb.isKinematic = false;
        col.enabled = true;
        rb.detectCollisions = true;
        rb.useGravity = false;

        isCharging = false;
        isDead = false;
        isTakingDamage = false;

        chargeTimer = 1;
        recentPlayerHit = false;

        anticipationRunning = false;
        windupRunning = false;
        windupHitPlayer = false;

        isPlayerVisible = false;
    }

    // -------------------------
    // DAMAGE
    // -------------------------

    private void OnCollisionEnter(Collision collision)
    {
        HandlePlayerHit(collision);
        HandleEnemyHit(collision);
        BounceFromWall(collision);
    }

    private void HandlePlayerHit(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out Player player))
        {
            if (!recentPlayerHit)
            {
                recentPlayerHit = true;
                windupHitPlayer = true;

                player.TakeDamage(atk);
                player.OnPush();
                PushSomething(collision, pushBackForce);

                isCharging = false;
                ChangeState(MinotaurState.WINDUP);
            }
        }
        return;
    }

    private void HandleEnemyHit(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out Enemy _))
        {
            if (isCharging)
            {
                isCharging = false;
                OnPush(gameObject, pushBackForce * collision.rigidbody.mass);
                ChangeState(MinotaurState.WINDUP);
            }
        }
        return;
    }

    public override void TakeDamage(float amount)
    {
        currentHP -= amount;
        if (currentHP <= 0)
        {
            Die();
            return;
        }

        // se NON siamo in anticipation → hit reaction
        if (CurrentState != MinotaurState.ANTICIPATION)
        {
            if (!isTakingDamage && gameObject.activeInHierarchy)
            {
                AudioManager.instance.CreateSound()
                .WithSoundData(AudioManager.instance.soundLibrary.minotaurSounds.hurtSound)
                .WithPosition(transform.position)
                .WithRandomPitch(true)
                .BuildAndPlay(transform);

                StartCoroutine(TakeDamageCO());
            }
        }
    }


    private IEnumerator TakeDamageCO()
    {
        isTakingDamage = true;

        if (anim != null)
            anim.SetBool("TakeDamage", true);

        // durata animazione hit (meglio se uguale al clip)
        yield return new WaitForSeconds(0.25f);

        isTakingDamage = false;

        if (anim != null)
            anim.SetBool("TakeDamage", false);
    }

    private void ReadjustMovement()
    {
        if (isCharging)
        {
            rb.linearDamping = 0;
        }

        if (CurrentState == MinotaurState.PATROL)
            rb.MovePosition(rb.position + patrolSpeed * Time.deltaTime * patrolDirection);
    }

    protected override void Die()
    {
        if (isDead)
            return;

        isDead = true;
        col.enabled = false;
        StopAllCoroutines();

        AudioManager.instance.CreateSound()
                .WithSoundData(AudioManager.instance.soundLibrary.minotaurSounds.deathSound)
                .WithPosition(transform.position)
                .WithRandomPitch(true)
                .BuildAndPlay(AudioManager.instance.transform);

        if (anim != null)
        {
            ResetAnimator();
            anim.SetBool("IsDead", true);
        }

        // stop fisica
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.detectCollisions = false;

        base.Die();
    }
}