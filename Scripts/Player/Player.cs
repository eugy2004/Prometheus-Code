using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    public static Player instance;
    public Animator anim;
    public PlayerAnimatorController animatorController;

    public Vector2 MoveInput => moveInput;

    private AnalogSnap analogSnap;
    private InputDevice dev;

    private PrometheusSounds prometheusSounds;

    private List<Vector2> previousMoveInputs;

    [SerializeField]
    private PrometheusStats baseStats;
    [SerializeField]
    private PrometheusStats currentStats;

    public PrometheusStats CurrentStats => currentStats;

    [SerializeField]
    private WeaponGeneralStats equippedWeaponStats;
    [SerializeField]
    private SpriteRenderer hairFire;

    public WeaponGeneralStats EquippedWeaponStats { get; set; }

    private Rigidbody rb;
    private float elapsedTimeSinceDashPerformed = 0f;
    private float elapsedTimeSinceLastShot = 0f;
    private float elapsedTimeSinceLastDamageTaken = 0f;
    private float elapsedTimeSinceLastHeatDecrease = 0f;


    private Vector2 moveInput;
    private Vector2 shootInput;
    private Vector2 aimDirection = Vector2.right;
    private Vector3 moveDirection;
    private Vector2 dashDirection;
    private Vector3 dashVector;
    private Vector2 lockedDirection = Vector2.down;
    public Vector2 AimDirection => aimDirection;


    private bool locked = false;
    private bool holdingShoot = false;
    private bool checkingShootInputs = false;
    public bool CheckingShootInputs => checkingShootInputs;
    private bool isDpad = false;
    private bool isBeingPushed = false;
    private bool isDashing;
    public bool IsDashing => isDashing;

    private bool isSurviving = false;

    private int shootInputSkip = 0;

    private float dashTimer;
    private float dashDuration;

    public bool IsBeingPushed => isBeingPushed;
    public bool IsHoldingShoot => holdingShoot;


    public bool IsAimingLocked => locked;
    public Vector2 LockedDirection => lockedDirection; //queste due mi servono per lockare lo sprite in dirzione di mira. Ahmed
    public List<Vector2> PreviousMoveInputs => previousMoveInputs;

    [SerializeField]
    private BaseWeapon equippedWeapon;
    
    public BaseWeapon EquippedWeapon { get { return equippedWeapon; } set {equippedWeapon = value; } }

    [SerializeField] GameObject afterImagePrefab;
    [SerializeField] float afterImageRate = 0.05f;

    float afterImageTimer;


    private void Awake()
    {
        instance = this;

        equippedWeapon = new BaseWeapon();
        equippedWeapon.InstantiateWeapon(equippedWeaponStats);

        shootInput = Vector2.zero;
        previousMoveInputs = new List<Vector2>();

        InitializeStats();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        analogSnap = GetComponent<AnalogSnap>();
        dashDuration = currentStats.dashDistance / currentStats.dashPower;

        prometheusSounds = AudioManager.instance.soundLibrary.prometheusSounds;
    }

    private void Update()
    {
        TimerVariablesIncrease();
        UpdateAimDirection();
        AutomaticHeatDecrease();
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            HandleAfterImage();
            dashTimer += Time.fixedDeltaTime;

            if (dashTimer >= dashDuration)
            {
                isDashing = false;
            }

            return; // 🔒 durante il dash niente movimento normale
        }

        if (!isBeingPushed)
        {
            moveDirection =
                (Vector3.right * moveInput.x +
                 Vector3.forward * moveInput.y) * currentStats.moveSpeed;

            rb.linearVelocity = moveDirection;

            if (!checkingShootInputs)
                ShootCheck();
        }
        else
        {
            if (elapsedTimeSinceLastDamageTaken >= currentStats.stunDuration)
            {
                isBeingPushed = false;
                rb.linearDamping = 0;
            }
        }

        DiagonalMovementCheck();
    }

    private void UpdateAimDirection()
    {
        if (shootInput != Vector2.zero)
        {
            if (checkingShootInputs)
            {
                shootInputSkip++;
                if (shootInputSkip >= 5)
                {
                    shootInputSkip--;
                    aimDirection = shootInput.normalized;
                    checkingShootInputs = false;
                }
            }
            else             
            {
                aimDirection = shootInput.normalized;
            }
        }
        else
        {
            shootInputSkip = 0;
            checkingShootInputs = true;
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        dev = context.control.device;

        if (dev is Gamepad)
        {
            isDpad = context.control.path.Contains("dpad");

            if (!isDpad)   // ← Snap solo per l'analogico
            {
                if (moveInput != Vector2.zero)
                    moveInput = analogSnap.SnappedValue;
            }
        }

        if (!locked && !holdingShoot && moveInput != Vector2.zero)
            lockedDirection = moveInput;
    }

    public void MoveWithJIKLEnabled(InputAction.CallbackContext context)
    {
        if (!GameManager.instance.playerControl) return;

        moveInput = context.ReadValue<Vector2>();

        dev = context.control.device;

        if (dev is Gamepad)
        {
            isDpad = context.control.path.Contains("dpad");

            if (!isDpad)   // ← Snap solo per l'analogico
            {
                if (moveInput != Vector2.zero)
                    moveInput = analogSnap.SnappedValue;
            }
        }
    }

    public void Shoot(InputAction.CallbackContext context)
    {
        if (!GameManager.instance.playerControl) return;

        if (context.performed)
        {
            holdingShoot = true;
        }
        if (context.canceled)
        {
            holdingShoot = false;
        }

        lockedDirection = aimDirection;
    }

    public Vector2 VisualDirection
    {
        get
        {
            if (locked)
                return lockedDirection;

            if (moveInput != Vector2.zero)
                return moveInput;

            if (previousMoveInputs.Count > 0)
                return previousMoveInputs[0];

            return Vector2.down;
        }
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (!GameManager.instance.playerControl) return;

        afterImageTimer = 0f;

        if (
            context.performed &&
            !isDashing &&
            elapsedTimeSinceDashPerformed >= currentStats.dashCooldown &&
            moveInput != Vector2.zero
        )
        {
            elapsedTimeSinceDashPerformed = 0f;
            dashTimer = 0f;
            isDashing = true;

            AudioManager.instance.CreateSound()
                .WithSoundData(prometheusSounds.dashSound)
                .WithPosition(transform.position)
                .WithRandomPitch(true)
                .BuildAndPlay(transform);

            dashDirection = moveInput.normalized;
            dashVector =
                (Vector3.right * dashDirection.x +
                 Vector3.forward * dashDirection.y) * currentStats.dashPower;

            rb.AddForce(dashVector, ForceMode.Impulse);
        }
    }

    void HandleAfterImage()
    {
        afterImageTimer -= Time.deltaTime;

        if (afterImageTimer <= 0f)
        {
            GameObject img = Instantiate(
                afterImagePrefab,
                transform.position,
                Quaternion.identity
            );

            // Flip in base alla direzione del dash
            float xDir = dashDirection.x != 0 ? Mathf.Sign(dashDirection.x) : 1f;

            img.transform.localScale = new Vector3(
                xDir,
                1f,
                1f
            );

            afterImageTimer = afterImageRate;
        }
    }


    public void AimLock(InputAction.CallbackContext context)
    {
        if (context.performed)
            locked = true;

        if (context.canceled)
        {
            locked = false;

            // riallinea subito la direzione al movimento corrente
            if (moveInput != Vector2.zero)
                lockedDirection = moveInput;
        }
    }

    public void DirectionalShoot(InputAction.CallbackContext context)
    {
        shootInput = context.ReadValue<Vector2>();   // ⭐ QUESTA RIGA

        if (context.performed)
            holdingShoot = true;

        if (context.canceled)
            holdingShoot = false;
    }

    private void ShootCheck()
    {
        if (holdingShoot && elapsedTimeSinceLastShot > equippedWeapon.weaponStats.fireCooldown)
        {
            elapsedTimeSinceLastShot = 0f;
            equippedWeapon.SetDirection(transform, aimDirection);
            anim.GetComponentInChildren<WeaponAnimatorController>().PlayThrowAnimation();
        }
    }

    public void TakeDamage(float damage)
    {
        if (elapsedTimeSinceLastDamageTaken > currentStats.invicibilityTime)
        {
            LowerHeat(damage);
            elapsedTimeSinceLastDamageTaken = 0f;

            HitStopManager.instance.DoHitStop(0.06f, 0f);
            CameraShake.instance.Shake(0.1f, 0.15f);
        }
    }

    public void OnPush()
    {
        isBeingPushed = true;
        rb.linearDamping = currentStats.pushBackResistance;
    }

    private void LowerHeat(float amount)
    {
        currentStats.currentHeat -= amount;

        if (currentStats.currentHeat <= 0)
        {
            if (!isSurviving)
            {
                currentStats.currentHeat = 0;
                isSurviving = true;
                Debug.Log("Player is now surviving!");
                hairFire.enabled = false;
            }
            else
            {
                animatorController.enabled = false;
                ResetAnimator();
                anim.SetTrigger("IsDead");
                GameManager.instance.OnGameEnd();
                enabled = false;
            }
        }
    }

    public void RestoreHeat(int amount)
    {
        currentStats.currentHeat += amount;
        isSurviving = false;
        hairFire.enabled = true;

        if (currentStats.currentHeat > currentStats.maxHeat)
        {
            currentStats.currentHeat = currentStats.maxHeat;
        }
    }

    public void IncreaseMaxHeat(int amount)
    {
        currentStats.maxHeat += amount;
        currentStats.currentHeat += amount;
    }

    public float GetCurrentHeat()
    {
        return currentStats.currentHeat;
    }

    public float GetMaxHeat()
    {
        return currentStats.maxHeat;
    }

    private void AutomaticHeatDecrease()
    {
        if (elapsedTimeSinceLastHeatDecrease >= currentStats.timeUntilAutomaticHeatDecrease 
            && !isSurviving
            && GameManager.instance.CurrentRoomInfo != null)
        {
            LowerHeat(currentStats.automaticHeatDecreaseValue);
            elapsedTimeSinceLastHeatDecrease = 0f;
        }
    }

    public void RestartPlayer() { }

    private void InitializeStats() 
    {         
        currentStats.maxHeat = baseStats.maxHeat;
        currentStats.currentHeat = baseStats.maxHeat;
        currentStats.moveSpeed = baseStats.moveSpeed;
        currentStats.dashPower = baseStats.dashPower;
        currentStats.dashCooldown = baseStats.dashCooldown;
        currentStats.dashDistance = baseStats.dashDistance;
        currentStats.stunDuration = baseStats.stunDuration;
        currentStats.invicibilityTime = baseStats.invicibilityTime;
        currentStats.pushBackResistance = baseStats.pushBackResistance;
        currentStats.timeUntilAutomaticHeatDecrease = baseStats.timeUntilAutomaticHeatDecrease;
        currentStats.automaticHeatDecreaseValue = baseStats.automaticHeatDecreaseValue;
    }

    private void DiagonalMovementCheck()
    {
        if (moveInput != Vector2.zero)
        {
            ShiftInputList();
        }
        else if (previousMoveInputs.Count > 0)
        {
            StoppingDirectionInputCheck();
            previousMoveInputs.Clear();
        }
    }

    private void ShiftInputList()
    {
        previousMoveInputs.Add(moveInput);
        if (previousMoveInputs.Count > 3)
        {
            previousMoveInputs.RemoveAt(0);
        }
    }

    private void StoppingDirectionInputCheck()
    {
        if (locked)
            return;

        Vector2 lastValidInput = previousMoveInputs[0];

        if (lastValidInput.x != 0 && lastValidInput.y != 0)
            lockedDirection = lastValidInput.normalized;
    }

    private void TimerVariablesIncrease()
    {
        if (elapsedTimeSinceDashPerformed < 256)
            elapsedTimeSinceDashPerformed += Time.deltaTime;

        if (elapsedTimeSinceLastShot < 256)
            elapsedTimeSinceLastShot += Time.deltaTime;

        if (elapsedTimeSinceLastDamageTaken < 256)
            elapsedTimeSinceLastDamageTaken += Time.deltaTime;

        if (elapsedTimeSinceLastHeatDecrease < 256)
            elapsedTimeSinceLastHeatDecrease += Time.deltaTime;
    }

    private void ResetAnimator()
    {
        anim.SetFloat("Speed", -10);
        anim.SetInteger("View", -10);
        anim.SetBool("TakeDamage", false);
        anim.SetBool("IsDashing", false);
    }
}
