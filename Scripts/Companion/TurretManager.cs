using System.Collections.Generic;
using UnityEngine;

public class TurretManager : MonoBehaviour
{
    [SerializeField]
    private WeaponGeneralStats turretStats;

    [HideInInspector]
    public List<GameObject> turrets;

    private float cooldownTimer = 0f;
    private Vector2 lastValidDirection = Vector2.right;

    private Vector3 shootingDirection;

    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        bool isCheckingInputs = false;
        if (Player.instance)
        {
            isCheckingInputs = Player.instance.CheckingShootInputs;
        }

        // 🔹 ROTAZIONE SEMPRE AGGIORNATA
        UpdateRotation();
        UpdateShootDirection();

        if (!isCheckingInputs && cooldownTimer <= 0f && shootingDirection != Vector3.zero)
        {
            TurretsShoot();
        }
    }

    private void UpdateRotation()
    {
        Vector2 dir2D;

        if (Player.instance == null)
        {
            return;
        }

        if (!Player.instance.CheckingShootInputs)
        {
            dir2D = Player.instance.AimDirection;
        }

        else if (Player.instance.MoveInput != Vector2.zero)
        {
            dir2D = Player.instance.MoveInput.normalized;
        }

        else
        {
            dir2D = lastValidDirection;
        }

        if (dir2D == Vector2.zero)
            return;

        lastValidDirection = dir2D;

        Vector3 dir3D =
            (dir2D.x * Vector3.right +
             dir2D.y * Vector3.forward).normalized;

        foreach (GameObject turret in turrets)
        {
            Transform graphic = turret.GetComponentInChildren<Animator>().transform;

            Quaternion lookRot = Quaternion.LookRotation(dir3D, Vector3.up);

            Quaternion modelOffset = Quaternion.Euler(-90f, 0f, 0f);

            graphic.rotation = Quaternion.Slerp(
                graphic.rotation,
                lookRot * modelOffset,
                Time.deltaTime * 12f
            );
        }

    }

    private void UpdateShootDirection()
    {
        if (Player.instance == null)
        {
            return;
        }

        Vector2 aimDir = Player.instance.AimDirection;

        shootingDirection =
            (aimDir.x * Vector3.right +
             aimDir.y * Vector3.forward).normalized;
    }

    private void TurretsShoot()
    {
        cooldownTimer = turretStats.fireCooldown;
        Shoot(shootingDirection);
    }

    private void Shoot(Vector3 shootingDirection)
    {
        foreach (GameObject turret in turrets)
        {
            GameObject bullet =
                BulletPoolingManager.instance.GetPooledTurretBullet();

            bullet.GetComponent<Bullet>().BulletShoot(
                turret.transform.position,
                shootingDirection,
                turretStats.bulletSpeed,
                turretStats.bulletLifeTime,
                turretStats.damage,
                turretStats.bulletSize,
                turretStats.pushBackForce
            );
        }
    }
}
