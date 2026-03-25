using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject optionsPanel;

    [Header("Pause Buttons")]
    public Button playButton;
    public Button optionsButton;
    public Button exitButton;

    [Header("Options")]
    public Slider volumeSlider;
    public Button backButton;

    bool isPaused = false;

    List<Selectable> pauseButtons = new List<Selectable>();
    List<Selectable> optionsButtons = new List<Selectable>();

    void Start()
    {
        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);

        pauseButtons.Clear();
        pauseButtons.Add(playButton);
        pauseButtons.Add(optionsButton);
        pauseButtons.Add(exitButton);

        optionsButtons.Clear();
        optionsButtons.Add(volumeSlider);
        optionsButtons.Add(backButton);

        volumeSlider.onValueChanged.AddListener(SetVolume);

        playButton.onClick.AddListener(ResumeGame);
        optionsButton.onClick.AddListener(OpenOptions);
        exitButton.onClick.AddListener(ExitGame);
        backButton.onClick.AddListener(CloseOptions);
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }

        if (!isPaused) return;

        bool up = Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame;
        bool down = Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame;
        bool left = Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame;
        bool right = Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame;
        bool confirm = Keyboard.current.enterKey.wasPressedThisFrame;

        if (optionsPanel.activeSelf)
        {
            if (up) MoveSelection(optionsButtons, -1);
            if (down) MoveSelection(optionsButtons, 1);

            GameObject current = EventSystem.current.currentSelectedGameObject;

            if (current == volumeSlider.gameObject)
            {
                if (left) volumeSlider.value -= 0.05f;
                if (right) volumeSlider.value += 0.05f;
            }
        }
        else
        {
            if (up) MoveSelection(pauseButtons, -1);
            if (down) MoveSelection(pauseButtons, 1);
        }

        if (confirm)
        {
            GameObject current = EventSystem.current.currentSelectedGameObject;

            if (current != null)
            {
                Button button = current.GetComponent<Button>();

                if (button != null)
                {
                    button.onClick.Invoke();
                }
            }
        }
    }

    void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    void PauseGame()
    {
        isPaused = true;

        pausePanel.SetActive(true);
        optionsPanel.SetActive(false);

        Time.timeScale = 0f;

        SelectUI(playButton.gameObject);
    }

    void ResumeGame()
    {
        isPaused = false;

        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);

        Time.timeScale = 1f;

        EventSystem.current.SetSelectedGameObject(null);
    }

    void OpenOptions()
    {
        pausePanel.SetActive(false);
        optionsPanel.SetActive(true);

        SelectUI(volumeSlider.gameObject);
    }

    void CloseOptions()
    {
        optionsPanel.SetActive(false);
        pausePanel.SetActive(true);

        SelectUI(optionsButton.gameObject);
    }

    void ExitGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    void SetVolume(float value)
    {
        AudioListener.volume = value;
    }

    void MoveSelection(List<Selectable> list, int direction)
    {
        GameObject current = EventSystem.current.currentSelectedGameObject;

        int index = list.FindIndex(x => x.gameObject == current);

        if (index == -1)
            index = 0;
        else
            index = (index + direction + list.Count) % list.Count;

        SelectUI(list[index].gameObject);
    }

    void SelectUI(GameObject obj)
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(obj);
    }
}