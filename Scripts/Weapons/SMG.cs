using UnityEngine;

public class SMG : BaseWeapon
{
    public override void SetDirection(Transform firePoint, Vector2 shootDirection)
    {
        Vector2 perpendicular = Vector2.Perpendicular(shootDirection).normalized;

        float spreadValue = Random.Range(-weaponStats.projectileSpread, weaponStats.projectileSpread);

        shootDirection += perpendicular * spreadValue * 0.01f;

        base.SetDirection(firePoint, shootDirection);
    }

    public override void LevelUpWeapon()
    {
        level++;
        if (level <= 3)
        {
            weaponStats.fireCooldown -= 0.09f;
            weaponStats.bulletSpeed += 3;
        }
        else
        {

        }
    }
}
