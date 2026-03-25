using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class QuitInputCombo : MonoBehaviour
{
    public InputActionReference startInput;
    public InputActionReference selectInput;

    public float comboWindow = 0.03f;

    private float lastStartTime = -100f;
    private float lastSelectTime = -100f;
    private bool comboTriggered = false;

    private void OnEnable()
    {
        startInput.action.performed += OnStartInput;
        selectInput.action.performed += OnSelectInput;

        startInput.action.Enable();
        selectInput.action.Enable();
    }

    private void OnDisable()
    {
        startInput.action.performed -= OnStartInput;
        selectInput.action.performed -= OnSelectInput;

        startInput.action.Disable();
        selectInput.action.Disable();
    }

    private void OnStartInput(InputAction.CallbackContext ctx)
    {
        lastStartTime = Time.time;
        CheckCombo();
    }

    private void OnSelectInput(InputAction.CallbackContext ctx)
    {
        lastSelectTime = Time.time;
        CheckCombo();
    }

    private void CheckCombo()
    {
        // Se le azioni sono vicine nel tempo e la combo non è stata già triggerata
        if (!comboTriggered && Mathf.Abs(lastStartTime - lastSelectTime) <= comboWindow)
        {
            comboTriggered = true;
            TriggerCombo();
        }
    }

    private void Update()
    {
        // Resetta il flag quando entrambe le azioni non sono più premute
        if (startInput.action.ReadValue<float>() == 0 && selectInput.action.ReadValue<float>() == 0)
        {
            comboTriggered = false;
        }
    }

    private void TriggerCombo()
    {
        Debug.Log("InputCombo");
        Application.Quit();
        // Qui puoi mettere l’effetto reale della combo, es:
        // Player.instance.TriggerSpecialAttack();
    }
}
