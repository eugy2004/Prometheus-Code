using UnityEngine;
using UnityEngine.Events;

public class BaseWeapon
{
    public WeaponGeneralStats weaponStats;

    protected int level = 1;

    private Vector3 direction;

    public virtual void InstantiateWeapon(WeaponGeneralStats stats)
    {
        weaponStats = stats;
    }

    public virtual void SetDirection(Transform firePoint, Vector2 shootDirection)
    {
        direction = Vector3.right * shootDirection.x + Vector3.forward * shootDirection.y;
        Shoot(firePoint, direction);
    }

    public virtual void Shoot(Transform firePoint, Vector3 direction)
    {
        GameObject availableBullet = BulletPoolingManager.instance.GetPooledPlayerBullet();

        availableBullet.GetComponent<Bullet>().BulletShoot(firePoint.position, direction, weaponStats.bulletSpeed, weaponStats.bulletLifeTime, weaponStats.damage, weaponStats.bulletSize , weaponStats.pushBackForce);    // Il proiettile ha la sua funzione per spararsi da solo
    }

    public virtual void LevelUpWeapon(){}
}