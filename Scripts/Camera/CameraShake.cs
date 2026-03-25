using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake instance;

    private Vector3 originalPos;
    private Coroutine shakeCO;

    public Vector3 OriginalPos { get => originalPos; set => originalPos = value; }

    private void Awake()
    {
        instance = this;
        originalPos = transform.localPosition;
    }

    public void Shake(float intensity, float duration)
    {
        if (shakeCO != null)
            StopCoroutine(shakeCO);

        shakeCO = StartCoroutine(ShakeCO(intensity, duration));
    }

    private IEnumerator ShakeCO(float intensity, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            Vector2 offset = Random.insideUnitCircle * intensity;
            transform.localPosition = originalPos + new Vector3(offset.x, offset.y, 0f);

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }
}

