using UnityEngine;

public class TurretBullet : Bullet
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out Enemy component))
        {
            component.TakeDamage(bulletDamage);
        }
        DisableBullet();
    }
}
