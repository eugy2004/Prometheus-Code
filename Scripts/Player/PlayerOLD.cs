using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerOLD : MonoBehaviour
{
    public static PlayerOLD instance;

    public Vector2 MoveInput => moveInput;

    private AnalogSnap analogSnap;
    private InputDevice dev;

    private PrometheusSounds prometheusSounds;

    private List<Vector2> previousMoveInputs;

    [SerializeField]
    private PrometheusStats baseStats;
    [SerializeField]
    private PrometheusStats currentStats;

    [SerializeField]
    private WeaponGeneralStats equippedWeaponStats;

    public WeaponGeneralStats EquippedWeaponStats { get; set; }

    private Rigidbody rb;
    private float elapsedTimeSinceDashPerformed = 0f;
    private float elapsedTimeSinceLastShot = 0f;
    private float elapsedTimeSinceLastDamageTaken = 0f;
    private float elapsedTimeSinceLastHeatDecrease = 0f;

    private Vector2 moveInput;
    private Vector3 moveDirection;
    private Vector2 dashDirection;
    private Vector3 dashVector;
    private Vector2 lockedDirection = Vector2.down;
    private Vector2 directionalShootDirection;

    private bool isNotDashing = true;
    private bool locked = false;
    private bool holdingShoot = false;
    private bool isDpad = false;
    private bool isBeingPushed = false;
    public bool IsBeingPushed => isBeingPushed;
    public bool IsHoldingShoot => holdingShoot;


    public bool IsAimingLocked => locked;
    public Vector2 LockedDirection => lockedDirection; //queste due mi servono per lockare lo sprite in direzione di mira. Ahmed
    public List<Vector2> PreviousMoveInputs => previousMoveInputs;

    [SerializeField]
    private BaseWeapon equippedWeapon;
    
    public BaseWeapon EquippedWeapon { get { return equippedWeapon; } set {equippedWeapon = value; } }

    private void Awake()
    {
        instance = this;

        equippedWeapon = new BaseWeapon();
        equippedWeapon.InstantiateWeapon(equippedWeaponStats);

        previousMoveInputs = new List<Vector2>();

        InitializeStats();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        analogSnap = GetComponent<AnalogSnap>();

        prometheusSounds = AudioManager.instance.soundLibrary.prometheusSounds;
    }

    private void Update()
    {
        TimerVariablesIncrease();
        AutomaticHeatDecrease();
    }

    private void FixedUpdate()
    {
        isNotDashing = elapsedTimeSinceDashPerformed >= currentStats.dashDistance / currentStats.dashPower;

        if (!isBeingPushed)
        {
            if (isNotDashing)
            {
                moveDirection = (Vector3.right * moveInput.x + Vector3.forward * moveInput.y) * currentStats.moveSpeed;
                rb.linearVelocity = moveDirection;
                ShootCheck();
            }
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

        if (!locked && moveInput != Vector2.zero)
            lockedDirection = moveInput;
    }

    public void MoveWithJIKLEnabled(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void Shoot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            holdingShoot = true;
        }
        if (context.canceled)
        {
            holdingShoot = false;
        }
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
        if (
            context.performed &&
            elapsedTimeSinceDashPerformed >= currentStats.dashCooldown &&
            moveInput != Vector2.zero
        )
        {
            elapsedTimeSinceDashPerformed = 0f;
            
            AudioManager.instance.CreateSound()
                .WithSoundData(prometheusSounds.dashSound)
                .WithPosition(transform.position)
                .WithRandomPitch(true)
                .BuildAndPlay(transform);

            dashDirection = moveInput.normalized;
            dashVector = (Vector3.right * dashDirection.x + Vector3.forward * dashDirection.y) * currentStats.dashPower;
            rb.AddForce(dashVector, ForceMode.Impulse);
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
        directionalShootDirection = context.ReadValue<Vector2>();

        if (context.performed)
        {
            holdingShoot = true;
        }
        if (context.canceled)
        {
            holdingShoot = false;
        }

        if (!locked && directionalShootDirection != Vector2.zero)
            lockedDirection = directionalShootDirection;
    }

    private void ShootCheck()
    {
        if (holdingShoot && elapsedTimeSinceLastShot > equippedWeapon.weaponStats.fireCooldown)
        {
            elapsedTimeSinceLastShot = 0f;
            equippedWeapon.SetDirection(transform, lockedDirection);
        }
    }

    public void TakeDamage(float damage)
    {
        if (elapsedTimeSinceLastDamageTaken > currentStats.invicibilityTime)
        {
            isBeingPushed = true;
            LowerHeat(damage);
            elapsedTimeSinceLastDamageTaken = 0f;

            // ------------------
            // FEEDBACK
            // ------------------
            HitStopManager.instance?.DoHitStop(0.06f, 0f);
            CameraShake.instance?.Shake(0.06f, 0.06f);
        }
    }


    private void LowerHeat(float amount)
    {
        currentStats.currentHeat -= amount;

        if (currentStats.currentHeat <= 0)
        {
            currentStats.currentHeat = 0;
            // Handle player death (e.g., trigger game over)
        }
    }

    public void RestoreHeat(int amount)
    {
        currentStats.currentHeat += amount;

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
        if (true)
        {
            LowerHeat(currentStats.automaticHeatDecreaseValue);
            elapsedTimeSinceLastHeatDecrease = 0f;
        }
    }

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
}
