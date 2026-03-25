using UnityEngine;

public class Sniper : BaseWeapon
{
    public override void Shoot(Transform firePoint, Vector3 direction)
    {
        GameObject sniperBullet = BulletPoolingManager.instance.GetPooledSniperBullet();

        sniperBullet.GetComponent<Bullet>().BulletShoot(firePoint.position, direction, weaponStats.bulletSpeed, weaponStats.bulletLifeTime, weaponStats.damage, weaponStats.bulletSize, weaponStats.pushBackForce);
    }

    public override void LevelUpWeapon()
    {
        level++;
        if (level <= 3)
        {
            weaponStats.damage *= 2f;
            weaponStats.bulletSize += 1;
        }
        else
        {

        }
    }
}
