using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LogoManager : MonoBehaviour
{
    private float timer = 15;

    private bool isLogoSkipped = false;

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0 && !isLogoSkipped)
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void SkipLogo()
    {
        isLogoSkipped = true;
        SceneManager.LoadScene("MainMenu");
    }
}
