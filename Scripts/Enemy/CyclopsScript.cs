using System.Collections;
using UnityEngine;

public class CyclopsScript : Enemy
{
    [Header("Stats")]
    [SerializeField] private CyclopsStats cyclopsSO;

    [Header("References")]
    [SerializeField] private CyclopsAnimatorController animator;
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject bullet;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private LayerMask observableLayers;

    // -------------------------
    // STATS
    // -------------------------

    private float maxHP;
    private float currentHP;
    private float atk;

    private float patrolSpeed;
    private float runSpeed;
    private float detectionDistance;
    private float timeToChangeDirection;
    private float timeToAim;
    private float timeToShoot;
    private float timeToReload;
    private float bulletSpeed;
    private float bulletRange;
    private float bulletSize;
    private float projectilePushBackForce;
    private float pushBackForce;

    // -------------------------
    // STATE
    // -------------------------

    public enum CyclopsState { PATROL, DETECTION, SHOOT }
    public enum TypeOfAttack { RUN, STAND }

    public CyclopsState currentState = CyclopsState.PATROL;
    public TypeOfAttack currentTypeOfAttack = TypeOfAttack.RUN;

    // -------------------------
    // RUNTIME DATA
    // -------------------------

    public Vector3 Velocity { get; private set; }
    public bool IsShooting => isShooting;
    [SerializeField]private bool isDead;
    public bool IsDead => isDead;
    private bool isTakingDamage;
    public bool IsTakingDamage => isTakingDamage;

    private Vector3 moveDirection;
    private float distanceFromPlayer;

    private bool isPlayerVisible;
    private bool isAiming;
    private bool isShooting;

    private int standAttackShots = 3;

    private float bounceLockTimer;

    private Vector3 lastSeenPlayerPosition;
    private float forgetPlayerTimer;

