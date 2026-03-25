using UnityEngine;

public class PlayerBullet : Bullet
{
    BouncingProjectile bounce;
    private TrailRenderer trailRenderer;
    
    void Start()
    {
        bounce = GetComponentInChildren<BouncingProjectile>();
        trailRenderer = GetComponentInChildren<TrailRenderer>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        col.enabled = false;
        if (collision.gameObject.TryGetComponent(out Enemy enemy))
        {
            enemy.OnPush(collision.gameObject, myPushBackForce * collision.rigidbody.mass);
            enemy.TakeDamage(bulletDamage);

            StartExplosion();
        }
        else
        {
            DisableBullet();
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        if (trailRenderer != null) trailRenderer.enabled = true;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (trailRenderer != null) trailRenderer.enabled = false;
    }

    void StartExplosion()
    {
        bounce.OnExplosionStart();
    }
}
