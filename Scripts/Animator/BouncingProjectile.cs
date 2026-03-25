using UnityEngine;

public class BouncingProjectile : MonoBehaviour
{
    [Header("Heights")]
    [SerializeField] float bounceDown = 2f;   // quanto scende sotto la Y base
    [SerializeField] float bounceUp = 2f;     // quanto sale sopra la Y base

    [Header("Speed")]
    [SerializeField] float bounceSpeed = 6f;

    [Header("Explosion")]
    [SerializeField] float explosionScale = 5f;
    [SerializeField] float explosionDuration = 0.2f;

    [SerializeField] PlayerBullet parentBullet;

    float baseY;
    float minY;
    float maxY;

    public bool exploding = false;

    Vector3 originalScale;

    TrailRenderer trail;

    bool goingDown = true;

    void Awake()
    {
        trail = GetComponent<TrailRenderer>();

        originalScale = transform.localScale;

        baseY = transform.localPosition.y;
        minY = baseY - bounceDown;
        maxY = baseY + bounceUp;
    }

    void Update()
    {
        if (exploding) return;

        float step = bounceSpeed * Time.deltaTime;

        if (goingDown)
        {
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition,
                new Vector3(0, minY, 0),
                step
            );

            if (Mathf.Abs(transform.localPosition.y - minY) < 0.01f)
                goingDown = false;
        }
        else
        {
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition,
                new Vector3(0, maxY, 0),
                step
            );

            if (Mathf.Abs(transform.localPosition.y - maxY) < 0.01f)
                goingDown = true;
        }
    }
    void LateUpdate()
    {
        if (exploding)
        {
            float t = Mathf.Clamp01(Time.deltaTime / explosionDuration);

            transform.localScale = Vector3.Lerp(transform.localScale, originalScale * explosionScale, t);
        }
    }

    public void OnExplosionStart()
    {
        exploding = true;

        Invoke(nameof(OnExplosionEnd), explosionDuration);
    }

    private void OnExplosionEnd()
    {
        exploding = false;

        parentBullet.DisableBullet();
    }

    private void OnEnable()
    {
        trail?.Clear();
    }

    private void OnDisable()
    {
        goingDown = true;

        transform.localPosition = Vector3.zero;

        transform.localScale = originalScale;

        transform.localRotation = Quaternion.identity;
    }
}
