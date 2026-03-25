using UnityEngine;
using UnityEngine.InputSystem;

public class InputCombo : MonoBehaviour
{
    public InputActionReference actionShoot; // Azione Shoot
    public InputActionReference actionDash;  // Azione Dash

    public float comboWindow = 0.03f;

    private float lastShootTime = -100f;
    private float lastDashTime = -100f;
    private bool comboTriggered = false;

    private void OnEnable()
    {
        actionShoot.action.performed += OnShoot;
        actionDash.action.performed += OnDash;

        actionShoot.action.Enable();
        actionDash.action.Enable();
    }

    private void OnDisable()
    {
        actionShoot.action.performed -= OnShoot;
        actionDash.action.performed -= OnDash;

        actionShoot.action.Disable();
        actionDash.action.Disable();
    }

    private void OnShoot(InputAction.CallbackContext ctx)
    {
        lastShootTime = Time.time;
        CheckCombo();
    }

    private void OnDash(InputAction.CallbackContext ctx)
    {
        lastDashTime = Time.time;
        CheckCombo();
    }

    private void CheckCombo()
    {
        // Se le azioni sono vicine nel tempo e la combo non è stata già triggerata
        if (!comboTriggered && Mathf.Abs(lastShootTime - lastDashTime) <= comboWindow)
        {
            comboTriggered = true;
            TriggerCombo();
        }
    }

    private void Update()
    {
        // Resetta il flag quando entrambe le azioni non sono più premute
        if (actionShoot.action.ReadValue<float>() == 0 && actionDash.action.ReadValue<float>() == 0)
        {
            comboTriggered = false;
        }
    }

    private void TriggerCombo()
    {
        Debug.Log("InputCombo");
        // Qui puoi mettere l’effetto reale della combo, es:
        // Player.instance.TriggerSpecialAttack();
    }
}
