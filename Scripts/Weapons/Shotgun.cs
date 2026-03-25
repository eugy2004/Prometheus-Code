using UnityEngine;

public class Shotgun : BaseWeapon
{
    public override void SetDirection(Transform firePoint, Vector2 shootDirection)
    {
        int count = weaponStats.projectilesPerShot;
        float spread = weaponStats.projectileSpread;

        // distribuzione uniforme
        float step = spread / (count - 1);
        float startAngle = -spread * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + step * i;

            Vector2 dir = Quaternion.Euler(0f, 0f, angle) * shootDirection;

            base.SetDirection(firePoint, dir.normalized);
        }
    }

    public override void LevelUpWeapon()
    {
        level++;
        if (level <= 3)
        {
            weaponStats.projectilesPerShot += 2;
            weaponStats.projectileSpread += 5;
        }
        else
        {
            //pierce
        }
    }
}
