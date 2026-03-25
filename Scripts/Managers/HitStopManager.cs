using System.Collections;
using UnityEngine;

public class HitStopManager : MonoBehaviour
{
    public static HitStopManager instance;

    private Coroutine hitStopCO;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void DoHitStop(float duration, float timeScale = 0f)
    {
        if (hitStopCO != null)
            StopCoroutine(hitStopCO);

        hitStopCO = StartCoroutine(HitStopCO(duration, timeScale));
    }

    private IEnumerator HitStopCO(float duration, float timeScale)
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = timeScale;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        Time.timeScale = originalTimeScale;
    }
}

