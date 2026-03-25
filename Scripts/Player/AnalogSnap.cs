using UnityEngine;
using UnityEngine.InputSystem;

public class AnalogSnap : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private string actionName = "Move";

    [Header("Snap Settings")]
    [Range(0f, 0.5f)]
    public float deadzone = 0.15f;

    [Range(4, 16)]
    public int directions = 8;

    public Vector2 SnappedValue { get; private set; }

    private InputAction action;
    private float step;

    private void Awake()
    {
        action = playerInput.currentActionMap.FindAction(actionName);
        step = 360f / directions;
    }

    private void Update()
    {
        Vector2 rawInput = action.ReadValue<Vector2>();

        // Se NON è gamepad → niente snap
        if (!(action.activeControl?.device is Gamepad))
        {
            SnappedValue = rawInput;
            return;
        }

        if (rawInput.magnitude < deadzone)
        {
            SnappedValue = Vector2.zero;
            return;
        }

        SnappedValue = SnapVector(rawInput);
    }

    private Vector2 SnapVector(Vector2 input)
    {
        float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
        float snappedAngle = Mathf.Round(angle / step) * step;
        float rad = snappedAngle * Mathf.Deg2Rad;

        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }
}
