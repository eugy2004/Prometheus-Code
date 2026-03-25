using System.Collections;
using UnityEngine;

public class SpriteFlipStretch : MonoBehaviour
{
    [SerializeField] private Transform graphics;

    [Header("Flip Stretch")]
    [SerializeField] private float flipYScale = 1.08f;
    [SerializeField] private float flipReturnDuration = 0.2f;
    [SerializeField]
    private AnimationCurve returnCurve =
        AnimationCurve.EaseInOut(0, 0, 1, 1);

    private int lastFlipSign = 1;
    private Coroutine flipCoroutine;

    /// <summary>
    /// Chiama questo metodo passando la direzione X/Y (o X/Z)
    /// </summary>
    public void HandleFlip(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) < 0.1f || Time.timeScale == 0)
            return;

        int newSign = dir.x > 0 ? 1 : -1;

        if (newSign != lastFlipSign)
        {
            lastFlipSign = newSign;
            ApplyFlip(newSign);
        }
    }

    private void ApplyFlip(int sign)
    {
        if (flipCoroutine != null)
            StopCoroutine(flipCoroutine);

        // flip immediato + stretch
        graphics.localScale = new Vector3(sign, flipYScale, 1f);

        flipCoroutine = StartCoroutine(ReturnYScale());
    }

    private IEnumerator ReturnYScale()
    {
        float t = 0f;

        while (t < flipReturnDuration)
        {
            t += Time.deltaTime;
            float normalized = t / flipReturnDuration;

            float curveValue = returnCurve.Evaluate(normalized);
            float y = Mathf.Lerp(flipYScale, 1f, curveValue);

            graphics.localScale = new Vector3(
                graphics.localScale.x,
                y,
                1f
            );

            yield return null;
        }

        graphics.localScale = new Vector3(
            graphics.localScale.x,
            1f,
            1f
        );

        flipCoroutine = null;
    }
}
