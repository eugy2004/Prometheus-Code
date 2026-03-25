using UnityEngine;

public class SniperBullet : Bullet
{
    private TrailRenderer trailRenderer;
    [Header("Explosion")]
    [SerializeField] float explosionScale = 5f;
    [SerializeField] float explosionDuration = 0.2f;

    public bool exploding = false;

    Vector3 originalScale;


    private void Start()
    {
        trailRenderer = GetComponent<TrailRenderer>();
    }

    float explosionTimer;

    void LateUpdate()
    {
        if (!exploding) return;

        explosionTimer += Time.deltaTime;
        float t = explosionTimer / explosionDuration;

        transform.localScale = Vector3.Lerp(
            originalScale,
            originalScale * explosionScale,
            t
        );
    }


    public override void OnEnable()
    {
        base.OnEnable();

        originalScale = transform.localScale;
        exploding = false;

        if (trailRenderer != null)
            trailRenderer.Clear();
    }


    public override void OnDisable()
    {
        base.OnDisable();
        transform.localScale = originalScale;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out Enemy enemy))
        {
            enemy.TakeDamage(bulletDamage);
        }

        StartExplosion();
    }


    public void OnExplosionStart()
    {
        exploding = true;

        Invoke(nameof(OnExplosionEnd), explosionDuration);
    }

    private void OnExplosionEnd()
    {
        exploding = false;
        DisableBullet();
    }


    void StartExplosion()
    {
        exploding = true;
        explosionTimer = 0f;

        Invoke(nameof(OnExplosionEnd), explosionDuration);
    }


}
