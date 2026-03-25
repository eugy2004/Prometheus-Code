using UnityEngine;

public class ShieldCompanion : Companion
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out EnemyBullet bullet))
        {
            bullet.gameObject.SetActive(false);
        }
    }
}