    // -------------------------
    // UNITY
    // -------------------------

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        StopAllCoroutines();
        ResetBehaviourVariables();
        ResetAnimator();
        SetupSOStats();
        ChangeState(CyclopsState.PATROL);
        ChangeDirection();
    }

    private void ResetAnimator()
    {
        anim.SetBool("IsShooting", false);
        anim.SetBool("IsDead", false);
        anim.SetBool("TakeDamage", false);
        anim.SetFloat("LookX", 0f);
        anim.SetFloat("LookY", 0f);
        anim.SetFloat("Speed", 0f);
        anim.SetInteger("View", 0);
    }

    private void Update()
    {
        if (bounceLockTimer > 0f)
            bounceLockTimer -= Time.deltaTime;

        if (!isDead)
            StateUpdate();
    }

    private void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, Player.instance.transform.position - transform.position, out RaycastHit hit, 25, observableLayers))
        {
            if (hit.collider.gameObject.TryGetComponent(out Player _))
            {
                lastSeenPlayerPosition = Player.instance.transform.position;
                isPlayerVisible = true;
            }
            else
                isPlayerVisible = false;
        }
        else
            isPlayerVisible = false;
    }

    // -------------------------
    // FSM
    // -------------------------

    private void StateUpdate()
    {
        switch (currentState)
        {
            case CyclopsState.PATROL:
                PatrolUpdate();
                break;

            case CyclopsState.DETECTION:
                DetectionUpdate();
                break;

            case CyclopsState.SHOOT:
                ShootUpdate();
                break;
        }
    }

    private void ChangeState(CyclopsState newState)
    {
        currentState = newState;
    }

    // -------------------------
    // PATROL
    // -------------------------

    private void PatrolUpdate()
    {
        UpdateDistanceFromPlayer();

        if (distanceFromPlayer <= detectionDistance && isPlayerVisible)
        {
            Velocity = Vector3.zero;
            ChangeState(CyclopsState.DETECTION);
            return;
        }

        timeToChangeDirection -= Time.deltaTime;
        if (timeToChangeDirection <= 0f)
            ChangeDirection();

        Velocity = moveDirection * patrolSpeed;
        transform.position += Velocity * Time.deltaTime;
    }

    private void ChangeDirection()
    {
        if (bounceLockTimer > 0f)
            return;

        if (Player.instance != null && Random.value < (2f / 3f))
        {
            Vector3 toPlayer = Player.instance.transform.position - transform.position;
            toPlayer.y = 0f;

            if (toPlayer.sqrMagnitude < 0.001f)
            {
                moveDirection = new Vector3(
                    Random.Range(-1f, 1f),
                    0f,
                    Random.Range(-1f, 1f)
                ).normalized;
            }
            else
            {
                float sx = Mathf.Sign(toPlayer.x);
                float sz = Mathf.Sign(toPlayer.z);

                bool preferX = Mathf.Abs(toPlayer.x) >= Mathf.Abs(toPlayer.z);

                if (preferX)
                {
                    moveDirection = new Vector3(
                        sx * Random.Range(0.7f, 1f),
                        0f,
                        sz * Random.Range(0f, 0.6f)
                    );
                }
                else
                {
                    moveDirection = new Vector3(
                        sx * Random.Range(0f, 0.6f),
                        0f,
                        sz * Random.Range(0.7f, 1f)
                    );
                }

                if (Random.value < 0.15f)
                    moveDirection = new Vector3(moveDirection.z, 0f, moveDirection.x);

                moveDirection = moveDirection.normalized;
            }
        }
        else
        {
            moveDirection = new Vector3(
                Random.Range(-1f, 1f),
                0f,
                Random.Range(-1f, 1f)
            ).normalized;
        }

        if (moveDirection != Vector3.zero)
            transform.forward = moveDirection;

        timeToChangeDirection = cyclopsSO.timeToChangeDirection;
    }

    private void BounceFromWall()
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
        timeToChangeDirection = cyclopsSO.timeToChangeDirection;
    }

    // -------------------------
    // DETECTION
    // -------------------------

    private void DetectionUpdate()
    {
        if (!isPlayerVisible)
            forgetPlayerTimer -= Time.deltaTime;

        Velocity = Vector3.zero;
        UpdateDistanceFromPlayer();
        UpdateLookDirectionToPlayer();

        if (distanceFromPlayer > detectionDistance || forgetPlayerTimer <= 0)
        {
            forgetPlayerTimer = 3;
            ChangeState(CyclopsState.PATROL);
            return;
        }

        if (!isAiming && isPlayerVisible)
            StartCoroutine(AimCO());
    }

    private IEnumerator AimCO()
    {
        isAiming = true;
        yield return new WaitForSeconds(timeToAim * 2 / 3);

        if (!isPlayerVisible)
        {
            isAiming = false;
            ChangeState(CyclopsState.DETECTION);
            yield break;
        }

        yield return new WaitForSeconds(timeToAim / 3);

        isAiming = false;
        currentTypeOfAttack = RandomizeTypeOfShoot();
        ChangeState(CyclopsState.SHOOT);
    }

    // -------------------------
    // SHOOT
    // -------------------------

    private void ShootUpdate()
    {
        UpdateLookDirectionToPlayer();

        if (currentTypeOfAttack == TypeOfAttack.RUN)
        {
            if (isPlayerVisible)
            {
                Velocity = moveDirection * runSpeed;
                transform.position += Velocity * Time.deltaTime;
            }
            else
            {
                Velocity = Vector3.zero;
                ChangeState(CyclopsState.DETECTION);
            }
        }
        else
        {
            GetComponent<Rigidbody>().linearDamping = pushBackResistance;
            Velocity = Vector3.zero;
        }

        if (!isShooting && isPlayerVisible)
            StartCoroutine(ShootCO());
    }

    private IEnumerator ShootCO()
    {
        isShooting = true;

        yield return new WaitForSeconds(timeToShoot);

        if (currentTypeOfAttack == TypeOfAttack.RUN)
        {
            Shoot();
            yield return new WaitForSeconds(timeToReload);
        }
        else
        {
            yield return StartCoroutine(StandAttackCO());
        }

        isShooting = false;

        UpdateDistanceFromPlayer();
        ChangeState(
            distanceFromPlayer <= detectionDistance && isPlayerVisible
                ? CyclopsState.DETECTION
                : CyclopsState.PATROL
        );
    }

    private IEnumerator StandAttackCO()
    {
        Velocity = Vector3.zero;

        for (int i = 0; i < standAttackShots; i++)
        {
            Shoot();
            
            if (isPlayerVisible)
            {
                yield return new WaitForSeconds(0.25f);
            }
            else
            {
                yield break;
            }
        }
    }

    // -------------------------
    // SHOOT LOGIC
    // -------------------------

    private void Shoot()
    {
        GameObject newBullet = BulletPoolingManager.instance.GetPooledEnemyBullet();
        Bullet bulletComp = newBullet.GetComponent<Bullet>();

        bulletComp.BulletShoot(
            transform.position,
            (lastSeenPlayerPosition - transform.position).normalized,
            bulletSpeed,
            bulletRange,
            atk,
            bulletSize,
            projectilePushBackForce
        );
    }

    // -------------------------
    // UTILS
    // -------------------------

    private void UpdateDistanceFromPlayer()
    {
        if (Player.instance == null)
        {
            distanceFromPlayer = float.MaxValue;
            return;
        }

        distanceFromPlayer = Vector3.Distance(
            Player.instance.transform.position,
            transform.position
        );
    }

    private TypeOfAttack RandomizeTypeOfShoot()
    {
        return Random.value < 0.5f ? TypeOfAttack.RUN : TypeOfAttack.STAND;
    }

    private void UpdateLookDirectionToPlayer()
    {
        if (Player.instance == null) return;

        Vector3 dir = Player.instance.transform.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.001f)
            animator.SetLookDirection(dir.normalized);
    }

    private void OnCollisionEnter(Collision collision)
    {
        int otherLayer = collision.gameObject.layer;

        if (otherLayer == LayerMask.NameToLayer("Wall") ||
        otherLayer == LayerMask.NameToLayer("Obstacle"))
        {
            BounceFromWall();
            return;
        }

        // collisione con un altro enemy
        if (((1 << otherLayer) & enemyLayer) != 0)
        {
            BounceFromEnemy(collision);
        }

        if (collision.gameObject.TryGetComponent(out Player player))
        {
            player.TakeDamage(atk);
            player.OnPush();
            PushSomething(collision, pushBackForce);
        }
    }

    private void BounceFromEnemy(Collision collision)
    {
        if (bounceLockTimer > 0f)
            return;

        Vector3 normal = collision.contacts[0].normal;
        normal.y = 0f;

        if (normal.sqrMagnitude < 0.001f)
            normal = -moveDirection;

        // riflessione tipo billiardo
        Vector3 newDir = Vector3.Reflect(moveDirection, normal).normalized;

        // piccola randomizzazione per evitare simmetrie
        newDir += new Vector3(
            Random.Range(-0.2f, 0.2f),
            0f,
            Random.Range(-0.2f, 0.2f)
        );

        moveDirection = newDir.normalized;
        transform.forward = moveDirection;

        // micro spinta per staccarsi
        transform.position += moveDirection * 0.2f;

        bounceLockTimer = 0.25f;
        timeToChangeDirection = cyclopsSO.timeToChangeDirection;
    }

    protected override void Die()
    {
        if (isDead)
            return;

        isDead = true;

        // STOP COROUTINE
        StopAllCoroutines();
        ResetAnimator();
        anim.SetBool("IsDead", true);

        base.Die();

        if (TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero; // Unity 6
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }

    public override void DealDamage(GameObject target) { }
    public override void TakeDamage(float amount)
    {
        currentHP -= amount;
        if (currentHP <= 0)
        {
            Die();
            return;
        }

        if (isTakingDamage || isDead)
            return;

        if (gameObject.activeInHierarchy)
            StartCoroutine(TakeDamageCO());
    }

    private IEnumerator TakeDamageCO()
    {
        if (gameObject == null)
            yield break;

        isTakingDamage = true;
        yield return new WaitForSeconds(0.25f);
        isTakingDamage = false;
    }
    private void SetupSOStats()
    {
        maxHP = cyclopsSO.maxHP;
        currentHP = cyclopsSO.currentHP;
        atk = cyclopsSO.atk;
        patrolSpeed = cyclopsSO.patrolSpeed;
        runSpeed = cyclopsSO.runSpeed;
        detectionDistance = cyclopsSO.detectionDistance;
        timeToChangeDirection = cyclopsSO.timeToChangeDirection;
        timeToAim = cyclopsSO.timeToAim;
        timeToShoot = cyclopsSO.timeToShoot;
        timeToReload = cyclopsSO.timeToReload;
        bulletSpeed = cyclopsSO.bulletSpeed;
        bulletRange = cyclopsSO.bulletRange;
        bulletSize = cyclopsSO.bulletSize;
        projectilePushBackForce = cyclopsSO.projectilePushBackForce;
        pushBackForce = cyclopsSO.pushBackForce;
        pushBackResistance = cyclopsSO.pushBackResistance;
    }

    public override void ResetBehaviourVariables()
    {
        rb.isKinematic = false;
        rb.detectCollisions = true;
        rb.useGravity = false;

        isTakingDamage = false;
        isDead = false;
        isShooting = false;
        isAiming = false;
        bounceLockTimer = 1;

        isPlayerVisible = false;
        forgetPlayerTimer = 3;

        timeToChangeDirection = 0;
    }
}
