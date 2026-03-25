using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Animator cameraAnim;

    [SerializeField] private string nextSceneName;

    public bool isGameStarted = false;
    private bool hasAnimationPlayed = false;

    private void Start()
    {
        StartCoroutine(WaitCameraAnim());
    }

    public void StartGame(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (isGameStarted) { SceneManager.LoadScene(nextSceneName); }
            if (!hasAnimationPlayed) return;
            cameraAnim.SetTrigger("Start");
            StartCoroutine(WaitAnim());
        }
    }

    private IEnumerator WaitAnim()
    {
        yield return new WaitForSeconds(1);
        isGameStarted = true;
        yield return new WaitForSeconds(19);
        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator WaitCameraAnim()
    {
        yield return new WaitForSeconds(5);
        hasAnimationPlayed = true;
    }
}