using UnityEngine;

public class SpikedBallCompanion : Companion
{
    [SerializeField]
    private float damage;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out Enemy enemy))
        {
            enemy.TakeDamage(damage);
        }
    }
}
