using UnityEngine;

public class EnemyBullet : Bullet
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out Player player))
        {
            player.OnPush();
            PushBack(collision.rigidbody);
            player.TakeDamage(bulletDamage);
            
        }

        gameObject.SetActive(false);
    }
}
